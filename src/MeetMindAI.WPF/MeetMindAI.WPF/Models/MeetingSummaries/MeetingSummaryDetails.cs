namespace MeetMindAI.WPF.Models.MeetingSummaries;

public sealed record MeetingSummaryDetails(
    Guid SummaryId,
    Guid MeetingId,
    string Summary,
    string Provider,
    string Model,
    string PromptVersion,
    DateTime GeneratedAtUtc,
    bool IsRegenerated)
{
    public DateTime GeneratedAtLocal =>
        GeneratedAtUtc.ToLocalTime();
}
