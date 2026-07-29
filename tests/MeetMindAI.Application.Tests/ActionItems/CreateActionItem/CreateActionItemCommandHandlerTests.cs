using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.ActionItems.CreateActionItem;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.ActionItems.CreateActionItem;

public sealed class CreateActionItemCommandHandlerTests
{
    private readonly Mock<IActionItemRepository> _actionItemRepositoryMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;

    private readonly CreateActionItemCommandHandler _handler;

    public CreateActionItemCommandHandlerTests()
    {
        _actionItemRepositoryMock =
            new Mock<IActionItemRepository>();

        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _handler = new CreateActionItemCommandHandler(
            _actionItemRepositoryMock.Object,
            _meetingRepositoryMock.Object,
            _dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateManualActionItem()
    {
        // Arrange
        var meeting = CreateMeeting();

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _actionItemRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<ActionItem>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dueDate =
            DateTime.UtcNow.AddDays(5);

        var command = new CreateActionItemCommand(
            meeting.Id,
            "Prepare project report",
            "Prepare the final project report.",
            ActionItemPriority.High,
            dueDate);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);

        _actionItemRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<ActionItem>(item =>
                    item.MeetingId == meeting.Id &&
                    item.Title == "Prepare project report" &&
                    item.Description ==
                        "Prepare the final project report." &&
                    item.Priority == ActionItemPriority.High &&
                    item.DueDate == dueDate &&
                    item.Status == ActionItemStatus.Pending &&
                    item.Source == ActionItemSource.Manual),
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

        var command = new CreateActionItemCommand(
            meetingId,
            "Prepare report",
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
            MeetingErrors.NotFound,
            result.Error);

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
    public async Task Handle_WithInvalidTitle_ShouldReturnDomainError()
    {
        // Arrange
        var meeting = CreateMeeting();

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var command = new CreateActionItemCommand(
            meeting.Id,
            "   ",
            null,
            ActionItemPriority.Low,
            null);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            ActionItemErrors.TitleRequired,
            result.Error);

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
    public async Task Handle_WithPastDueDate_ShouldReturnDomainError()
    {
        // Arrange
        var meeting = CreateMeeting();

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var command = new CreateActionItemCommand(
            meeting.Id,
            "Prepare report",
            null,
            ActionItemPriority.Medium,
            DateTime.UtcNow.Date.AddDays(-1));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            ActionItemErrors.InvalidDueDate,
            result.Error);

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

    private static Meeting CreateMeeting()
    {
        var result = Meeting.Create(
            "Action Item Test Meeting",
            null,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
