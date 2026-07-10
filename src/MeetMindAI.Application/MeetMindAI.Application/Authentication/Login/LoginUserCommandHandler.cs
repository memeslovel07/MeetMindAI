using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;
using MeetMindAI.Application.Authentication.RefreshToken;
namespace MeetMindAI.Application.Authentication.Login;

/// <summary>
/// Handles <see cref="LoginUserCommand"/> requests.
/// </summary>
public sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(refreshTokenRepository);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(jwtTokenGenerator);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

   
        public async Task<Result<LoginUserResponse>> Handle(
    LoginUserCommand request,
    CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (user is null)
        {
            return Result<LoginUserResponse>.Failure(
                UserErrors.InvalidCredentials);
        }

        var verificationResult = _passwordHasher.Verify(
            request.Password,
            user.PasswordHash);

        if (!verificationResult.Succeeded)
        {
            return Result<LoginUserResponse>.Failure(
                UserErrors.InvalidCredentials);
        }

        var tokens = _jwtTokenGenerator.Generate(user);

        var refreshToken = Domain.Entities.Users.RefreshToken.Create(
            user.Id,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAtUtc);

        await _refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);

        user.UpdateLastLogin(_dateTimeProvider.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<LoginUserResponse>.Success(
            new LoginUserResponse(
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.AccessTokenExpiresAtUtc));
    }
}
