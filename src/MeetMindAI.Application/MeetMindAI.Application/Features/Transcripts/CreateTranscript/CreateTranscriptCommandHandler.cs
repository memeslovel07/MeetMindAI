using MediatR;

using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.Transcripts.CreateTranscript;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.Transcripts.CreateTranscript;

/// <summary>
/// Handles <see cref="CreateTranscriptCommand"/> requests.
/// </summary>
public sealed class CreateTranscriptCommandHandler
    : IRequestHandler<CreateTranscriptCommand, Result<CreateTranscriptResponse>>
{
    private readonly ITranscriptRepository _transcriptRepository;
    private readonly IMeetingRepository _meetingRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateTranscriptCommandHandler(
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

    public async Task<Result<CreateTranscriptResponse>> Handle(
     CreateTranscriptCommand request,
     CancellationToken cancellationToken)
    {
        // 1. Authentication
        if (_currentUserService.UserId is not Guid currentUserId)
        {
            return Result<CreateTranscriptResponse>.Failure(
                Error.Unauthorized);
        }

        // 2. Meeting existence
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<CreateTranscriptResponse>.Failure(
                MeetingErrors.NotFound);
        }

        // 3. Ownership
        if (meeting.OrganizerId != currentUserId)
        {
            return Result<CreateTranscriptResponse>.Failure(
                Error.Forbidden);
        }

        // 4. Duplicate transcript check
        var existingTranscript =
            await _transcriptRepository.GetByMeetingIdAsync(
                request.MeetingId,
                cancellationToken);

        if (existingTranscript is not null)
        {
            return Result<CreateTranscriptResponse>.Failure(
                TranscriptErrors.AlreadyExists);
        }

        // 5. Domain creation
        var transcriptResult = Transcript.Create(
            request.MeetingId,
            request.Content,
            request.Language,
            request.Duration);

        if (transcriptResult.IsFailure)
        {
            return Result<CreateTranscriptResponse>.Failure(
                transcriptResult.Error);
        }

        // 6. Persistence
        await _transcriptRepository.AddAsync(
            transcriptResult.Value,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<CreateTranscriptResponse>.Success(
            new CreateTranscriptResponse(
                transcriptResult.Value.Id));
    }
}
