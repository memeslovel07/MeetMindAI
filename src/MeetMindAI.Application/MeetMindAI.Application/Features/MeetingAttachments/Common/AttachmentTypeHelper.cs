using MeetMindAI.Domain.Enums;

namespace MeetMindAI.Application.Common.Helpers;

public static class AttachmentTypeHelper
{
    public static AttachmentType GetAttachmentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return AttachmentType.Other;

        if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Audio;

        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Video;

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Image;

        if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Document;

        if (contentType.Contains("word", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Document;

        if (contentType.Contains("excel", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Document;

        if (contentType.Contains("presentation", StringComparison.OrdinalIgnoreCase))
            return AttachmentType.Document;

        return AttachmentType.Other;
    }
}
