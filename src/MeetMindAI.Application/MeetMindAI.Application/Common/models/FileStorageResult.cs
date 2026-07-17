namespace MeetMindAI.Application.Common.Models;

public sealed class FileStorageResult
{
    public string OriginalFileName { get; init; } = string.Empty;

    public string StoredFileName { get; init; } = string.Empty;

    public string StorageKey { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public string Extension { get; init; } = string.Empty;

    public long SizeInBytes { get; init; }
}
