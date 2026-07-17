namespace MeetMindAI.API.Contracts.Transcripts;

/// <summary>
/// Represents a request to update a transcript.
/// </summary>
public sealed record UpdateTranscriptRequest(
    string Content,
    string? Language,
    int? DurationSeconds);
