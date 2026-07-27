namespace MeetMindAI.WPF.Models.Meetings;

public sealed record MeetingDetails(
    Guid Id,
    string Title,
    string? Description,
    Guid OrganizerId,
    DateTime? ScheduledAtUtc,
    int DurationMinutes,
    MeetingStatus Status)
{
    public DateTime? ScheduledAtLocal =>
        ScheduledAtUtc?.ToLocalTime();
}
