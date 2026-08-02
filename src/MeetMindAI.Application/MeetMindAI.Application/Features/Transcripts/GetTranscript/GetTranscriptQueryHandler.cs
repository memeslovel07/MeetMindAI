using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;

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
    private readonly IMeetingRepository _meetingRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTranscriptQueryHandler(
        ITranscriptRepository transcriptRepository,
        IMeetingRepository meetingRepository,
        ICurrentUserService currentUserService)
    {
        ArgumentNullException.ThrowIfNull(transcriptRepository);
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(currentUserService);

        _transcriptRepository = transcriptRepository;
        _meetingRepository = meetingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetTranscriptResponse>> Handle(
        GetTranscriptQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid currentUserId)
        {
            return Result<GetTranscriptResponse>.Failure(
                Error.Unauthorized);
        }

        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<GetTranscriptResponse>.Failure(
                MeetingErrors.NotFound);
        }

        if (meeting.OrganizerId != currentUserId)
        {
            return Result<GetTranscriptResponse>.Failure(
                Error.Forbidden);
        }

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
