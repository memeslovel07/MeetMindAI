using MediatR;

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

    public DeleteTranscriptCommandHandler(
        ITranscriptRepository transcriptRepository,
        IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(transcriptRepository);
        ArgumentNullException.ThrowIfNull(dbContext);

        _transcriptRepository = transcriptRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<DeleteTranscriptResponse>> Handle(
        DeleteTranscriptCommand request,
        CancellationToken cancellationToken)
    {
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
