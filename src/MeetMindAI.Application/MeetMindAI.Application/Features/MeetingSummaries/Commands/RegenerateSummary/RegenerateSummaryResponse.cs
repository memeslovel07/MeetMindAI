namespace MeetMindAI.Application.Features.MeetingSummaries.Commands.RegenerateSummary;

public sealed record RegenerateSummaryResponse(
    Guid SummaryId,
    string Summary);
