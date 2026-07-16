namespace MeetMindAI.API.Contracts.Transcripts;

/// <summary>
/// Represents a request to create a transcript.
/// </summary>
public sealed record CreateTranscriptRequest(
    string Content,
    string? Language,
    int? DurationSeconds);
