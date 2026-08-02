using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using MeetMindAI.API.Contracts.Authentication;
using MeetMindAI.API.Contracts.Meetings;
using MeetMindAI.API.Contracts.Transcripts;
using MeetMindAI.API.IntegrationTests.Infrastructure;
using MeetMindAI.Application.Authentication.Login;
using MeetMindAI.Application.Features.Transcripts.CreateTranscript;
using MeetMindAI.Application.Features.Transcripts.DeleteTranscript;
using MeetMindAI.Application.Features.Transcripts.GetTranscript;
using MeetMindAI.Application.Features.Transcripts.UpdateTranscript;
using MeetMindAI.Application.Meetings.CreateMeeting;

using Xunit;

namespace MeetMindAI.API.IntegrationTests.Transcripts;

public sealed class TranscriptFlowTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TranscriptFlowTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Get_Update_Delete_Transcript_ShouldSucceed()
    {
        // ---------------------------------
        // Authenticate
        // ---------------------------------

        await AuthenticateAsync();

        // ---------------------------------
        // Create meeting
        // ---------------------------------

        var meetingResponse =
            await _client.PostAsJsonAsync(
                "/api/meetings",
                new CreateMeetingRequest(
                    "Transcript Integration Meeting",
                    "Meeting used for transcript integration testing.",
                    DateTime.UtcNow.AddDays(1),
                    60));

        Assert.Equal(
            HttpStatusCode.Created,
            meetingResponse.StatusCode);

        var meeting =
            await meetingResponse.Content
                .ReadFromJsonAsync<CreateMeetingResponse>();

        Assert.NotNull(meeting);

        // ---------------------------------
        // Create transcript
        // ---------------------------------

        var createTranscriptResponse =
            await _client.PostAsJsonAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript",
                new CreateTranscriptRequest(
                    "This is the original transcript.",
                    "en",
                    300));

        Assert.Equal(
            HttpStatusCode.Created,
            createTranscriptResponse.StatusCode);

        var createdTranscript =
            await createTranscriptResponse.Content
                .ReadFromJsonAsync<CreateTranscriptResponse>();

        Assert.NotNull(createdTranscript);

        Assert.NotEqual(
            Guid.Empty,
            createdTranscript.TranscriptId);

        // ---------------------------------
        // Get transcript
        // ---------------------------------

        var getResponse =
            await _client.GetAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var transcript =
            await getResponse.Content
                .ReadFromJsonAsync<GetTranscriptResponse>();

        Assert.NotNull(transcript);

        Assert.Equal(
            createdTranscript.TranscriptId,
            transcript.TranscriptId);

        Assert.Equal(
            meeting.MeetingId,
            transcript.MeetingId);

        Assert.Equal(
            "This is the original transcript.",
            transcript.Content);

        Assert.Equal(
            "en",
            transcript.Language);

        Assert.Equal(
            TimeSpan.FromSeconds(300),
            transcript.Duration);

        // ---------------------------------
        // Update transcript
        // ---------------------------------

        var updateResponse =
            await _client.PutAsJsonAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript",
                new UpdateTranscriptRequest(
                    "This is the updated transcript.",
                    "en-US",
                    600));

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updated =
            await updateResponse.Content
                .ReadFromJsonAsync<UpdateTranscriptResponse>();

        Assert.NotNull(updated);

        Assert.Equal(
            createdTranscript.TranscriptId,
            updated.TranscriptId);

        // ---------------------------------
        // Verify update
        // ---------------------------------

        var getUpdatedResponse =
            await _client.GetAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript");

        Assert.Equal(
            HttpStatusCode.OK,
            getUpdatedResponse.StatusCode);

        var updatedTranscript =
            await getUpdatedResponse.Content
                .ReadFromJsonAsync<GetTranscriptResponse>();

        Assert.NotNull(updatedTranscript);

        Assert.Equal(
            createdTranscript.TranscriptId,
            updatedTranscript.TranscriptId);

        Assert.Equal(
            "This is the updated transcript.",
            updatedTranscript.Content);

        Assert.Equal(
            "en-US",
            updatedTranscript.Language);

        Assert.Equal(
            TimeSpan.FromSeconds(600),
            updatedTranscript.Duration);

        // ---------------------------------
        // Delete transcript
        // ---------------------------------

        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript");

        Assert.Equal(
            HttpStatusCode.OK,
            deleteResponse.StatusCode);

        var deleted =
            await deleteResponse.Content
                .ReadFromJsonAsync<DeleteTranscriptResponse>();

        Assert.NotNull(deleted);

        Assert.Equal(
            createdTranscript.TranscriptId,
            deleted.TranscriptId);

        // ---------------------------------
        // Verify deletion
        // ---------------------------------

        var getDeletedResponse =
            await _client.GetAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getDeletedResponse.StatusCode);
    }

    private async Task AuthenticateAsync()
    {
        var email =
            $"transcript-{Guid.NewGuid():N}@meetmind.test";

        const string password =
            "TestPassword123!";

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest(
                    "Transcript",
                    "Tester",
                    email,
                    password));

        Assert.Equal(
            HttpStatusCode.Created,
            registerResponse.StatusCode);

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

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.AccessToken);
    }

    [Fact]
    public async Task DifferentUser_ShouldNotBeAbleToAccessOrModifyTranscript()
    {
        // ---------------------------------
        // User A
        // ---------------------------------

        await AuthenticateAsync();

        var meetingResponse =
            await _client.PostAsJsonAsync(
                "/api/meetings",
                new CreateMeetingRequest(
                    "Private Transcript Meeting",
                    "Owned by User A",
                    DateTime.UtcNow.AddDays(1),
                    60));

        Assert.Equal(
            HttpStatusCode.Created,
            meetingResponse.StatusCode);

        var meeting =
            await meetingResponse.Content
                .ReadFromJsonAsync<CreateMeetingResponse>();

        Assert.NotNull(meeting);

        var createTranscriptResponse =
            await _client.PostAsJsonAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript",
                new CreateTranscriptRequest(
                    "User A private transcript.",
                    "en",
                    300));

        Assert.Equal(
            HttpStatusCode.Created,
            createTranscriptResponse.StatusCode);

        // ---------------------------------
        // User B
        // ---------------------------------

        await AuthenticateAsync();

        // ---------------------------------
        // User B attempts GET
        // ---------------------------------

        var getResponse =
            await _client.GetAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            getResponse.StatusCode);

        // ---------------------------------
        // User B attempts UPDATE
        // ---------------------------------

        var updateResponse =
            await _client.PutAsJsonAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript",
                new UpdateTranscriptRequest(
                    "User B attempted modification.",
                    "en",
                    600));

        Assert.Equal(
            HttpStatusCode.Forbidden,
            updateResponse.StatusCode);

        // ---------------------------------
        // User B attempts DELETE
        // ---------------------------------

        var deleteResponse =
            await _client.DeleteAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DifferentUser_ShouldNotBeAbleToCreateTranscriptForMeeting()
    {
        // Arrange — User A
        var ownerEmail =
            $"owner-{Guid.NewGuid()}@example.com";

        var ownerRegisterResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    FirstName = "Meeting",
                    LastName = "Owner",
                    Email = ownerEmail,
                    Password = "Password123!"
                });

        Assert.Equal(
            HttpStatusCode.Created,
            ownerRegisterResponse.StatusCode);

        var ownerLoginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    Email = ownerEmail,
                    Password = "Password123!"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            ownerLoginResponse.StatusCode);

        var ownerLogin =
            await ownerLoginResponse.Content
                .ReadFromJsonAsync<LoginUserResponse>();

        Assert.NotNull(ownerLogin);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                ownerLogin.AccessToken);

        // User A creates the meeting.
        var meetingResponse =
            await _client.PostAsJsonAsync(
                "/api/meetings",
                new
                {
                    Title = "Private Meeting",
                    Description = "Owned by User A",
                    ScheduledAtUtc = DateTime.UtcNow.AddDays(1),
                    DurationMinutes = 60
                });

        Assert.Equal(
            HttpStatusCode.Created,
            meetingResponse.StatusCode);

        var meeting =
            await meetingResponse.Content
                .ReadFromJsonAsync<CreateMeetingResponse>();

        Assert.NotNull(meeting);

        // Arrange — User B
        var attackerEmail =
            $"other-{Guid.NewGuid()}@example.com";

        var attackerRegisterResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    FirstName = "Different",
                    LastName = "User",
                    Email = attackerEmail,
                    Password = "Password123!"
                });

        Assert.Equal(
            HttpStatusCode.Created,
            attackerRegisterResponse.StatusCode);

        var attackerLoginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    Email = attackerEmail,
                    Password = "Password123!"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            attackerLoginResponse.StatusCode);

        var attackerLogin =
            await attackerLoginResponse.Content
                .ReadFromJsonAsync<LoginUserResponse>();

        Assert.NotNull(attackerLogin);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                attackerLogin.AccessToken);

        // Act — User B attempts to create transcript
        // for User A's meeting.
        var response =
            await _client.PostAsJsonAsync(
                $"/api/meetings/{meeting.MeetingId}/transcript",
                new
                {
                    Content =
                        "Unauthorized transcript content.",
                    Language = "English",
                    DurationSeconds = 120
                });

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
}
