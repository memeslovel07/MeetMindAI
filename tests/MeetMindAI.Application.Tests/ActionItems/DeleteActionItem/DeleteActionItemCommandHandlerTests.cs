using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.ActionItems.DeleteActionItem;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.ActionItems.DeleteActionItem;

public sealed class DeleteActionItemCommandHandlerTests
{
    private readonly Mock<IActionItemRepository>
        _actionItemRepositoryMock;

    private readonly Mock<IApplicationDbContext>
        _dbContextMock;

    private readonly DeleteActionItemCommandHandler _handler;

    public DeleteActionItemCommandHandlerTests()
    {
        _actionItemRepositoryMock =
            new Mock<IActionItemRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _handler = new DeleteActionItemCommandHandler(
            _actionItemRepositoryMock.Object,
            _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldSoftDeleteActionItem()
    {
        // Arrange
        var actionItem = CreateActionItem();
        var deletedBy = Guid.NewGuid();

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItem.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actionItem);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteActionItemCommand(
            actionItem.Id,
            deletedBy);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.True(actionItem.IsDeleted);

        Assert.Equal(
            deletedBy,
            actionItem.DeletedBy);

        Assert.NotNull(
            actionItem.DeletedAtUtc);

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

        var command = new DeleteActionItemCommand(
            actionItemId,
            Guid.NewGuid());

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
    public async Task Handle_WhenAlreadyDeleted_ShouldReturnDomainError()
    {
        // Arrange
        var actionItem = CreateActionItem();

        var firstDeletedBy = Guid.NewGuid();

        var firstDeleteResult = actionItem.Delete(
            firstDeletedBy,
            DateTime.UtcNow.AddMinutes(-5));

        Assert.True(firstDeleteResult.IsSuccess);
        Assert.True(actionItem.IsDeleted);

        var originalDeletedBy =
            actionItem.DeletedBy;

        var originalDeletedAtUtc =
            actionItem.DeletedAtUtc;

        _actionItemRepositoryMock
            .Setup(x => x.GetByIdAsync(
                actionItem.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actionItem);

        var command = new DeleteActionItemCommand(
            actionItem.Id,
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            ActionItemErrors.AlreadyDeleted,
            result.Error);

        Assert.Equal(
            originalDeletedBy,
            actionItem.DeletedBy);

        Assert.Equal(
            originalDeletedAtUtc,
            actionItem.DeletedAtUtc);

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
    public async Task Handle_WithValidRequest_ShouldNeverPhysicallyRemoveActionItem()
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

        var command = new DeleteActionItemCommand(
            actionItem.Id,
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(actionItem.IsDeleted);

        _actionItemRepositoryMock.Verify(
            x => x.Remove(
                It.IsAny<ActionItem>()),
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
            "Delete test action item",
            "Action item used for deletion tests.",
            ActionItemPriority.Medium,
            DateTime.UtcNow.Date.AddDays(3));

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
