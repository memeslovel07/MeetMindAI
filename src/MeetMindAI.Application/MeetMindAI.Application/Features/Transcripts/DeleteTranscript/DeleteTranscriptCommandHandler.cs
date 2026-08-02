using MediatR;

using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.Transcripts.DeleteTranscript;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Transcripts.DeleteTranscript;

/// <summary>
/// Handles <see cref="DeleteTranscriptCommand"/> requests.
/// </summary>
public sealed class DeleteTranscriptCommandHandler
    : IRequestHandler<DeleteTranscriptCommand, Result<DeleteTranscriptResponse>>
{
    private readonly ITranscriptRepository _transcriptRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMeetingRepository _meetingRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTranscriptCommandHandler(
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

    public async Task<Result<DeleteTranscriptResponse>> Handle(
        DeleteTranscriptCommand request,
        CancellationToken cancellationToken)
    {

        if (_currentUserService.UserId is not Guid currentUserId)
        {
            return Result<DeleteTranscriptResponse>.Failure(
                Error.Unauthorized);
        }

        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<DeleteTranscriptResponse>.Failure(
                MeetingErrors.NotFound);
        }

        if (meeting.OrganizerId != currentUserId)
        {
            return Result<DeleteTranscriptResponse>.Failure(
                Error.Forbidden);
        }

        var transcript = await _transcriptRepository.GetByMeetingIdAsync(
            request.MeetingId,
            cancellationToken);

        if (transcript is null)
        {
            return Result<DeleteTranscriptResponse>.Failure(
                TranscriptErrors.NotFound);
        }

        _transcriptRepository.Remove(transcript);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<DeleteTranscriptResponse>.Success(
            new DeleteTranscriptResponse(
                transcript.Id));
    }
}
