namespace MeetMindAI.Application.Meetings.UpdateMeeting;

/// <summary>
/// Represents the response after updating a meeting.
/// </summary>
public sealed record UpdateMeetingResponse(
    Guid MeetingId);
