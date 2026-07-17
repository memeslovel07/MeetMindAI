namespace MeetMindAI.API.Contracts.Meetings;

/// <summary>
/// Represents a request to update a meeting.
/// </summary>
public sealed record UpdateMeetingRequest(
    string Title,
    string? Description,
    DateTime? ScheduledAtUtc,
    int DurationMinutes);
