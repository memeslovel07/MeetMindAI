namespace MeetMindAI.WPF.Models.MeetingSummaries;

public sealed record GenerateSummaryResponse(
    Guid SummaryId,
    string Summary);
