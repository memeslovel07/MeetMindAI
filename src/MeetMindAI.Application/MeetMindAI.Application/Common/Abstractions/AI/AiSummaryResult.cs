namespace MeetMindAI.Application.Common.Abstractions.AI;

public sealed record AiSummaryResult(
    string Summary,
    string Provider,
    string Model);
