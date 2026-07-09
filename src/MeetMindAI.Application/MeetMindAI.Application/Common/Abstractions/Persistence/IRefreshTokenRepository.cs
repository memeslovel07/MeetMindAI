using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Domain.Entities.Users;

namespace MeetMindAI.Application.Common.Abstractions.Persistence;

/// <summary>
/// Defines persistence operations for the <see cref="RefreshToken"/> aggregate.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Gets a refresh token by its token value.
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all refresh tokens belonging to the specified user.
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active refresh tokens belonging to the specified user.
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
