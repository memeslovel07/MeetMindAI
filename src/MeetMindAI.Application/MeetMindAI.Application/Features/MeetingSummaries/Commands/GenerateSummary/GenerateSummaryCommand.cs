using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingSummaries.Commands.GenerateSummary;

public sealed record GenerateSummaryCommand(
    Guid MeetingId)
    : IRequest<Result<GenerateSummaryResponse>>;
