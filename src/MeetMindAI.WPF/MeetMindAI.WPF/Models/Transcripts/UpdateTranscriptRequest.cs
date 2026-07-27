namespace MeetMindAI.WPF.Models.Transcripts;

public sealed record UpdateTranscriptRequest(
    string Content,
    string? Language,
    int? DurationSeconds);
