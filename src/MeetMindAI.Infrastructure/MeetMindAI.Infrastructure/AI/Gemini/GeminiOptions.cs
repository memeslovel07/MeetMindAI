namespace MeetMindAI.Infrastructure.AI.Gemini;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = "gemini-3.5-flash";
}
