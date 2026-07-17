namespace MeetMindAI.Application.Features.Transcripts.UpdateTranscript;

/// <summary>
/// Represents the response after updating a transcript.
/// </summary>
public sealed record UpdateTranscriptResponse(
    Guid TranscriptId);
