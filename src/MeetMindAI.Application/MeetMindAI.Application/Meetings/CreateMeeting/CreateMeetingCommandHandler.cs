using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Meetings.CreateMeeting;

/// <summary>
/// Handles <see cref="CreateMeetingCommand"/> requests.
/// </summary>
public sealed class CreateMeetingCommandHandler
    : IRequestHandler<CreateMeetingCommand, Result<CreateMeetingResponse>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateMeetingCommandHandler(
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

    public async Task<Result<CreateMeetingResponse>> Handle(
        CreateMeetingCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid organizerId)
        {
            return Result<CreateMeetingResponse>.Failure(
                Error.Unauthorized);
        }

        var meetingResult = Meeting.Create(
            request.Title,
            request.Description,
            organizerId,
            request.ScheduledAtUtc,
            request.DurationMinutes);

        if (meetingResult.IsFailure)
        {
            return Result<CreateMeetingResponse>.Failure(
                meetingResult.Error);
        }

        var meeting = meetingResult.Value;

        await _meetingRepository.AddAsync(
            meeting,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<CreateMeetingResponse>.Success(
            new CreateMeetingResponse(
                meeting.Id));
    }
}
