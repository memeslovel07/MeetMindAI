namespace MeetMindAI.Application.Meetings.CreateMeeting;

/// <summary>
/// Represents the response returned after creating a meeting.
/// </summary>
/// <param name="MeetingId">
/// The identifier of the newly created meeting.
/// </param>
public sealed record CreateMeetingResponse(
    Guid MeetingId);
