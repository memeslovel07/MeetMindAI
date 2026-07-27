using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Application.Common.Interfaces.Persistence;

public interface IActionItemRepository
{
    Task AddAsync(
        ActionItem actionItem,
        CancellationToken cancellationToken = default);

    Task<ActionItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActionItem>> GetByMeetingIdAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);

    void Update(ActionItem actionItem);

    void Remove(ActionItem actionItem);
}
