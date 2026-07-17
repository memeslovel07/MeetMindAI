using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;


using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Users.GetCurrentUser;

/// <summary>
/// Handles requests for the currently authenticated user.
/// </summary>
public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, Result<GetCurrentUserResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        ArgumentNullException.ThrowIfNull(currentUserService);
        ArgumentNullException.ThrowIfNull(userRepository);

        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<Result<GetCurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated ||
            _currentUserService.UserId is null)
        {
            return Result<GetCurrentUserResponse>.Failure(
                UserErrors.InvalidCredentials);
        }

        var user = await _userRepository.GetByIdAsync(
            _currentUserService.UserId.Value,
            cancellationToken);

        if (user is null)
        {
            return Result<GetCurrentUserResponse>.Failure(
                UserErrors.UserNotFound);
        }

        return Result<GetCurrentUserResponse>.Success(
            new GetCurrentUserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role.ToString(),
                user.EmailConfirmed,
                user.LastLoginAtUtc));
    }
}
