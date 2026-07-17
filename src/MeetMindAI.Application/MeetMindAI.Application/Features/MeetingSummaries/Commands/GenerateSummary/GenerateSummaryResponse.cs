namespace MeetMindAI.Application.Features.MeetingSummaries.Commands.GenerateSummary;

public sealed record GenerateSummaryResponse(
    Guid SummaryId,
    string Summary);
