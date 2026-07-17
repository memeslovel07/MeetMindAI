using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Domain.Entities.Users;

namespace MeetMindAI.Persistence.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for <see cref="RefreshToken"/> entities.
/// </summary>
public sealed class RefreshTokenRepository
    : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(
      string token,
      CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        token = token.Trim();

        return await Context.RefreshTokens
    .Include(x => x.User)
    .SingleOrDefaultAsync(
        x => x.Token == token,
        cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.RefreshTokens
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await Context.RefreshTokens
            .Where(x =>
                x.UserId == userId &&
                x.RevokedAtUtc == null &&
                x.ExpiresAtUtc > utcNow)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
