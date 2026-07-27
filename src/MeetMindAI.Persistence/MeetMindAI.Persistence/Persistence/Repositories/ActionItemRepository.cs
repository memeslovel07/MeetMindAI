using Microsoft.EntityFrameworkCore;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Persistence.Persistence.Repositories;

/// <summary>
/// Provides persistence operations for <see cref="ActionItem"/> entities.
/// </summary>
public sealed class ActionItemRepository
    : Repository<ActionItem>, IActionItemRepository
{
    public ActionItemRepository(
        ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<ActionItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Context.ActionItems
            .SingleOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ActionItem>> GetByMeetingIdAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        return await Context.ActionItems
            .Where(x =>
                x.MeetingId == meetingId &&
                !x.IsDeleted)
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
