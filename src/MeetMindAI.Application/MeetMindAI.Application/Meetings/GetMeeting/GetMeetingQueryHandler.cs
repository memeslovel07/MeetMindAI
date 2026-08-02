using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.GetMeeting;

/// <summary>
/// Handles <see cref="GetMeetingQuery"/> requests.
/// </summary>
public sealed class GetMeetingQueryHandler
    : IRequestHandler<GetMeetingQuery, Result<GetMeetingResponse>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMeetingQueryHandler(
        IMeetingRepository meetingRepository,
        ICurrentUserService currentUserService)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(currentUserService);

        _meetingRepository = meetingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetMeetingResponse>> Handle(
        GetMeetingQuery request,
        CancellationToken cancellationToken)
    {
        // Verify authenticated user.
        if (_currentUserService.UserId is not Guid currentUserId)
        {
            return Result<GetMeetingResponse>.Failure(
                Error.Unauthorized);
        }

        var meeting =
            await _meetingRepository.GetByIdAsync(
                request.MeetingId,
                cancellationToken);

        if (meeting is null)
        {
            return Result<GetMeetingResponse>.Failure(
                MeetingErrors.NotFound);
        }

        // Verify meeting ownership.
        if (meeting.OrganizerId != currentUserId)
        {
            return Result<GetMeetingResponse>.Failure(
                Error.Forbidden);
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
