using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.GetMyMeetings;

/// <summary>
/// Handles <see cref="GetMyMeetingsQuery"/> requests.
/// </summary>
public sealed class GetMyMeetingsQueryHandler
    : IRequestHandler<
        GetMyMeetingsQuery,
        Result<IReadOnlyList<GetMyMeetingsResponse>>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyMeetingsQueryHandler(
        IMeetingRepository meetingRepository,
        ICurrentUserService currentUserService)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(currentUserService);

        _meetingRepository = meetingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<GetMyMeetingsResponse>>> Handle(
        GetMyMeetingsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid organizerId)
        {
            return Result<IReadOnlyList<GetMyMeetingsResponse>>
                .Failure(Error.Unauthorized);
        }

        var meetings = await _meetingRepository.GetByOrganizerIdAsync(
            organizerId,
            cancellationToken);

        var response = meetings
            .Select(meeting => new GetMyMeetingsResponse(
                meeting.Id,
                meeting.Title,
                meeting.ScheduledAtUtc,
                meeting.DurationMinutes,
                meeting.Status))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<GetMyMeetingsResponse>>
            .Success(response);
    }
}
