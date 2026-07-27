namespace MeetMindAI.WPF.Models.Meetings;

public sealed record CreateMeetingRequest(
    string Title,
    string? Description,
    DateTime? ScheduledAtUtc,
    int DurationMinutes);
