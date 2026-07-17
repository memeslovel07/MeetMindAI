using MediatR;


using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.Transcripts.GetTranscript;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.Transcripts.GetTranscript;

/// <summary>
/// Handles transcript retrieval.
/// </summary>
public sealed class GetTranscriptQueryHandler
    : IRequestHandler<GetTranscriptQuery, Result<GetTranscriptResponse>>
{
    private readonly ITranscriptRepository _transcriptRepository;

    public GetTranscriptQueryHandler(
        ITranscriptRepository transcriptRepository)
    {
        ArgumentNullException.ThrowIfNull(transcriptRepository);

        _transcriptRepository = transcriptRepository;
    }

    public async Task<Result<GetTranscriptResponse>> Handle(
        GetTranscriptQuery request,
        CancellationToken cancellationToken)
    {
        var transcript =
            await _transcriptRepository.GetByMeetingIdAsync(
                request.MeetingId,
                cancellationToken);

        if (transcript is null)
        {
            return Result<GetTranscriptResponse>.Failure(
                TranscriptErrors.NotFound);
        }

        return Result<GetTranscriptResponse>.Success(
            new GetTranscriptResponse(
                transcript.Id,
                transcript.MeetingId,
                transcript.Content,
                transcript.Language,
                transcript.Duration));
    }
}
