using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingSummaries.Commands.GetMeetingSummary;

public sealed record GetMeetingSummaryQuery(
    Guid MeetingId)
    : IRequest<Result<GetMeetingSummaryResponse>>;
