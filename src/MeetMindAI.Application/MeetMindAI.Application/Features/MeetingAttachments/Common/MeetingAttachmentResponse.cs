namespace MeetMindAI.Application.Features.MeetingAttachments.Common;

public sealed record MeetingAttachmentResponse(
    Guid Id,
    Guid MeetingId,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string AttachmentType,
    DateTime UploadedAt);
