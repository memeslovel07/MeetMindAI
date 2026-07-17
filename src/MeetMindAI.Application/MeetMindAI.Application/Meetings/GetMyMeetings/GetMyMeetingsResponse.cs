using MeetMindAI.Domain.Enums;

namespace MeetMindAI.Application.Meetings.GetMyMeetings;

/// <summary>
/// Represents a meeting card displayed to the current user.
/// </summary>
public sealed record GetMyMeetingsResponse(
    Guid Id,
    string Title,
    DateTime? ScheduledAtUtc,
    int DurationMinutes,
    MeetingStatus Status);
