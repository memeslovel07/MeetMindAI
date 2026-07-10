using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Authentication.Logout;

/// <summary>
/// Handles logout requests by revoking a refresh token.
/// </summary>
public sealed class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IApplicationDbContext _dbContext;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(refreshTokenRepository);
        ArgumentNullException.ThrowIfNull(dbContext);

        _refreshTokenRepository = refreshTokenRepository;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
            request.RefreshToken,
            cancellationToken);

        // Idempotent logout.
        if (refreshToken is null)
        {
            return Result.Success();
        }

        if (refreshToken.IsRevoked)
        {
            return Result.Success();
        }

        refreshToken.Revoke(
            RefreshTokenRevocationReason.UserLogout);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
