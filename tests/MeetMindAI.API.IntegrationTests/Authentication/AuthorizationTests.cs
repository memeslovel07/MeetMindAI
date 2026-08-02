using Xunit;

using System.Net;

using MeetMindAI.API.IntegrationTests.Infrastructure;

namespace MeetMindAI.API.IntegrationTests.Authentication;

public sealed class AuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response =
            await _client.GetAsync("/api/users/me");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
