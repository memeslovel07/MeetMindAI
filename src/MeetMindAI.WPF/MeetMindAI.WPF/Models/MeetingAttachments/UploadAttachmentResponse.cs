namespace MeetMindAI.WPF.Models.MeetingAttachments;

public sealed record UploadAttachmentResponse(
    Guid Id,
    Guid MeetingId,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string AttachmentType,
    DateTime UploadedAt);
