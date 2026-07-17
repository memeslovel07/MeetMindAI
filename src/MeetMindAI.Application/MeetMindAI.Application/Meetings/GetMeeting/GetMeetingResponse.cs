namespace MeetMindAI.Application.Meetings.GetMeeting;

using MeetMindAI.Domain.Enums;

/// <summary>
/// Represents a meeting returned to the client.
/// </summary>
public sealed record GetMeetingResponse(
    Guid Id,
    string Title,
    string? Description,
    Guid OrganizerId,
    DateTime? ScheduledAtUtc,
    int DurationMinutes,
    MeetingStatus Status);
