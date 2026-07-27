using System.Net.Http.Json;
using System.Text.Json;

using MeetMindAI.WPF.Models.Authentication;

namespace MeetMindAI.WPF.Services.Authentication;

public sealed class AuthenticationApiService
    : IAuthenticationApiService
{
    private readonly System.Net.Http.HttpClient _httpClient;

    public AuthenticationApiService(
        System.Net.Http.HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(error)
                    ? "Login failed."
                    : error);
        }

        var loginResponse =
            await response.Content.ReadFromJsonAsync<LoginResponse>(
                cancellationToken: cancellationToken);

        if (loginResponse is null)
        {
            throw new InvalidOperationException(
                "The server returned an invalid login response.");
        }

        return loginResponse;
    }
}
