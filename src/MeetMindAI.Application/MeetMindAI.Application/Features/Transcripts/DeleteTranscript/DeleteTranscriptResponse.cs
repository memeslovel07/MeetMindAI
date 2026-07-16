namespace MeetMindAI.Application.Features.Transcripts.DeleteTranscript;

/// <summary>
/// Represents the response after deleting a transcript.
/// </summary>
public sealed record DeleteTranscriptResponse(
    Guid TranscriptId);
