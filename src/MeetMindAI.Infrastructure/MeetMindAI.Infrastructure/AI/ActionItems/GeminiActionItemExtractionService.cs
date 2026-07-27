using System.Net.Http.Json;
using System.Text.Json;

using MeetMindAI.Application.Common.Interfaces.AI;
using MeetMindAI.Domain.Enums.Meetings;

using Microsoft.Extensions.Options;

namespace MeetMindAI.Infrastructure.AI.Gemini;

public sealed class GeminiActionItemExtractionService
    : IActionItemExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiActionItemExtractionService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    private static string CleanJsonResponse(string response)
    {
        var cleaned = response.Trim();

        var startIndex = cleaned.IndexOf('[');
        var endIndex = cleaned.LastIndexOf(']');

        if (startIndex == -1 ||
            endIndex == -1 ||
            endIndex < startIndex)
        {
            throw new InvalidOperationException(
                "Gemini response did not contain a valid JSON array.");
        }

        return cleaned.Substring(
            startIndex,
            endIndex - startIndex + 1);
    }

    public async Task<IReadOnlyList<ExtractedActionItem>> ExtractAsync(
        string transcript,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return Array.Empty<ExtractedActionItem>();
        }

        var prompt = BuildPrompt(transcript);

        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",

                responseSchema = new
                {
                    type = "ARRAY",

                    items = new
                    {
                        type = "OBJECT",

                        properties = new
                        {
                            title = new
                            {
                                type = "STRING"
                            },

                            description = new
                            {
                                type = "STRING",
                                nullable = true
                            },

                            priority = new
                            {
                                type = "STRING",
                                @enum = new[]
                    {
                        "Low",
                        "Medium",
                        "High"
                    }
                            },

                            dueDate = new
                            {
                                type = "STRING",
                                nullable = true
                            }
                        },

                        required = new[]
            {
                "title",
                "priority"
            }
                    }
                }
            }
        };

        var url =
    $"v1beta/models/{_options.Model}:generateContent";

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        httpRequest.Headers.Add(
            "x-goog-api-key",
            _options.ApiKey);

        httpRequest.Content =
            JsonContent.Create(request);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        var responseContent =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Gemini API request failed. " +
                $"Status: {(int)response.StatusCode} ({response.StatusCode}). " +
                $"Response: {responseContent}");
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
            return Array.Empty<ExtractedActionItem>();
        }

      

        // Put breakpoint here and inspect generatedText
        var cleanedJson = CleanJsonResponse(generatedText);

        

        // Inspect cleanedJson here too
        List<GeminiActionItemResponse>? extractedItems;

        try
        {
            extractedItems =
                JsonSerializer.Deserialize<List<GeminiActionItemResponse>>(
                    cleanedJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "Gemini returned an invalid action-item response.",
                ex);
        }

        if (extractedItems is null)
        {
            return Array.Empty<ExtractedActionItem>();
        }

        return extractedItems
            .Where(item => !string.IsNullOrWhiteSpace(item.Title))
            .Select(Map)
            .ToList();
    }
    private static ExtractedActionItem Map(
        GeminiActionItemResponse item)
    {
        var priority = Enum.TryParse<ActionItemPriority>(
            item.Priority,
            ignoreCase: true,
            out var parsedPriority)
                ? parsedPriority
                : ActionItemPriority.Medium;

        DateTime? dueDate = null;

        if (!string.IsNullOrWhiteSpace(item.DueDate) &&
            DateTime.TryParse(item.DueDate, out var parsedDueDate))
        {
            dueDate = parsedDueDate;
        }

        return new ExtractedActionItem(
            item.Title,
            item.Description,
            priority,
            dueDate);
    }

    private static string BuildPrompt(string transcript)
    {
        return $$"""
            You are an AI meeting assistant.

            Extract actionable tasks from the following meeting transcript.


            Rules:
            - Extract only clear and actionable tasks.
            - Do not invent tasks.
            - Keep titles concise.
            - Use Medium priority when priority is unclear.
            - Use null for dueDate when no deadline is explicitly mentioned.
            - Do not include markdown.
            - Do not include explanations outside the JSON array.
            - If there are no action items, return [].

            Transcript:
            {{transcript}}
            """;
    }

    private sealed class GeminiActionItemResponse
    {
        public string Title { get; init; } = string.Empty;

        public string? Description { get; init; }

        public string Priority { get; init; } = "Medium";

        public string? DueDate { get; init; }
    }


}
