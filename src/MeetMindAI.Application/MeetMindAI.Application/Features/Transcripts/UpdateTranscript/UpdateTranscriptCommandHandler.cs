using MediatR;

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

    public UpdateTranscriptCommandHandler(
        ITranscriptRepository transcriptRepository,
        IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(transcriptRepository);
        ArgumentNullException.ThrowIfNull(dbContext);

        _transcriptRepository = transcriptRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<UpdateTranscriptResponse>> Handle(
        UpdateTranscriptCommand request,
        CancellationToken cancellationToken)
    {
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
