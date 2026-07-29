using Xunit;

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

    private readonly UpdateTranscriptCommandHandler _handler;

    public UpdateTranscriptCommandHandlerTests()
    {
        _transcriptRepositoryMock =
            new Mock<ITranscriptRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _handler = new UpdateTranscriptCommandHandler(
            _transcriptRepositoryMock.Object,
            _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateExistingTranscript()
    {
        // Arrange
        var transcript = CreateTranscript();

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
        var meetingId = Guid.NewGuid();

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transcript?)null);

        var command = new UpdateTranscriptCommand(
            meetingId,
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
        var transcript = CreateTranscript();

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
        var transcript = CreateTranscript();

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

    private static Transcript CreateTranscript()
    {
        var result = Transcript.Create(
            Guid.NewGuid(),
            "Original transcript content.",
            "English",
            TimeSpan.FromMinutes(30));

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
