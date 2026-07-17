using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Application.Common.Interfaces.Persistence;

public interface ITranscriptRepository
{
    Task AddAsync(Transcript transcript, CancellationToken cancellationToken = default);

    Task<Transcript?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Transcript?> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default);

    void Update(Transcript transcript);

    void Remove(Transcript transcript);
}
