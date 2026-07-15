using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.UpdateMeeting;

/// <summary>
/// Handles <see cref="UpdateMeetingCommand"/> requests.
/// </summary>
public sealed class UpdateMeetingCommandHandler
    : IRequestHandler<UpdateMeetingCommand, Result<UpdateMeetingResponse>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMeetingCommandHandler(
        IMeetingRepository meetingRepository,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(currentUserService);

        _meetingRepository = meetingRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UpdateMeetingResponse>> Handle(
        UpdateMeetingCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<UpdateMeetingResponse>.Failure(
                MeetingErrors.NotFound);
        }

        if (_currentUserService.UserId != meeting.OrganizerId)
        {
            return Result<UpdateMeetingResponse>.Failure(
                Error.Forbidden);
        }

        var updateResult = meeting.Update(
            request.Title,
            request.Description,
            request.ScheduledAtUtc,
            request.DurationMinutes);

        if (updateResult.IsFailure)
        {
            return Result<UpdateMeetingResponse>.Failure(
                updateResult.Error);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<UpdateMeetingResponse>.Success(
            new UpdateMeetingResponse(
                meeting.Id));
    }
}
