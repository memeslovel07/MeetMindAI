using MediatR;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.Transcripts.UpdateTranscript;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Transcripts.UpdateTranscript;

/// <summary>
/// Handles <see cref="UpdateTranscriptCommand"/> requests.
/// </summary>
public sealed class UpdateTranscriptCommandHandler
    : IRequestHandler<UpdateTranscriptCommand, Result<UpdateTranscriptResponse>>
{
    private readonly ITranscriptRepository _transcriptRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMeetingRepository _meetingRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTranscriptCommandHandler(
    ITranscriptRepository transcriptRepository,
    IMeetingRepository meetingRepository,
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService)
    {
        ArgumentNullException.ThrowIfNull(transcriptRepository);
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(currentUserService);

        _transcriptRepository = transcriptRepository;
        _meetingRepository = meetingRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }



    public async Task<Result<UpdateTranscriptResponse>> Handle(
     UpdateTranscriptCommand request,
     CancellationToken cancellationToken)
    {
        // 1. Authentication
        if (_currentUserService.UserId is not Guid currentUserId)
        {
            return Result<UpdateTranscriptResponse>.Failure(
                Error.Unauthorized);
        }

        // 2. Meeting existence
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<UpdateTranscriptResponse>.Failure(
                MeetingErrors.NotFound);
        }

        // 3. Ownership
        if (meeting.OrganizerId != currentUserId)
        {
            return Result<UpdateTranscriptResponse>.Failure(
                Error.Forbidden);
        }

        // 4. Only now retrieve transcript
        var transcript = await _transcriptRepository.GetByMeetingIdAsync(
            request.MeetingId,
            cancellationToken);

        if (transcript is null)
        {
            return Result<UpdateTranscriptResponse>.Failure(
                TranscriptErrors.NotFound);
        }

        var updateResult = transcript.UpdateContent(
            request.Content,
            request.Language,
            request.Duration);

        if (updateResult.IsFailure)
        {
            return Result<UpdateTranscriptResponse>.Failure(
                updateResult.Error);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<UpdateTranscriptResponse>.Success(
            new UpdateTranscriptResponse(
                transcript.Id));
    }
}
