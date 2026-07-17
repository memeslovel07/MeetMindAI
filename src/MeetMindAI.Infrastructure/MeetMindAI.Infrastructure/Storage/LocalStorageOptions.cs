namespace MeetMindAI.Infrastructure.Storage;

public sealed class LocalStorageOptions
{
    public const string SectionName = "LocalStorage";

    public string RootPath { get; set; } = "Storage";
}
