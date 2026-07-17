namespace MeetMindAI.Application.Common.Options;

/// <summary>
/// Configuration options for AI providers.
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string PromptVersion { get; init; } = "v1";
}
