using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.Transcripts.CreateTranscript;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.Transcripts.CreateTranscript;

public sealed class CreateTranscriptCommandHandlerTests
{
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

        _handler = new CreateTranscriptCommandHandler(
            _transcriptRepositoryMock.Object,
            _meetingRepositoryMock.Object,
            _dbContextMock.Object);
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

        Assert.True(
            existingTranscriptResult.IsSuccess);

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

    private static Meeting CreateMeeting()
    {
        var result = Meeting.Create(
            "Transcript Test Meeting",
            null,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
