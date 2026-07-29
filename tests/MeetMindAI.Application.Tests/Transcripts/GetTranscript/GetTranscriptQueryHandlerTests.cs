using Xunit;

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

    public GetTranscriptQueryHandlerTests()
    {
        _transcriptRepositoryMock =
            new Mock<ITranscriptRepository>();

        _handler = new GetTranscriptQueryHandler(
            _transcriptRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTranscriptExists_ShouldReturnTranscriptResponse()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        var duration =
            TimeSpan.FromMinutes(42);

        var transcriptResult = Transcript.Create(
            meetingId,
            "This is the meeting transcript content.",
            "English",
            duration);

        Assert.True(transcriptResult.IsSuccess);

        var transcript =
            transcriptResult.Value;

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
        var meetingId =
            Guid.NewGuid();

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transcript?)null);

        var query =
            new GetTranscriptQuery(meetingId);

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
                meetingId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
