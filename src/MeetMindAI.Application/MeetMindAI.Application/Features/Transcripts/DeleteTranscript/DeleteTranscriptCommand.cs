using MediatR;

using MeetMindAI.Application.Features.Transcripts.DeleteTranscript;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Transcripts.DeleteTranscript;

/// <summary>
/// Represents a request to delete a transcript.
/// </summary>
public sealed record DeleteTranscriptCommand(
    Guid MeetingId)
    : IRequest<Result<DeleteTranscriptResponse>>;
