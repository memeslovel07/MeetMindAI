namespace MeetMindAI.Application.Common.Abstractions.AI;

public interface IAiSummaryService
{
    Task<AiSummaryResult> GenerateSummaryAsync(
        string transcript,
        CancellationToken cancellationToken = default);
}
