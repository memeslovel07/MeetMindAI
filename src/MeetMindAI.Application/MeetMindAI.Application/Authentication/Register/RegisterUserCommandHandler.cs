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

namespace MeetMindAI.Application.Authentication.Register;

/// <summary>
/// Handles <see cref="RegisterUserCommand"/> requests.
/// </summary>
public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Check for duplicate email
        if (await _userRepository.ExistsByEmailAsync(
                request.Email,
                cancellationToken))
        {
            return Result<RegisterUserResponse>.Failure(
        UserErrors.EmailAlreadyExists);
        }

            // Hash the password
            var passwordHash = _passwordHasher.Hash(request.Password);



            var createUserResult = User.Create(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName);

            if (createUserResult.IsFailure)
            {
                return Result<RegisterUserResponse>.Failure(
                    createUserResult.Error);
            }

            var user = createUserResult.Value;

            await _userRepository.AddAsync(
                user,
                cancellationToken);



            await _dbContext.SaveChangesAsync(cancellationToken);

            // Return response
            return Result<RegisterUserResponse>.Success(
                new RegisterUserResponse(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email));
        }
    }

