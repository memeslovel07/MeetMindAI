using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.ActionItems.UpdateActionItem;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.ActionItems.UpdateActionItem;

public sealed class UpdateActionItemCommandHandlerTests
{
    private readonly Mock<IActionItemRepository>
        _actionItemRepositoryMock;

    private readonly Mock<IApplicationDbContext>
        _dbContextMock;

    private readonly UpdateActionItemCommandHandler _handler;

    public UpdateActionItemCommandHandlerTests()
    {
        _actionItemRepositoryMock =
            new Mock<IActionItemRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _handler = new UpdateActionItemCommandHandler(
            _actionItemRepositoryMock.Object,
            _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateActionItem()
    {
        // Arrange
        var actionItem = CreateActionItem();

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItem.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actionItem);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dueDate =
            DateTime.UtcNow.Date.AddDays(7);

        var command = new UpdateActionItemCommand(
            actionItem.Id,
            "Updated action item",
            "Updated description.",
            ActionItemPriority.High,
            dueDate);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            "Updated action item",
            actionItem.Title);

        Assert.Equal(
            "Updated description.",
            actionItem.Description);

        Assert.Equal(
            ActionItemPriority.High,
            actionItem.Priority);

        Assert.Equal(
            dueDate,
            actionItem.DueDate);

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
        var actionItemId = Guid.NewGuid();

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionItem?)null);

        var command = new UpdateActionItemCommand(
            actionItemId,
            "Updated item",
            null,
            ActionItemPriority.Medium,
            null);

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
    public async Task Handle_WithInvalidTitle_ShouldReturnDomainError()
    {
        // Arrange
        var actionItem = CreateActionItem();

        var originalTitle =
            actionItem.Title;

        var originalDescription =
            actionItem.Description;

        var originalPriority =
            actionItem.Priority;

        var originalDueDate =
            actionItem.DueDate;

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItem.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actionItem);

        var command = new UpdateActionItemCommand(
            actionItem.Id,
            "   ",
            "Should not be applied.",
            ActionItemPriority.High,
            DateTime.UtcNow.Date.AddDays(5));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            ActionItemErrors.TitleRequired,
            result.Error);

        // Failed validation must not partially
        // mutate the entity.
        Assert.Equal(
            originalTitle,
            actionItem.Title);

        Assert.Equal(
            originalDescription,
            actionItem.Description);

        Assert.Equal(
            originalPriority,
            actionItem.Priority);

        Assert.Equal(
            originalDueDate,
            actionItem.DueDate);

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
    public async Task Handle_WithValidRequest_ShouldNeverAddNewActionItem()
    {
        // Arrange
        var actionItem = CreateActionItem();

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItem.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actionItem);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateActionItemCommand(
            actionItem.Id,
            "Changed existing item",
            null,
            ActionItemPriority.Medium,
            null);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _actionItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<ActionItem>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _actionItemRepositoryMock.Verify(
            x => x.Update(actionItem),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static ActionItem CreateActionItem()
    {
        var result = ActionItem.Create(
            Guid.NewGuid(),
            "Original action item",
            "Original description.",
            ActionItemPriority.Low,
            DateTime.UtcNow.Date.AddDays(3));

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
