using MeetMindAI.WPF.Models.MeetingAttachments;

namespace MeetMindAI.WPF.Services.MeetingAttachments;

public interface IMeetingAttachmentApiService
{
    Task<IReadOnlyList<MeetingAttachmentItem>> GetAllAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);

    Task<UploadAttachmentResponse> UploadAsync(
        Guid meetingId,
        string filePath,
        CancellationToken cancellationToken = default);

    Task<DownloadedAttachment> DownloadAsync(
        Guid meetingId,
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid meetingId,
        Guid attachmentId,
        CancellationToken cancellationToken = default);
}
