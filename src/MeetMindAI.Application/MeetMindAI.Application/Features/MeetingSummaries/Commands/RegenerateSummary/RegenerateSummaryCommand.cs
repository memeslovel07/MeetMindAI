using MediatR;

using MeetMindAI.Application.Features.MeetingSummaries.Commands.GenerateSummary;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingSummaries.Commands.RegenerateSummary;

public sealed record RegenerateSummaryCommand(
    Guid MeetingId)
    : IRequest<Result<RegenerateSummaryResponse>>;
