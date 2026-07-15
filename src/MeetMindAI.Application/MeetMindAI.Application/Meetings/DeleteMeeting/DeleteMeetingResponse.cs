namespace MeetMindAI.Application.Meetings.DeleteMeeting;

/// <summary>
/// Represents the response after deleting a meeting.
/// </summary>
public sealed record DeleteMeetingResponse(
    Guid MeetingId);
