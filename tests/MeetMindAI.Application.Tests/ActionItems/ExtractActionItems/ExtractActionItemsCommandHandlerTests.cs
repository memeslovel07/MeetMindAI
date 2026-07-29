using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.AI;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.ActionItems.Commands.ExtractActionItems;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums.Meetings;

using Moq;

namespace MeetMindAI.Application.Tests.ActionItems.ExtractActionItems;

public sealed class ExtractActionItemsCommandHandlerTests
{
    private readonly Mock<ITranscriptRepository>
        _transcriptRepositoryMock;

    private readonly Mock<IActionItemRepository>
        _actionItemRepositoryMock;

    private readonly Mock<IActionItemExtractionService>
        _extractionServiceMock;

    private readonly Mock<IApplicationDbContext>
        _dbContextMock;

    private readonly ExtractActionItemsCommandHandler _handler;

    public ExtractActionItemsCommandHandlerTests()
    {
        _transcriptRepositoryMock =
            new Mock<ITranscriptRepository>();

        _actionItemRepositoryMock =
            new Mock<IActionItemRepository>();

        _extractionServiceMock =
            new Mock<IActionItemExtractionService>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _handler = new ExtractActionItemsCommandHandler(
            _transcriptRepositoryMock.Object,
            _actionItemRepositoryMock.Object,
            _extractionServiceMock.Object,
            _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTranscriptDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transcript?)null);

        var command =
            new ExtractActionItemsCommand(meetingId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Transcript.NotFound",
            result.Error.Code);

        _extractionServiceMock.Verify(
            x => x.ExtractAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _actionItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<ActionItem>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAiItemsAlreadyExist_ShouldReturnFailure()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        var transcript =
            CreateTranscript(meetingId);

        var aiActionItem =
            CreateActionItem(
                meetingId,
                "Existing AI item",
                ActionItemSource.AiGenerated);

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        _actionItemRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<ActionItem>
                {
                    aiActionItem
                });

        var command =
            new ExtractActionItemsCommand(meetingId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "ActionItems.AlreadyExtracted",
            result.Error.Code);

        _extractionServiceMock.Verify(
            x => x.ExtractAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _actionItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<ActionItem>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAiReturnsNoItems_ShouldReturnEmptySuccess()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        var transcript =
            CreateTranscript(meetingId);

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        _actionItemRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ActionItem>());

        _extractionServiceMock
            .Setup(x => x.ExtractAsync(
                transcript.Content,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<ExtractedActionItem>());

        var command =
            new ExtractActionItemsCommand(meetingId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);

        _extractionServiceMock.Verify(
            x => x.ExtractAsync(
                transcript.Content,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _actionItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<ActionItem>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidAiItems_ShouldCreateAiGeneratedActionItems()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        var transcript =
            CreateTranscript(meetingId);

        var dueDate =
            DateTime.UtcNow.Date.AddDays(5);

        IReadOnlyList<ExtractedActionItem> extractedItems =
            new List<ExtractedActionItem>
            {
                new(
                    "Prepare project report",
                    "Prepare the final report.",
                    ActionItemPriority.High,
                    dueDate),

                new(
                    "Schedule client review",
                    null,
                    ActionItemPriority.Medium,
                    null)
            };

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        _actionItemRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ActionItem>());

        _extractionServiceMock
            .Setup(x => x.ExtractAsync(
                transcript.Content,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedItems);

        _actionItemRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<ActionItem>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var command =
            new ExtractActionItemsCommand(meetingId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            2,
            result.Value.Count);

        Assert.All(
            result.Value,
            id => Assert.NotEqual(Guid.Empty, id));

        _actionItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ActionItem>(item =>
                    item.MeetingId == meetingId &&
                    item.Source ==
                        ActionItemSource.AiGenerated),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidAndValidAiItems_ShouldSkipInvalidItem()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        var transcript =
            CreateTranscript(meetingId);

        IReadOnlyList<ExtractedActionItem> extractedItems =
            new List<ExtractedActionItem>
            {
                new(
                    "   ",
                    "Invalid because title is empty.",
                    ActionItemPriority.High,
                    null),

                new(
                    "Valid AI action item",
                    "This item should be persisted.",
                    ActionItemPriority.Medium,
                    null)
            };

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        _actionItemRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ActionItem>());

        _extractionServiceMock
            .Setup(x => x.ExtractAsync(
                transcript.Content,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedItems);

        _actionItemRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<ActionItem>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command =
            new ExtractActionItemsCommand(meetingId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Single(result.Value);

        _actionItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ActionItem>(item =>
                    item.Title == "Valid AI action item" &&
                    item.Source ==
                        ActionItemSource.AiGenerated),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithOnlyManualExistingItems_ShouldStillExtractAiItems()
    {
        // Arrange
        var meetingId = Guid.NewGuid();

        var transcript =
            CreateTranscript(meetingId);

        var manualItem =
            CreateActionItem(
                meetingId,
                "Existing manual item",
                ActionItemSource.Manual);

        IReadOnlyList<ExtractedActionItem> extractedItems =
            new List<ExtractedActionItem>
            {
                new(
                    "AI generated task",
                    null,
                    ActionItemPriority.Medium,
                    null)
            };

        _transcriptRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(transcript);

        _actionItemRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<ActionItem>
                {
                    manualItem
                });

        _extractionServiceMock
            .Setup(x => x.ExtractAsync(
                transcript.Content,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedItems);

        _actionItemRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<ActionItem>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command =
            new ExtractActionItemsCommand(meetingId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);

        _extractionServiceMock.Verify(
            x => x.ExtractAsync(
                transcript.Content,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _actionItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ActionItem>(item =>
                    item.Source ==
                        ActionItemSource.AiGenerated),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Transcript CreateTranscript(
        Guid meetingId)
    {
        var result = Transcript.Create(
            meetingId,
            "Somesh will prepare the report. "
            + "The client review should be scheduled next week.",
            "English",
            TimeSpan.FromMinutes(30));

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static ActionItem CreateActionItem(
        Guid meetingId,
        string title,
        ActionItemSource source)
    {
        var result = ActionItem.Create(
            meetingId,
            title,
            null,
            ActionItemPriority.Medium,
            null,
            source);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
