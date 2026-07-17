using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Authentication.RefreshToken;

/// <summary>
/// Handles refresh token requests.
/// </summary>
public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(refreshTokenRepository);
        ArgumentNullException.ThrowIfNull(jwtTokenGenerator);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var existingRefreshToken =
            await _refreshTokenRepository.GetByTokenAsync(
                request.RefreshToken,
                cancellationToken);

        if (existingRefreshToken is null)
        {
            return Result<RefreshTokenResponse>.Failure(
                UserErrors.InvalidRefreshToken);
        }

        if (!existingRefreshToken.IsActive(_dateTimeProvider.UtcNow))
        {
            return Result<RefreshTokenResponse>.Failure(
                UserErrors.InvalidRefreshToken);
        }

        var user = existingRefreshToken.User;

        var tokens = _jwtTokenGenerator.Generate(user);

        var newRefreshToken = existingRefreshToken.Rotate(
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAtUtc);

        await _refreshTokenRepository.AddAsync(
            newRefreshToken,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.AccessTokenExpiresAtUtc));
    }
}
