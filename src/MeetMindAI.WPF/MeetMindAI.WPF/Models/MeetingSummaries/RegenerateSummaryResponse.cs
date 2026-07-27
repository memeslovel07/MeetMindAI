namespace MeetMindAI.WPF.Models.MeetingSummaries;

public sealed record RegenerateSummaryResponse(
    Guid SummaryId,
    string Summary);
