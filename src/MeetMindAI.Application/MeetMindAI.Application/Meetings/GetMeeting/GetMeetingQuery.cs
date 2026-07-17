using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.GetMeeting;

/// <summary>
/// Represents a request to retrieve a meeting.
/// </summary>
public sealed record GetMeetingQuery(
    Guid MeetingId)
    : IRequest<Result<GetMeetingResponse>>;
