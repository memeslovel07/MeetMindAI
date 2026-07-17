using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Application.Common.Interfaces.Persistence;

public interface IMeetingSummaryRepository
{
    Task AddAsync(
        MeetingSummary summary,
        CancellationToken cancellationToken = default);

    Task<MeetingSummary?> GetByMeetingIdAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);

    void Update(
        MeetingSummary summary);

    void Remove(
        MeetingSummary summary);
}
