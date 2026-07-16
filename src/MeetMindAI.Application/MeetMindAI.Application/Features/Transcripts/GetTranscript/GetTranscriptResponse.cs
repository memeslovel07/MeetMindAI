namespace MeetMindAI.Application.Features.Transcripts.GetTranscript;

/// <summary>
/// Represents transcript details.
/// </summary>
public sealed record GetTranscriptResponse(
    Guid TranscriptId,
    Guid MeetingId,
    string Content,
    string? Language,
    TimeSpan? Duration);
