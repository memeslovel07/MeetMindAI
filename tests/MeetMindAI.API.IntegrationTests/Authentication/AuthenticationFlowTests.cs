using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MeetMindAI.Application.Authentication.RefreshToken;
using MeetMindAI.API.Contracts.Authentication;
using MeetMindAI.API.IntegrationTests.Infrastructure;
using MeetMindAI.Application.Authentication.Login;
using MeetMindAI.Application.Authentication.Register;
using MeetMindAI.Application.Users.GetCurrentUser;

using Xunit;
using MeetMindAI.Infrastructure.Authentication;

namespace MeetMindAI.API.IntegrationTests.Authentication;

public sealed class AuthenticationFlowTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationFlowTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Login_AndGetCurrentUser_ShouldSucceed()
    {
        // Arrange
        var email =
            $"integration-{Guid.NewGuid():N}@meetmind.test";

        const string password =
            "TestPassword123!";

        var registerRequest =
            new RegisterRequest(
                "Somesh",
                "Verma",
                email,
                password);

        // ---------------------------------
        // Register
        // ---------------------------------

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        var registeredUser =
            await registerResponse.Content
                .ReadFromJsonAsync<RegisterUserResponse>();

        Assert.NotNull(registeredUser);

        Assert.NotEqual(
            Guid.Empty,
            registeredUser.UserId);

        Assert.Equal(
            email,
            registeredUser.Email);

        Assert.Equal(
            "Somesh",
            registeredUser.FirstName);

        Assert.Equal(
            "Verma",
            registeredUser.LastName);

        // ---------------------------------
        // Login
        // ---------------------------------

        var loginRequest =
            new LoginRequest(
                email,
                password);

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var login =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginUserResponse>();

        Assert.NotNull(login);

        Assert.False(
            string.IsNullOrWhiteSpace(
                login.AccessToken));

        Assert.False(
            string.IsNullOrWhiteSpace(
                login.RefreshToken));

        Assert.True(
            login.AccessTokenExpiresAtUtc >
            DateTime.UtcNow);

        // ---------------------------------
        // Authenticate future requests
        // ---------------------------------

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);

        // ---------------------------------
        // GET /api/users/me
        // ---------------------------------

        var meResponse =
            await _client.GetAsync(
                "/api/users/me");

        Assert.Equal(
            HttpStatusCode.OK,
            meResponse.StatusCode);

        var currentUser =
            await meResponse.Content
                .ReadFromJsonAsync<GetCurrentUserResponse>();

        Assert.NotNull(currentUser);

        Assert.Equal(
            registeredUser.UserId,
            currentUser.Id);

        Assert.Equal(
            email,
            currentUser.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email =
            $"duplicate-{Guid.NewGuid():N}@meetmind.test";

        const string password = "TestPassword123!";

        var request = new RegisterRequest(
            "Somesh",
            "Verma",
            email,
            password);

        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        // Act
        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var email =
            $"wrong-password-{Guid.NewGuid():N}@meetmind.test";

        const string password = "TestPassword123!";

        await RegisterUserAsync(
            email,
            password);

        var loginRequest = new LoginRequest(
            email,
            "WrongPassword123!");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new LoginRequest(
            $"missing-{Guid.NewGuid():N}@meetmind.test",
            "TestPassword123!");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "this-is-not-a-valid-jwt");

        // Act
        var response =
            await _client.GetAsync(
                "/api/users/me");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    private async Task RegisterUserAsync(
    string email,
    string password)
    {
        var request = new RegisterRequest(
            "Test",
            "User",
            email,
            password);

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ShouldRotateRefreshToken()
    {
        // Arrange
        var email =
            $"refresh-{Guid.NewGuid():N}@meetmind.test";

        const string password = "TestPassword123!";

        await RegisterUserAsync(
            email,
            password);

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(
                    email,
                    password));

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var login =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginUserResponse>();

        Assert.NotNull(login);

        // Act
        var refreshResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    RefreshToken = login.RefreshToken
                });

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            refreshResponse.StatusCode);

        var refreshed =
            await refreshResponse.Content
                .ReadFromJsonAsync<RefreshTokenResponse>();

        Assert.NotNull(refreshed);

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshed.AccessToken));

        Assert.False(
            string.IsNullOrWhiteSpace(
                refreshed.RefreshToken));

        Assert.NotEqual(
            login.RefreshToken,
            refreshed.RefreshToken);

        Assert.True(
            refreshed.AccessTokenExpiresAtUtc >
            DateTime.UtcNow);
    }

    [Fact]
    public async Task Refresh_WithRotatedOldToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var email =
            $"rotation-{Guid.NewGuid():N}@meetmind.test";

        const string password = "TestPassword123!";

        await RegisterUserAsync(
            email,
            password);

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(
                    email,
                    password));

        var login =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginUserResponse>();

        Assert.NotNull(login);

        var originalRefreshToken =
            login.RefreshToken;

        // First refresh rotates the token.
        var firstRefresh =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    RefreshToken = originalRefreshToken
                });

        Assert.Equal(
            HttpStatusCode.OK,
            firstRefresh.StatusCode);

        // Act
        // Try the old token again.
        var secondRefresh =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    RefreshToken = originalRefreshToken
                });

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            secondRefresh.StatusCode);
    }

    [Fact]
    public async Task Logout_ShouldRevokeRefreshToken()
    {
        // Arrange
        var email =
            $"logout-{Guid.NewGuid():N}@meetmind.test";

        const string password = "TestPassword123!";

        await RegisterUserAsync(
            email,
            password);

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(
                    email,
                    password));

        var login =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginUserResponse>();

        Assert.NotNull(login);

        // Act
        var logoutResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/logout",
                new
                {
                    RefreshToken = login.RefreshToken
                });

        // Assert logout itself.
        Assert.Equal(
            HttpStatusCode.NoContent,
            logoutResponse.StatusCode);

        // The revoked token must no longer be usable.
        var refreshResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    RefreshToken = login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refreshResponse.StatusCode);
    }

}
