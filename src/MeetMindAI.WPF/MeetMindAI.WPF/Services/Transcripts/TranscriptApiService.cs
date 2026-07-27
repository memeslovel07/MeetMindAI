using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

using MeetMindAI.WPF.Models.Transcripts;

namespace MeetMindAI.WPF.Services.Transcripts;

public sealed class TranscriptApiService
    : ITranscriptApiService
{
    private readonly HttpClient _httpClient;

    public TranscriptApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TranscriptDetails?> GetAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                $"api/meetings/{meetingId}/transcript",
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
                $"Unable to load transcript. HTTP {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<TranscriptDetails>(
                cancellationToken: cancellationToken);
    }

    public async Task<CreateTranscriptResponse> CreateAsync(
        Guid meetingId,
        CreateTranscriptRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                $"api/meetings/{meetingId}/transcript",
                request,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException(
                "A transcript already exists for this meeting.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Unable to create transcript. {error}");
        }

        return await response.Content
                   .ReadFromJsonAsync<CreateTranscriptResponse>(
                       cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "The server returned an empty response.");
    }

    public async Task<UpdateTranscriptResponse> UpdateAsync(
        Guid meetingId,
        UpdateTranscriptRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PutAsJsonAsync(
                $"api/meetings/{meetingId}/transcript",
                request,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                "The transcript could not be found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Unable to update transcript. {error}");
        }

        return await response.Content
                   .ReadFromJsonAsync<UpdateTranscriptResponse>(
                       cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "The server returned an empty response.");
    }
}
