using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Domain.Errors;

namespace MeetMindAI.Domain.Tests.Meetings;

public sealed class ActionItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreatePendingManualActionItem()
    {
        var meetingId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(2);

        var result = ActionItem.Create(
            meetingId,
            "Finish documentation",
            "Complete the V1 documentation.",
            ActionItemPriority.High,
            dueDate);

        Assert.True(result.IsSuccess);

        var actionItem = result.Value;

        Assert.NotEqual(Guid.Empty, actionItem.Id);
        Assert.Equal(meetingId, actionItem.MeetingId);
        Assert.Equal("Finish documentation", actionItem.Title);
        Assert.Equal(
            "Complete the V1 documentation.",
            actionItem.Description);
        Assert.Equal(ActionItemPriority.High, actionItem.Priority);
        Assert.Equal(ActionItemStatus.Pending, actionItem.Status);
        Assert.Equal(ActionItemSource.Manual, actionItem.Source);
        Assert.Equal(dueDate, actionItem.DueDate);
        Assert.Null(actionItem.CompletedAt);
    }

    [Fact]
    public void Create_WithAiSource_ShouldPreserveSource()
    {
        var result = ActionItem.Create(
            Guid.NewGuid(),
            "Test attachment upload",
            null,
            ActionItemPriority.Medium,
            null,
            ActionItemSource.AiGenerated);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            ActionItemSource.AiGenerated,
            result.Value.Source);
    }

    [Fact]
    public void Create_ShouldTrimTitleAndDescription()
    {
        var result = ActionItem.Create(
            Guid.NewGuid(),
            "  Finish testing  ",
            "  Run all domain tests.  ",
            ActionItemPriority.Medium,
            null);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            "Finish testing",
            result.Value.Title);
        Assert.Equal(
            "Run all domain tests.",
            result.Value.Description);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldFail()
    {
        var result = ActionItem.Create(
            Guid.NewGuid(),
            "   ",
            null,
            ActionItemPriority.Low,
            null);

        Assert.True(result.IsFailure);
        Assert.Equal(
            ActionItemErrors.TitleRequired,
            result.Error);
    }

    [Fact]
    public void Create_WithPastDueDate_ShouldFail()
    {
        var result = ActionItem.Create(
            Guid.NewGuid(),
            "Prepare report",
            null,
            ActionItemPriority.High,
            DateTime.UtcNow.AddDays(-2));

        Assert.True(result.IsFailure);
        Assert.Equal(
            ActionItemErrors.InvalidDueDate,
            result.Error);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateActionItem()
    {
        var actionItem = CreateActionItem();

        var dueDate = DateTime.UtcNow.AddDays(5);

        var result = actionItem.Update(
            "Updated task",
            "Updated description",
            ActionItemPriority.Critical,
            dueDate);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated task", actionItem.Title);
        Assert.Equal(
            "Updated description",
            actionItem.Description);
        Assert.Equal(
            ActionItemPriority.Critical,
            actionItem.Priority);
        Assert.Equal(dueDate, actionItem.DueDate);
    }

    [Fact]
    public void MarkCompleted_PendingItem_ShouldCompleteItem()
    {
        var actionItem = CreateActionItem();

        var before = DateTime.UtcNow;

        var result = actionItem.MarkCompleted();

        var after = DateTime.UtcNow;

        Assert.True(result.IsSuccess);
        Assert.Equal(
            ActionItemStatus.Completed,
            actionItem.Status);

        Assert.NotNull(actionItem.CompletedAt);

        Assert.InRange(
            actionItem.CompletedAt.Value,
            before,
            after);
    }

    [Fact]
    public void MarkCompleted_AlreadyCompletedItem_ShouldFail()
    {
        var actionItem = CreateActionItem();

        var firstResult = actionItem.MarkCompleted();

        Assert.True(firstResult.IsSuccess);

        var completedAt = actionItem.CompletedAt;

        var secondResult = actionItem.MarkCompleted();

        Assert.True(secondResult.IsFailure);
        Assert.Equal(
            ActionItemErrors.AlreadyCompleted,
            secondResult.Error);

        Assert.Equal(
            completedAt,
            actionItem.CompletedAt);
    }

    [Fact]
    public void Reopen_CompletedItem_ShouldReturnItemToPending()
    {
        var actionItem = CreateActionItem();

        var completeResult = actionItem.MarkCompleted();

        Assert.True(completeResult.IsSuccess);

        actionItem.Reopen();

        Assert.Equal(
            ActionItemStatus.Pending,
            actionItem.Status);

        Assert.Null(actionItem.CompletedAt);
    }

    [Fact]
    public void Delete_ValidItem_ShouldSoftDeleteItem()
    {
        var actionItem = CreateActionItem();

        var deletedBy = Guid.NewGuid();
        var deletedAtUtc = DateTime.UtcNow;

        var result = actionItem.Delete(
            deletedBy,
            deletedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.True(actionItem.IsDeleted);
        Assert.Equal(
            deletedBy,
            actionItem.DeletedBy);
        Assert.Equal(
            deletedAtUtc,
            actionItem.DeletedAtUtc);
    }

    [Fact]
    public void Delete_AlreadyDeletedItem_ShouldFail()
    {
        var actionItem = CreateActionItem();

        var firstResult = actionItem.Delete(
            Guid.NewGuid(),
            DateTime.UtcNow);

        Assert.True(firstResult.IsSuccess);

        var secondResult = actionItem.Delete(
            Guid.NewGuid(),
            DateTime.UtcNow);

        Assert.True(secondResult.IsFailure);
        Assert.Equal(
            ActionItemErrors.AlreadyDeleted,
            secondResult.Error);
    }

    private static ActionItem CreateActionItem()
    {
        var result = ActionItem.Create(
            Guid.NewGuid(),
            "Complete MeetMindAI",
            "Finish V1 testing.",
            ActionItemPriority.Medium,
            DateTime.UtcNow.AddDays(2));

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
