using MeetMindAI.Application.Common.Abstractions.AI;

namespace MeetMindAI.Infrastructure.AI.Mock;

/// <summary>
/// Mock AI summary generator used during development.
/// </summary>
public sealed class MockAiSummaryService : IAiSummaryService
{
    public Task<AiSummaryResult> GenerateSummaryAsync(
        string transcript,
        CancellationToken cancellationToken = default)
    {
        var summary = $"""
            Meeting Summary

            Transcript Length: {transcript.Length} characters.

            This is a mock AI-generated summary.

            Replace this implementation with Gemini or OpenAI in production.
            """;

        return Task.FromResult(
            new AiSummaryResult(
                summary,
                "Mock AI",
                "Mock-v1"));
    }
}
