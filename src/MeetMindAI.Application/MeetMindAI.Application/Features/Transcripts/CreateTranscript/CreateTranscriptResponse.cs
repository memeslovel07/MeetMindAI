namespace MeetMindAI.Application.Features.Transcripts.CreateTranscript;

/// <summary>
/// Represents the response returned after creating a transcript.
/// </summary>
public sealed record CreateTranscriptResponse(
    Guid TranscriptId);
