using MediatR;

using MeetMindAI.Application.Features.Transcripts.UpdateTranscript;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Transcripts.UpdateTranscript;

/// <summary>
/// Represents a request to update a transcript.
/// </summary>
public sealed record UpdateTranscriptCommand(
    Guid MeetingId,
    string Content,
    string? Language,
    TimeSpan? Duration)
    : IRequest<Result<UpdateTranscriptResponse>>;
