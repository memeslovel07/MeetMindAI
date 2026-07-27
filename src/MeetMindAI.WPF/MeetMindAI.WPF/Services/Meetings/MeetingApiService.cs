using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

using MeetMindAI.WPF.Models.Meetings;

namespace MeetMindAI.WPF.Services.Meetings;

public sealed class MeetingApiService : IMeetingApiService
{
    private readonly HttpClient _httpClient;

    public MeetingApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<MeetingListItem>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                "api/meetings/mine",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Unable to load meetings. HTTP {(int)response.StatusCode}.");
        }

        var meetings =
            await response.Content.ReadFromJsonAsync<List<MeetingListItem>>(
                cancellationToken: cancellationToken);

        return meetings ?? [];
    }

    public async Task<CreateMeetingResponse> CreateAsync(
    CreateMeetingRequest request,
    CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/meetings",
                request,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Unable to create meeting. " +
                $"HTTP {(int)response.StatusCode}. " +
                $"{errorContent}");
        }

        var result =
            await response.Content.ReadFromJsonAsync<CreateMeetingResponse>(
                cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "The server returned an empty create-meeting response.");
    }

    public async Task<MeetingDetails> GetByIdAsync(
    Guid meetingId,
    CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                $"api/meetings/{meetingId}",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                "The meeting could not be found.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Unable to load meeting. HTTP {(int)response.StatusCode}.");
        }

        var meeting =
            await response.Content.ReadFromJsonAsync<MeetingDetails>(
                cancellationToken: cancellationToken);

        return meeting
            ?? throw new InvalidOperationException(
                "The server returned an empty meeting response.");
    }

}
