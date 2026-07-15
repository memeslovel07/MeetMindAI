using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.UpdateMeeting;

/// <summary>
/// Represents a request to update a meeting.
/// </summary>
public sealed record UpdateMeetingCommand(
    Guid MeetingId,
    string Title,
    string? Description,
    DateTime? ScheduledAtUtc,
    int DurationMinutes)
    : IRequest<Result<UpdateMeetingResponse>>;
