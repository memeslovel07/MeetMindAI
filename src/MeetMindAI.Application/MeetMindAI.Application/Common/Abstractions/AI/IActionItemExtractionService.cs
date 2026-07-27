namespace MeetMindAI.Application.Common.Interfaces.AI;

public interface IActionItemExtractionService
{
    Task<IReadOnlyList<ExtractedActionItem>> ExtractAsync(
        string transcript,
        CancellationToken cancellationToken = default);
}
