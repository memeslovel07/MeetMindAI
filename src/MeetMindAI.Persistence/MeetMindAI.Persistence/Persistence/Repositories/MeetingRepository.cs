using Microsoft.EntityFrameworkCore;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Persistence.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for <see cref="Meeting"/> entities.
/// </summary>
public sealed class MeetingRepository
    : Repository<Meeting>, IMeetingRepository
{
    public MeetingRepository(
        ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<Meeting?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Context.Meetings
    .SingleOrDefaultAsync(
        x => x.Id == id && !x.IsDeleted,
        cancellationToken);
    }

    public async Task<IReadOnlyList<Meeting>> GetByOrganizerIdAsync(
        Guid organizerId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Meetings
            .Where(x => x.OrganizerId == organizerId &&!x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
