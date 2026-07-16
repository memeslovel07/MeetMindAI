using MediatR;

using MeetMindAI.Application.Features.Transcripts.CreateTranscript;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.Transcripts.CreateTranscript;

/// <summary>
/// Represents a request to create a transcript.
/// </summary>
public sealed record CreateTranscriptCommand(
    Guid MeetingId,
    string Content,
    string? Language,
    TimeSpan? Duration)
    : IRequest<Result<CreateTranscriptResponse>>;
