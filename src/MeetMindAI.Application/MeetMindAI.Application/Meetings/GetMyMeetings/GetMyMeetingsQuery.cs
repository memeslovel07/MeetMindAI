using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.GetMyMeetings;

/// <summary>
/// Represents a request to retrieve the current user's meetings.
/// </summary>
public sealed record GetMyMeetingsQuery()
    : IRequest<Result<IReadOnlyList<GetMyMeetingsResponse>>>;
