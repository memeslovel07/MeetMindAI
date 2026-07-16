using MediatR;

using MeetMindAI.Application.Features.Transcripts.GetTranscript;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.Transcripts.GetTranscript;

/// <summary>
/// Gets the transcript for a meeting.
/// </summary>
public sealed record GetTranscriptQuery(
    Guid MeetingId)
    : IRequest<Result<GetTranscriptResponse>>;
