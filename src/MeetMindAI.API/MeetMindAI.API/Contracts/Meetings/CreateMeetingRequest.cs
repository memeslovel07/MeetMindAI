namespace MeetMindAI.API.Contracts.Meetings;

/// <summary>
/// Represents a request to create a meeting.
/// </summary>
public sealed record CreateMeetingRequest(
    string Title,
    string? Description,
    DateTime? ScheduledAtUtc,
    int DurationMinutes);
