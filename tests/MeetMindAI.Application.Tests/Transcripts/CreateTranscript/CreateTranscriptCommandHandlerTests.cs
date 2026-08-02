using Xunit;

using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Shared.Results;
using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.Transcripts.CreateTranscript;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.Transcripts.CreateTranscript;

public sealed class CreateTranscriptCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ITranscriptRepository> _transcriptRepositoryMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;

    private readonly CreateTranscriptCommandHandler _handler;

    public CreateTranscriptCommandHandlerTests()
    {
        _transcriptRepositoryMock =
            new Mock<ITranscriptRepository>();

        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _handler = new CreateTranscriptCommandHandler(
            _transcriptRepositoryMock.Object,
            _meetingRepositoryMock.Object,
            _dbContextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateTranscript()
    {
        // Arrange
        var meeting = CreateMeeting();

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

        _transcriptRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Transcript>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateTranscriptCommand(
            meeting.Id,
            "This is the meeting transcript.",
            "English",
            TimeSpan.FromMinutes(30));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotEqual(
            Guid.Empty,
            result.Value.TranscriptId);

        _transcriptRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Transcript>(transcript =>
                    transcript.MeetingId == meeting.Id &&
                    transcript.Content ==
                        "This is the meeting transcript." &&
                    transcript.Language == "English" &&
                    transcript.Duration ==
                        TimeSpan.FromMinutes(30)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMeetingDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        var meetingId = Guid.NewGuid();

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meeting?)null);

        var command = new CreateTranscriptCommand(
            meetingId,
            "Transcript content.",
            "English",
            TimeSpan.FromMinutes(30));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            MeetingErrors.NotFound,
            result.Error);

        _transcriptRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Transcript>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTranscriptAlreadyExists_ShouldReturnAlreadyExists()
    {
        // Arrange
        var meeting = CreateMeeting();

        var existingTranscriptResult =
            Transcript.Create(
                meeting.Id,
                "Existing transcript.",
                "English",
                TimeSpan.FromMinutes(20));

        Assert.True(existingTranscriptResult.IsSuccess);

        var existingTranscript =
            existingTranscriptResult.Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTranscript);

        var command = new CreateTranscriptCommand(
            meeting.Id,
            "Another transcript.",
            "English",
            TimeSpan.FromMinutes(30));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            TranscriptErrors.AlreadyExists,
            result.Error);

        _transcriptRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Transcript>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidTranscript_ShouldReturnDomainError()
    {
        // Arrange
        var meeting = CreateMeeting();

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

        var command = new CreateTranscriptCommand(
            meeting.Id,
            "   ",
            "English",
            TimeSpan.FromMinutes(30));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
     TranscriptErrors.EmptyContent,
     result.Error);

        _transcriptRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Transcript>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private Meeting CreateMeeting()
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
            30);

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

        var command = new CreateTranscriptCommand(
            meeting.Id,
            "I should not be allowed to create this.",
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

        _transcriptRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Transcript>(),
                It.IsAny<CancellationToken>()),
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

        var command = new CreateTranscriptCommand(
            Guid.NewGuid(),
            "Transcript content.",
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
            x => x.AddAsync(
                It.IsAny<Transcript>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
