using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Features.ActionItems.GetMeetingActionItems;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.ActionItems.GetMeetingActionItems;

public sealed class GetMeetingActionItemsQueryHandlerTests
{
    private readonly Mock<IActionItemRepository>
        _actionItemRepositoryMock;

    private readonly Mock<IMeetingRepository>
        _meetingRepositoryMock;

    private readonly GetMeetingActionItemsQueryHandler _handler;

    public GetMeetingActionItemsQueryHandlerTests()
    {
        _actionItemRepositoryMock =
            new Mock<IActionItemRepository>();

        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _handler = new GetMeetingActionItemsQueryHandler(
            _actionItemRepositoryMock.Object,
            _meetingRepositoryMock.Object);
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

        var query =
            new GetMeetingActionItemsQuery(meetingId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            MeetingErrors.NotFound,
            result.Error);

        _meetingRepositoryMock.Verify(
            x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _actionItemRepositoryMock.Verify(
            x => x.GetByMeetingIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenActionItemsExist_ShouldReturnMappedActionItems()
    {
        // Arrange
        var meeting = CreateMeeting();

        var firstActionItem = CreateActionItem(
            meeting.Id,
            "Prepare project report",
            "Prepare the final project report.",
            ActionItemPriority.High,
            DateTime.UtcNow.Date.AddDays(5));

        var secondActionItem = CreateActionItem(
            meeting.Id,
            "Schedule client review",
            null,
            ActionItemPriority.Medium,
            null);

        // Complete the second item so we can also
        // verify Status and CompletedAt mapping.
        var completeResult =
            secondActionItem.MarkCompleted();

        Assert.True(completeResult.IsSuccess);

        IReadOnlyList<ActionItem> actionItems =
            new List<ActionItem>
            {
                firstActionItem,
                secondActionItem
            };

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _actionItemRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actionItems);

        var query =
            new GetMeetingActionItemsQuery(
                meeting.Id);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        var firstResponse =
            result.Value[0];

        Assert.Equal(
            firstActionItem.Id,
            firstResponse.Id);

        Assert.Equal(
            meeting.Id,
            firstResponse.MeetingId);

        Assert.Equal(
            "Prepare project report",
            firstResponse.Title);

        Assert.Equal(
            "Prepare the final project report.",
            firstResponse.Description);

        Assert.Equal(
            ActionItemPriority.High,
            firstResponse.Priority);

        Assert.Equal(
            ActionItemStatus.Pending,
            firstResponse.Status);

        Assert.Null(
            firstResponse.AssignedUserId);

        Assert.Equal(
            firstActionItem.DueDate,
            firstResponse.DueDate);

        Assert.Null(
            firstResponse.CompletedAt);

        Assert.Equal(
            firstActionItem.CreatedAtUtc,
            firstResponse.CreatedAtUtc);

        Assert.Equal(
            firstActionItem.UpdatedAtUtc,
            firstResponse.UpdatedAtUtc);

        var secondResponse =
            result.Value[1];

        Assert.Equal(
            secondActionItem.Id,
            secondResponse.Id);

        Assert.Equal(
            "Schedule client review",
            secondResponse.Title);

        Assert.Equal(
            ActionItemStatus.Completed,
            secondResponse.Status);

        Assert.Equal(
            secondActionItem.CompletedAt,
            secondResponse.CompletedAt);

        Assert.NotNull(
            secondResponse.CompletedAt);

        _meetingRepositoryMock.Verify(
            x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _actionItemRepositoryMock.Verify(
            x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMeetingHasNoActionItems_ShouldReturnEmptyCollection()
    {
        // Arrange
        var meeting = CreateMeeting();

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _actionItemRepositoryMock
            .Setup(x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<ActionItem>());

        var query =
            new GetMeetingActionItemsQuery(
                meeting.Id);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);

        _meetingRepositoryMock.Verify(
            x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _actionItemRepositoryMock.Verify(
            x => x.GetByMeetingIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Meeting CreateMeeting()
    {
        var result = Meeting.Create(
            "Action Items Test Meeting",
            null,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            60);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static ActionItem CreateActionItem(
        Guid meetingId,
        string title,
        string? description,
        ActionItemPriority priority,
        DateTime? dueDate)
    {
        var result = ActionItem.Create(
            meetingId,
            title,
            description,
            priority,
            dueDate);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
