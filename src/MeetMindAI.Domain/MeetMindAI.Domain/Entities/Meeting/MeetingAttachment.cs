using MeetMindAI.Domain.Common;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Entities.Meetings;

public sealed class MeetingAttachment : AuditableEntity
{
    private MeetingAttachment()
    {
    }

    private MeetingAttachment(
        Guid meetingId,
        string originalFileName,
        string storedFileName,
        string contentType,
        string extension,
        long sizeInBytes,
        AttachmentType attachmentType,
        string storageKey)
    {
        MeetingId = meetingId;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        ContentType = contentType;
        Extension = extension;
        SizeInBytes = sizeInBytes;
        AttachmentType = attachmentType;
        StorageKey = storageKey;
    }

    public Guid MeetingId { get; private set; }

    public Meeting Meeting { get; private set; } = null!;

    public string OriginalFileName { get; private set; } = string.Empty;

    public string StoredFileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public string Extension { get; private set; } = string.Empty;

    public long SizeInBytes { get; private set; }

    public AttachmentType AttachmentType { get; private set; }

    public string StorageKey { get; private set; } = string.Empty;

    public static Result<MeetingAttachment> Create(
        Guid meetingId,
        string originalFileName,
        string storedFileName,
        string contentType,
        string extension,
        long sizeInBytes,
        AttachmentType attachmentType,
        string storageKey)
    {
        if (meetingId == Guid.Empty)
            return Result<MeetingAttachment>.Failure(
                MeetingAttachmentErrors.InvalidMeetingId);

        if (string.IsNullOrWhiteSpace(originalFileName))
            return Result<MeetingAttachment>.Failure(
                MeetingAttachmentErrors.OriginalFileNameRequired);

        if (string.IsNullOrWhiteSpace(storedFileName))
            return Result<MeetingAttachment>.Failure(
                MeetingAttachmentErrors.StoredFileNameRequired);

        if (string.IsNullOrWhiteSpace(storageKey))
            return Result<MeetingAttachment>.Failure(
                MeetingAttachmentErrors.StorageKeyRequired);

        if (sizeInBytes <= 0)
            return Result<MeetingAttachment>.Failure(
                MeetingAttachmentErrors.InvalidFileSize);

        var attachment = new MeetingAttachment(
            meetingId,
            originalFileName.Trim(),
            storedFileName.Trim(),
            contentType.Trim(),
            extension.Trim().ToLowerInvariant(),
            sizeInBytes,
            attachmentType,
            storageKey.Trim());

        return Result<MeetingAttachment>.Success(
            attachment);
    }
}
