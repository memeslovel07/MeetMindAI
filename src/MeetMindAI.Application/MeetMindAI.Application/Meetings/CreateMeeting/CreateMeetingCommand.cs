using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.CreateMeeting;

/// <summary>
/// Represents a request to create a new meeting.
/// </summary>
public sealed record CreateMeetingCommand(
    string Title,
    string? Description,
    DateTime? ScheduledAtUtc,
    int DurationMinutes)
    : IRequest<Result<CreateMeetingResponse>>;
