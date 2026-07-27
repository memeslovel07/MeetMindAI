namespace MeetMindAI.WPF.Models.MeetingAttachments;

public sealed record MeetingAttachmentItem(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    string AttachmentType,
    DateTime CreatedAtUtc)
{
    public DateTime CreatedAtLocal =>
        CreatedAtUtc.ToLocalTime();

    public string DisplaySize =>
        FormatFileSize(SizeInBytes);

    private static string FormatFileSize(
        long bytes)
    {
        const double kb = 1024;
        const double mb = kb * 1024;
        const double gb = mb * 1024;

        if (bytes >= gb)
        {
            return $"{bytes / gb:0.##} GB";
        }

        if (bytes >= mb)
        {
            return $"{bytes / mb:0.##} MB";
        }

        if (bytes >= kb)
        {
            return $"{bytes / kb:0.##} KB";
        }

        return $"{bytes} B";
    }
}
