using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Shared.Results;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.Transcripts.GetTranscript;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.Transcripts.GetTranscript;

public sealed class GetTranscriptQueryHandlerTests
{
    private readonly Mock<ITranscriptRepository>
        _transcriptRepositoryMock;

    private readonly GetTranscriptQueryHandler _handler;
    private readonly Mock<IMeetingRepository>
    _meetingRepositoryMock;

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock;

    public GetTranscriptQueryHandlerTests()
    {
        _transcriptRepositoryMock =
            new Mock<ITranscriptRepository>();

        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _handler = new GetTranscriptQueryHandler(
            _transcriptRepositoryMock.Object,
            _meetingRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTranscriptExists_ShouldReturnTranscriptResponse()
    {
        // Arrange
        // Arrange
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        var meetingResult = Meeting.Create(
            "Transcript Meeting",
            "Test meeting",
            organizerId,
            DateTime.UtcNow.AddDays(1),
            60);

        Assert.True(meetingResult.IsSuccess);

        var meeting = meetingResult.Value;
        var meetingId = meeting.Id;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var duration =
            TimeSpan.FromMinutes(42);

        var transcriptResult = Transcript.Create(
            meetingId,
            "This is the meeting transcript content.",
            "English",
            duration);

        Assert.True(transcriptResult.IsSuccess);

        var transcript = transcriptResult.Value;

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        var query =
            new GetTranscriptQuery(meetingId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value;

        Assert.Equal(
            transcript.Id,
            response.TranscriptId);

        Assert.Equal(
            meetingId,
            response.MeetingId);

        Assert.Equal(
            "This is the meeting transcript content.",
            response.Content);

        Assert.Equal(
            "English",
            response.Language);

        Assert.Equal(
            duration,
            response.Duration);

        _transcriptRepositoryMock.Verify(
            x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTranscriptDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        var meetingResult = Meeting.Create(
            "Meeting Without Transcript",
            null,
            organizerId,
            DateTime.UtcNow.AddDays(1),
            60);

        Assert.True(meetingResult.IsSuccess);

        var meeting = meetingResult.Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transcript?)null);

        var query =
            new GetTranscriptQuery(meeting.Id);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            TranscriptErrors.NotFound,
            result.Error);

        _transcriptRepositoryMock.Verify(
            x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var query =
            new GetTranscriptQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            Error.Unauthorized,
            result.Error);

        _meetingRepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _transcriptRepositoryMock.Verify(
            x => x.GetByMeetingIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnMeeting_ShouldReturnForbidden()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var meetingResult = Meeting.Create(
            "Private Meeting",
            null,
            organizerId,
            DateTime.UtcNow.AddDays(1),
            60);

        Assert.True(meetingResult.IsSuccess);

        var meeting = meetingResult.Value;

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(differentUserId);

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var query =
            new GetTranscriptQuery(meeting.Id);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            Error.Forbidden,
            result.Error);

        _transcriptRepositoryMock.Verify(
            x => x.GetByMeetingIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
