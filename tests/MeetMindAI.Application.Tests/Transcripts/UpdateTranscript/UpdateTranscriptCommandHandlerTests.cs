using Xunit;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Shared.Results;
using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Transcripts.UpdateTranscript;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.Transcripts.UpdateTranscript;

public sealed class UpdateTranscriptCommandHandlerTests
{
    private readonly Mock<ITranscriptRepository> _transcriptRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateTranscriptCommandHandler _handler;

    public UpdateTranscriptCommandHandlerTests()
    {
        _transcriptRepositoryMock =
            new Mock<ITranscriptRepository>();

        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _handler = new UpdateTranscriptCommandHandler(
            _transcriptRepositoryMock.Object,
            _meetingRepositoryMock.Object,
            _dbContextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateExistingTranscript()
    {
        // Arrange
        var meeting = CreateOwnedMeeting();
        var transcript = CreateTranscript(meeting.Id);

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                transcript.MeetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateTranscriptCommand(
            transcript.MeetingId,
            "Updated transcript content.",
            "Hindi",
            TimeSpan.FromMinutes(45));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            transcript.Id,
            result.Value.TranscriptId);

        Assert.Equal(
            "Updated transcript content.",
            transcript.Content);

        Assert.Equal(
            "Hindi",
            transcript.Language);

        Assert.Equal(
            TimeSpan.FromMinutes(45),
            transcript.Duration);

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

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transcript?)null);

        var command = new UpdateTranscriptCommand(
            meeting.Id,
            "Updated transcript.",
            "English",
            TimeSpan.FromMinutes(30));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            TranscriptErrors.NotFound,
            result.Error);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidContent_ShouldReturnDomainError()
    {
        // Arrange
        var meeting = CreateOwnedMeeting();
        var transcript = CreateTranscript(meeting.Id);

        var originalContent =
            transcript.Content;

        var originalLanguage =
            transcript.Language;

        var originalDuration =
            transcript.Duration;

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                transcript.MeetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        var command = new UpdateTranscriptCommand(
            transcript.MeetingId,
            "   ",
            "Hindi",
            TimeSpan.FromMinutes(60));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            TranscriptErrors.EmptyContent,
            result.Error);

        // A failed update must not partially mutate
        // the existing transcript.
        Assert.Equal(
            originalContent,
            transcript.Content);

        Assert.Equal(
            originalLanguage,
            transcript.Language);

        Assert.Equal(
            originalDuration,
            transcript.Duration);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldNeverAddNewTranscript()
    {
        // Arrange
        var meeting = CreateOwnedMeeting();
        var transcript = CreateTranscript(meeting.Id);

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                transcript.MeetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateTranscriptCommand(
            transcript.MeetingId,
            "Changed existing transcript.",
            "English",
            TimeSpan.FromMinutes(40));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _transcriptRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Transcript>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Transcript CreateTranscript(Guid meetingId)
    {
        var result = Transcript.Create(
            meetingId,
            "Original transcript content.",
            "English",
            TimeSpan.FromMinutes(30));

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private Meeting CreateOwnedMeeting()
    {
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        var result = Meeting.Create(
            "Transcript Test Meeting",
            "Meeting for transcript tests.",
            organizerId,
            DateTime.UtcNow.AddDays(1),
            60);

        Assert.True(result.IsSuccess);

        var meeting = result.Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        return meeting;
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var command = new UpdateTranscriptCommand(
            Guid.NewGuid(),
            "Updated transcript.",
            "English",
            TimeSpan.FromMinutes(30));

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
            x => x.GetByMeetingIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
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

        var command = new UpdateTranscriptCommand(
            meeting.Id,
            "Unauthorized modification.",
            "English",
            TimeSpan.FromMinutes(30));

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

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


}
