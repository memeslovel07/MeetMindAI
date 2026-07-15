using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Shared.Results;
using MeetMindAI.Domain.Errors;

namespace MeetMindAI.Application.Meetings.GetMeeting;

/// <summary>
/// Handles <see cref="GetMeetingQuery"/> requests.
/// </summary>
public sealed class GetMeetingQueryHandler
    : IRequestHandler<GetMeetingQuery, Result<GetMeetingResponse>>
{
    private readonly IMeetingRepository _meetingRepository;

    public GetMeetingQueryHandler(
        IMeetingRepository meetingRepository)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);

        _meetingRepository = meetingRepository;
    }

    public async Task<Result<GetMeetingResponse>> Handle(
        GetMeetingQuery request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<GetMeetingResponse>.Failure(
                MeetingErrors.NotFound);
        }

        return Result<GetMeetingResponse>.Success(
            new GetMeetingResponse(
                meeting.Id,
                meeting.Title,
                meeting.Description,
                meeting.OrganizerId,
                meeting.ScheduledAtUtc,
                meeting.DurationMinutes,
                meeting.Status));
    }
}
