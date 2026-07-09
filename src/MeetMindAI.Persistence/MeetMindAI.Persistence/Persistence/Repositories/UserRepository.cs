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
/// Provides persistence operations for <see cref="User"/> entities.
/// </summary>
public sealed class UserRepository
    : Repository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    public UserRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(normalizedEmail);

        normalizedEmail = normalizedEmail
    .Trim()
    .ToUpperInvariant();

        return await Context.Users
            .SingleOrDefaultAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(normalizedEmail);

        return await Context.Users
            .AnyAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }
}
