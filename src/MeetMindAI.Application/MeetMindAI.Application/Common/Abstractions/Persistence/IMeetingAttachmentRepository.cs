using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Application.Common.Interfaces.Persistence;

public interface IMeetingAttachmentRepository
{
    Task AddAsync(MeetingAttachment attachment, CancellationToken cancellationToken = default);

    Task<MeetingAttachment?> GetByIdAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MeetingAttachment>> GetByMeetingIdAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);

    void Remove(MeetingAttachment attachment);
}
