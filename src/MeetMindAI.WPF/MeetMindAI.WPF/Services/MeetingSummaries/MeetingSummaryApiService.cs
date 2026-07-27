using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

using MeetMindAI.WPF.Models.MeetingSummaries;

namespace MeetMindAI.WPF.Services.MeetingSummaries;

public sealed class MeetingSummaryApiService
    : IMeetingSummaryApiService
{
    private readonly HttpClient _httpClient;

    public MeetingSummaryApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MeetingSummaryDetails?> GetAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                $"api/meetings/{meetingId}/summary",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Unable to load summary. HTTP {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<MeetingSummaryDetails>(
                cancellationToken: cancellationToken);
    }

    public async Task<GenerateSummaryResponse> GenerateAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsync(
                $"api/meetings/{meetingId}/summary",
                content: null,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                "A transcript is required before generating a summary.");
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException(
                "A summary already exists for this meeting.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Unable to generate summary. {error}");
        }

        return await response.Content
                   .ReadFromJsonAsync<GenerateSummaryResponse>(
                       cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "The server returned an empty summary response.");
    }

    public async Task<RegenerateSummaryResponse> RegenerateAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PutAsync(
                $"api/meetings/{meetingId}/summary",
                content: null,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                "The meeting, transcript, or summary could not be found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Unable to regenerate summary. {error}");
        }

        return await response.Content
                   .ReadFromJsonAsync<RegenerateSummaryResponse>(
                       cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "The server returned an empty summary response.");
    }
}
