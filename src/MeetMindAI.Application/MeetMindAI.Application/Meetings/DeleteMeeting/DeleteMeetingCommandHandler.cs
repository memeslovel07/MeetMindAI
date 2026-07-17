using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.DeleteMeeting;

/// <summary>
/// Handles <see cref="DeleteMeetingCommand"/> requests.
/// </summary>
public sealed class DeleteMeetingCommandHandler
    : IRequestHandler<DeleteMeetingCommand, Result<DeleteMeetingResponse>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteMeetingCommandHandler(
        IMeetingRepository meetingRepository,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(currentUserService);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        _meetingRepository = meetingRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<DeleteMeetingResponse>> Handle(
        DeleteMeetingCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<DeleteMeetingResponse>.Failure(
                MeetingErrors.NotFound);
        }

        if (_currentUserService.UserId != meeting.OrganizerId)
        {
            return Result<DeleteMeetingResponse>.Failure(
                Error.Forbidden);
        }

        var deleteResult = meeting.Delete(
            _currentUserService.UserId,
            _dateTimeProvider.UtcNow);

        if (deleteResult.IsFailure)
        {
            return Result<DeleteMeetingResponse>.Failure(
                deleteResult.Error);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<DeleteMeetingResponse>.Success(
            new DeleteMeetingResponse(meeting.Id));
    }
}
