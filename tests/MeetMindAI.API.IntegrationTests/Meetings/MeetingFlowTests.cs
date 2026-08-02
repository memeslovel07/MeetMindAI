using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using MeetMindAI.API.Contracts.Authentication;
using MeetMindAI.API.Contracts.Meetings;
using MeetMindAI.API.IntegrationTests.Infrastructure;
using MeetMindAI.Application.Authentication.Login;
using MeetMindAI.Application.Meetings.CreateMeeting;
using MeetMindAI.Application.Meetings.GetMeeting;
using MeetMindAI.Application.Meetings.GetMyMeetings;
using MeetMindAI.Application.Meetings.UpdateMeeting;

using Xunit;

namespace MeetMindAI.API.IntegrationTests.Meetings;

public sealed class MeetingFlowTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MeetingFlowTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Get_Update_Delete_Meeting_ShouldSucceed()
    {
        // Arrange
        await AuthenticateAsync();

        var scheduledAt =
            DateTime.UtcNow.AddDays(1);

        var createRequest =
            new CreateMeetingRequest(
                "Integration Test Meeting",
                "Original meeting description",
                scheduledAt,
                60);

        // ---------------------------------
        // Create
        // ---------------------------------

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/meetings",
                createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateMeetingResponse>();

        Assert.NotNull(created);

        Assert.NotEqual(
            Guid.Empty,
            created.MeetingId);

        // ---------------------------------
        // Get by ID
        // ---------------------------------

        var getResponse =
            await _client.GetAsync(
                $"/api/meetings/{created.MeetingId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var meeting =
            await getResponse.Content
                .ReadFromJsonAsync<GetMeetingResponse>();

        Assert.NotNull(meeting);

        Assert.Equal(
            created.MeetingId,
            meeting.Id);

        Assert.Equal(
            "Integration Test Meeting",
            meeting.Title);

        Assert.Equal(
            "Original meeting description",
            meeting.Description);

        Assert.Equal(
            60,
            meeting.DurationMinutes);

        // ---------------------------------
        // Get My Meetings
        // ---------------------------------

        var mineResponse =
            await _client.GetAsync(
                "/api/meetings/mine");

        Assert.Equal(
            HttpStatusCode.OK,
            mineResponse.StatusCode);

        var myMeetings =
            await mineResponse.Content
                .ReadFromJsonAsync<
                    List<GetMyMeetingsResponse>>();

        Assert.NotNull(myMeetings);

        Assert.Contains(
            myMeetings,
            x => x.Id == created.MeetingId);

        // ---------------------------------
        // Update
        // ---------------------------------

        var updateRequest =
            new UpdateMeetingRequest(
                "Updated Integration Meeting",
                "Updated meeting description",
                scheduledAt.AddDays(1),
                90);

        var updateResponse =
            await _client.PutAsJsonAsync(
                $"/api/meetings/{created.MeetingId}",
                updateRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updated =
            await updateResponse.Content
                .ReadFromJsonAsync<UpdateMeetingResponse>();

        Assert.NotNull(updated);

        Assert.Equal(
            created.MeetingId,
            updated.MeetingId);

        // ---------------------------------
        // Verify update
        // ---------------------------------

        var updatedGetResponse =
            await _client.GetAsync(
                $"/api/meetings/{created.MeetingId}");

        Assert.Equal(
            HttpStatusCode.OK,
            updatedGetResponse.StatusCode);

        var updatedMeeting =
            await updatedGetResponse.Content
                .ReadFromJsonAsync<GetMeetingResponse>();

        Assert.NotNull(updatedMeeting);

        Assert.Equal(
            "Updated Integration Meeting",
            updatedMeeting.Title);

        Assert.Equal(
            "Updated meeting description",
            updatedMeeting.Description);

        Assert.Equal(
            90,
            updatedMeeting.DurationMinutes);

        // ---------------------------------
        // Delete
        // ---------------------------------

        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/meetings/{created.MeetingId}");

        Assert.Equal(
            HttpStatusCode.OK,
            deleteResponse.StatusCode);

        // ---------------------------------
        // Verify soft deletion
        // ---------------------------------

        var deletedGetResponse =
            await _client.GetAsync(
                $"/api/meetings/{created.MeetingId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            deletedGetResponse.StatusCode);

        // Also verify deleted meeting no
        // longer appears in "mine".
        var mineAfterDeleteResponse =
            await _client.GetAsync(
                "/api/meetings/mine");

        Assert.Equal(
            HttpStatusCode.OK,
            mineAfterDeleteResponse.StatusCode);

        var meetingsAfterDelete =
            await mineAfterDeleteResponse.Content
                .ReadFromJsonAsync<
                    List<GetMyMeetingsResponse>>();

        Assert.NotNull(meetingsAfterDelete);

        Assert.DoesNotContain(
            meetingsAfterDelete,
            x => x.Id == created.MeetingId);
    }

    private async Task AuthenticateAsync()
    {
        var email =
            $"meeting-{Guid.NewGuid():N}@meetmind.test";

        const string password =
            "TestPassword123!";

        // Register
        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest(
                    "Meeting",
                    "Tester",
                    email,
                    password));

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

        // Login
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

        // Authenticate subsequent requests.
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);
    }


    [Fact]
    public async Task DifferentUser_ShouldNotBeAbleToUpdateOrDeleteMeeting()
    {
        // ---------------------------------
        // User A
        // ---------------------------------

        await AuthenticateAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/meetings",
                new CreateMeetingRequest(
                    "User A Meeting",
                    "Owned by User A",
                    DateTime.UtcNow.AddDays(1),
                    60));

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateMeetingResponse>();

        Assert.NotNull(created);

        // ---------------------------------
        // User B
        // ---------------------------------

        await AuthenticateAsync();

        // _client now contains User B's JWT.

        // ---------------------------------
        // User B attempts update
        // ---------------------------------

        var updateRequest =
            new UpdateMeetingRequest(
                "Hacked Meeting",
                "User B should not be allowed",
                DateTime.UtcNow.AddDays(2),
                120);

        var updateResponse =
            await _client.PutAsJsonAsync(
                $"/api/meetings/{created.MeetingId}",
                updateRequest);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            updateResponse.StatusCode);

        // ---------------------------------
        // User B attempts deletion
        // ---------------------------------

        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/meetings/{created.MeetingId}");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DifferentUser_ShouldNotBeAbleToGetMeeting()
    {
        // User A
        await AuthenticateAsync();

        var createResponse =
            await _client.PostAsJsonAsync(
                "/api/meetings",
                new CreateMeetingRequest(
                    "Private Meeting",
                    "Only User A should access this",
                    DateTime.UtcNow.AddDays(1),
                    60));

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateMeetingResponse>();

        Assert.NotNull(created);

        // User B
        await AuthenticateAsync();

        // User B attempts to read User A's meeting.
        var response =
            await _client.GetAsync(
                $"/api/meetings/{created.MeetingId}");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

}
