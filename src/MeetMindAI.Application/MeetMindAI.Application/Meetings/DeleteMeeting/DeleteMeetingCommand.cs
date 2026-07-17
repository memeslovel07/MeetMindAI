using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.DeleteMeeting;

/// <summary>
/// Represents a request to delete a meeting.
/// </summary>
public sealed record DeleteMeetingCommand(
    Guid MeetingId)
    : IRequest<Result<DeleteMeetingResponse>>;
