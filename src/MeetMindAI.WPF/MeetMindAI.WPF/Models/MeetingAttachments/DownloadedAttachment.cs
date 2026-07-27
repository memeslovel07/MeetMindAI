namespace MeetMindAI.WPF.Models.MeetingAttachments;

public sealed record DownloadedAttachment(
    string FileName,
    string ContentType,
    byte[] Content);
