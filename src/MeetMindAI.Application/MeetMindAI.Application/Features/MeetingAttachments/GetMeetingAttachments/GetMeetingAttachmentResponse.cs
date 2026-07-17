namespace MeetMindAI.Application.Features.MeetingAttachments.GetMeetingAttachments;

public sealed record GetMeetingAttachmentResponse(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string AttachmentType,
    DateTime CreatedAtUtc);
