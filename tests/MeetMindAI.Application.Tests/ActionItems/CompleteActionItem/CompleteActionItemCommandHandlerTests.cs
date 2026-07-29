using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.ActionItems.CompleteActionItem;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.ActionItems.CompleteActionItem;

public sealed class CompleteActionItemCommandHandlerTests
{
    private readonly Mock<IActionItemRepository>
        _actionItemRepositoryMock;

    private readonly Mock<IApplicationDbContext>
        _dbContextMock;

    private readonly CompleteActionItemCommandHandler _handler;

    public CompleteActionItemCommandHandlerTests()
    {
        _actionItemRepositoryMock =
            new Mock<IActionItemRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _handler = new CompleteActionItemCommandHandler(
            _actionItemRepositoryMock.Object,
            _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithPendingActionItem_ShouldMarkCompleted()
    {
        // Arrange
        var actionItem = CreateActionItem();

        Assert.Equal(
            ActionItemStatus.Pending,
            actionItem.Status);

        Assert.Null(
            actionItem.CompletedAt);

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItem.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actionItem);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command =
            new CompleteActionItemCommand(
                actionItem.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            ActionItemStatus.Completed,
            actionItem.Status);

        Assert.NotNull(
            actionItem.CompletedAt);

        _actionItemRepositoryMock.Verify(
            x => x.Update(actionItem),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenActionItemDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var actionItemId =
            Guid.NewGuid();

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionItem?)null);

        var command =
            new CompleteActionItemCommand(
                actionItemId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            ActionItemErrors.NotFound,
            result.Error);

        _actionItemRepositoryMock.Verify(
            x => x.Update(
                It.IsAny<ActionItem>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAlreadyCompleted_ShouldReturnDomainError()
    {
        // Arrange
        var actionItem =
            CreateActionItem();

        var firstCompleteResult =
            actionItem.MarkCompleted();

        Assert.True(
            firstCompleteResult.IsSuccess);

        Assert.Equal(
            ActionItemStatus.Completed,
            actionItem.Status);

        var completedAt =
            actionItem.CompletedAt;

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItem.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actionItem);

        var command =
            new CompleteActionItemCommand(
                actionItem.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            ActionItemErrors.AlreadyCompleted,
            result.Error);

        Assert.Equal(
            ActionItemStatus.Completed,
            actionItem.Status);

        Assert.Equal(
            completedAt,
            actionItem.CompletedAt);

        _actionItemRepositoryMock.Verify(
            x => x.Update(
                It.IsAny<ActionItem>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static ActionItem CreateActionItem()
    {
        var result = ActionItem.Create(
            Guid.NewGuid(),
            "Complete project report",
            "Finish the final report.",
            ActionItemPriority.Medium,
            DateTime.UtcNow.Date.AddDays(3));

        Assert.True(
            result.IsSuccess);

        return result.Value;
    }
}
