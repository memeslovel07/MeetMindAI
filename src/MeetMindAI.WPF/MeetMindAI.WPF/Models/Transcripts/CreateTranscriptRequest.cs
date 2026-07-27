namespace MeetMindAI.WPF.Models.Transcripts;

public sealed record CreateTranscriptRequest(
    string Content,
    string? Language,
    int? DurationSeconds);
