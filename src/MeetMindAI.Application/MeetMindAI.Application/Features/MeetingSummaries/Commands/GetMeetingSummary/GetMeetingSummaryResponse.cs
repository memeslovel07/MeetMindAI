namespace MeetMindAI.Application.Features.MeetingSummaries.Commands.GetMeetingSummary;

public sealed record GetMeetingSummaryResponse(
    Guid SummaryId,
    Guid MeetingId,
    string Summary,
    string Provider,
    string Model,
    string PromptVersion,
    DateTime GeneratedAtUtc,
    bool IsRegenerated);
