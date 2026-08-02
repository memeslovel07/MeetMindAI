using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Transcripts.DeleteTranscript;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Moq;

namespace MeetMindAI.Application.Tests.Transcripts.DeleteTranscript;

public sealed class DeleteTranscriptCommandHandlerTests
{
    private readonly Mock<ITranscriptRepository> _transcriptRepositoryMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly DeleteTranscriptCommandHandler _handler;

    public DeleteTranscriptCommandHandlerTests()
    {
        _transcriptRepositoryMock =
            new Mock<ITranscriptRepository>();

        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _handler = new DeleteTranscriptCommandHandler(
            _transcriptRepositoryMock.Object,
            _meetingRepositoryMock.Object,
            _dbContextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOwnerDeletesExistingTranscript_ShouldSucceed()
    {
        // Arrange
        var meeting = CreateOwnedMeeting();
        var transcript = CreateTranscript(meeting.Id);

        SetupMeeting(meeting);

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command =
            new DeleteTranscriptCommand(meeting.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            transcript.Id,
            result.Value.TranscriptId);

        _transcriptRepositoryMock.Verify(
            x => x.Remove(transcript),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTranscriptDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var meeting = CreateOwnedMeeting();

        SetupMeeting(meeting);

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transcript?)null);

        var command =
            new DeleteTranscriptCommand(meeting.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            TranscriptErrors.NotFound,
            result.Error);

        _transcriptRepositoryMock.Verify(
            x => x.Remove(It.IsAny<Transcript>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var command =
            new DeleteTranscriptCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            command,
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
            x => x.Remove(It.IsAny<Transcript>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotOwnMeeting_ShouldReturnForbidden()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var meetingResult = Meeting.Create(
            "Private Meeting",
            null,
            ownerId,
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(meetingResult.IsSuccess);

        var meeting = meetingResult.Value;

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(differentUserId);

        SetupMeeting(meeting);

        var command =
            new DeleteTranscriptCommand(meeting.Id);

        // Act
        var result = await _handler.Handle(
            command,
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

        _transcriptRepositoryMock.Verify(
            x => x.Remove(It.IsAny<Transcript>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private Meeting CreateOwnedMeeting()
    {
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        var result = Meeting.Create(
            "Transcript Test Meeting",
            null,
            organizerId,
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private void SetupMeeting(Meeting meeting)
    {
        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
    }

    private static Transcript CreateTranscript(
        Guid meetingId)
    {
        var result = Transcript.Create(
            meetingId,
            "Transcript to delete.",
            "English",
            TimeSpan.FromMinutes(30));

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
