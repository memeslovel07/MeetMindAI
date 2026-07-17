using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Errors;

public static class MeetingAttachmentErrors
{
    public static readonly Error InvalidMeetingId =
        new(
            "MeetingAttachment.InvalidMeetingId",
            "Meeting id is required.");

    public static readonly Error OriginalFileNameRequired =
        new(
            "MeetingAttachment.OriginalFileNameRequired",
            "Original file name is required.");

    public static readonly Error StoredFileNameRequired =
        new(
            "MeetingAttachment.StoredFileNameRequired",
            "Stored file name is required.");

    public static readonly Error StorageKeyRequired =
        new(
            "MeetingAttachment.StorageKeyRequired",
            "Storage key is required.");

    public static readonly Error InvalidFileSize =
        new(
            "MeetingAttachment.InvalidFileSize",
            "File size must be greater than zero.");

    public static readonly Error NotFound =
        new(
            "MeetingAttachment.NotFound",
            "Meeting attachment not found.");

    public static readonly Error FileStorageError =
        new(
            "MeetingAttachment.FileStorageError",
            "An error occurred while accessing the file storage.");
    public static readonly Error FileDownloadError =
        new(
            "MeetingAttachment.FileDownloadError",
            "An error occurred while downloading the file.");

     public static readonly Error FileUploadError =
        new(
            "MeetingAttachment.FileUploadError",
            "An error occurred while uploading the file.");

    public static readonly Error FileNotFound =
        new(
            "MeetingAttachment.FileDeleteError",
            "An error occurred while deleting the file.");
}
