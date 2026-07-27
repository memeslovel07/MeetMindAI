using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

using MeetMindAI.WPF.Models.ActionItems;

namespace MeetMindAI.WPF.Services.ActionItems;

public sealed class ActionItemApiService
    : IActionItemApiService
{
    private readonly HttpClient _httpClient;

    public ActionItemApiService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ActionItemDetails>> GetByMeetingAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                $"api/meetings/{meetingId}/action-items",
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to load action items.",
                cancellationToken);
        }

        return await response.Content
                   .ReadFromJsonAsync<List<ActionItemDetails>>(
                       cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<CreateActionItemResponse> CreateAsync(
        Guid meetingId,
        CreateActionItemRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                $"api/meetings/{meetingId}/action-items",
                request,
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to create action item.",
                cancellationToken);
        }

        return await response.Content
                   .ReadFromJsonAsync<CreateActionItemResponse>(
                       cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException(
                   "The server returned an empty response.");
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateActionItemRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PutAsJsonAsync(
                $"api/action-items/{id}",
                request,
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to update action item.",
                cancellationToken);
        }
    }

    public async Task CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"api/action-items/{id}/complete");

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to complete action item.",
                cancellationToken);
        }
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.DeleteAsync(
                $"api/action-items/{id}",
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to delete action item.",
                cancellationToken);
        }
    }

    public async Task<IReadOnlyList<Guid>> ExtractAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsync(
                $"api/extract?meetingId={meetingId}",
                content: null,
                cancellationToken);

        EnsureAuthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateExceptionAsync(
                response,
                "Unable to extract action items.",
                cancellationToken);
        }

        return await response.Content
                   .ReadFromJsonAsync<List<Guid>>(
                       cancellationToken: cancellationToken)
               ?? [];
    }

    private static void EnsureAuthorized(
        HttpResponseMessage response)
    {
        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "Your session has expired.");
        }
    }

    private static async Task<Exception> CreateExceptionAsync(
        HttpResponseMessage response,
        string message,
        CancellationToken cancellationToken)
    {
        var error =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        return new InvalidOperationException(
            $"{message} {error}");
    }
}
