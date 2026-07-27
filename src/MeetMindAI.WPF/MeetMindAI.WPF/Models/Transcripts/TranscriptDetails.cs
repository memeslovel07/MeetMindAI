namespace MeetMindAI.WPF.Models.Transcripts;

public sealed record TranscriptDetails(
    Guid TranscriptId,
    Guid MeetingId,
    string Content,
    string? Language,
    TimeSpan? Duration);
