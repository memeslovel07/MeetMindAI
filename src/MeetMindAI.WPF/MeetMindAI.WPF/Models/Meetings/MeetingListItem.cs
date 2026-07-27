namespace MeetMindAI.WPF.Models.Meetings;

public sealed record MeetingListItem(
    Guid Id,
    string Title,
    DateTime? ScheduledAtUtc,
    int DurationMinutes,
    MeetingStatus Status)
{
    public DateTime? ScheduledAtLocal =>
        ScheduledAtUtc?.ToLocalTime();
}
