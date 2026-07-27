using System.Net.Http.Json;
using System.Text.Json;

using MeetMindAI.Application.Common.Abstractions.AI;

using Microsoft.Extensions.Options;

namespace MeetMindAI.Infrastructure.AI.Gemini;

public sealed class GeminiAiSummaryService
    : IAiSummaryService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiAiSummaryService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<AiSummaryResult> GenerateSummaryAsync(
        string transcript,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            throw new ArgumentException(
                "Transcript cannot be empty.",
                nameof(transcript));
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = BuildPrompt(transcript)
                        }
                    }
                }
            }
        };

        var url =
            $"v1beta/models/{_options.Model}:generateContent";

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                url);

        httpRequest.Headers.Add(
            "x-goog-api-key",
            _options.ApiKey);

        httpRequest.Content =
            JsonContent.Create(requestBody);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        var responseContent =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Gemini summary generation failed. " +
                $"Status: {(int)response.StatusCode} ({response.StatusCode}).");
        }

        using var document =
            JsonDocument.Parse(responseContent);

        var generatedText = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(generatedText))
        {
            throw new InvalidOperationException(
                "Gemini returned an empty meeting summary.");
        }

        return new AiSummaryResult(
            generatedText.Trim(),
            "Gemini",
            _options.Model);
    }

    private static string BuildPrompt(
        string transcript)
    {
        return $$"""
            You are an AI meeting assistant.

            Create a concise and professional summary of the following
            meeting transcript.

            Requirements:
            - Summarize the main topics discussed.
            - Include important decisions.
            - Include important conclusions and outcomes.
            - Do not invent information.
            - Do not include action items unless they are relevant to understanding the meeting outcome.
            - Return only the summary text.
            - Do not include markdown headings.

            Meeting transcript:

            {{transcript}}
            """;
    }
}
