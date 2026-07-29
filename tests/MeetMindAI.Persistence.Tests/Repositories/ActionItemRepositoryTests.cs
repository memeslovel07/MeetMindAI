using Xunit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Persistence.Persistence;
using MeetMindAI.Persistence.Persistence.Repositories;

namespace MeetMindAI.Persistence.Tests.Repositories;

public sealed class ActionItemRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly ActionItemRepository _repository;

    public ActionItemRepositoryTests()
    {
        _connection = new SqliteConnection(
            "DataSource=:memory:");

        _connection.Open();

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

        _context =
            new ApplicationDbContext(options);

        _context.Database.EnsureCreated();

        _repository =
            new ActionItemRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenActionItemExists_ShouldReturnActionItem()
    {
        // Arrange
        var meeting = await CreateAndPersistMeetingAsync();

        var actionItem = CreateActionItem(
            meeting.Id,
            "Prepare report",
            ActionItemPriority.High);

        await _context.ActionItems.AddAsync(actionItem);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByIdAsync(actionItem.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(actionItem.Id, result.Id);
        Assert.Equal("Prepare report", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenActionItemDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result =
            await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenActionItemIsDeleted_ShouldReturnNull()
    {
        // Arrange
        var meeting = await CreateAndPersistMeetingAsync();

        var actionItem = CreateActionItem(
            meeting.Id,
            "Deleted item",
            ActionItemPriority.Medium);

        var deleteResult =
            actionItem.Delete(
                null,
                DateTime.UtcNow);

        Assert.True(deleteResult.IsSuccess);

        await _context.ActionItems.AddAsync(actionItem);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByIdAsync(actionItem.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_ShouldReturnOnlyMeetingActionItems()
    {
        // Arrange
        var firstMeeting =
            await CreateAndPersistMeetingAsync();

        var secondMeeting =
            await CreateAndPersistMeetingAsync();

        var firstItem = CreateActionItem(
            firstMeeting.Id,
            "First item",
            ActionItemPriority.High);

        var secondItem = CreateActionItem(
            firstMeeting.Id,
            "Second item",
            ActionItemPriority.Medium);

        var otherItem = CreateActionItem(
            secondMeeting.Id,
            "Other meeting item",
            ActionItemPriority.High);

        await _context.ActionItems.AddRangeAsync(
            firstItem,
            secondItem,
            otherItem);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                firstMeeting.Id);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            item => Assert.Equal(
                firstMeeting.Id,
                item.MeetingId));

        Assert.DoesNotContain(
            result,
            item => item.Id == otherItem.Id);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_ShouldExcludeDeletedActionItems()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var activeItem = CreateActionItem(
            meeting.Id,
            "Active item",
            ActionItemPriority.High);

        var deletedItem = CreateActionItem(
            meeting.Id,
            "Deleted item",
            ActionItemPriority.High);

        var deleteResult =
            deletedItem.Delete(
                null,
                DateTime.UtcNow);

        Assert.True(deleteResult.IsSuccess);

        await _context.ActionItems.AddRangeAsync(
            activeItem,
            deletedItem);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(activeItem.Id, result[0].Id);

        Assert.DoesNotContain(
            result,
            item => item.Id == deletedItem.Id);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_ShouldOrderPendingBeforeCompleted()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var completedItem = CreateActionItem(
            meeting.Id,
            "Completed",
            ActionItemPriority.High);

        var completeResult =
            completedItem.MarkCompleted();

        Assert.True(completeResult.IsSuccess);

        var pendingItem = CreateActionItem(
            meeting.Id,
            "Pending",
            ActionItemPriority.Low);

        await _context.ActionItems.AddRangeAsync(
            completedItem,
            pendingItem);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(
            ActionItemStatus.Pending,
            result[0].Status);

        Assert.Equal(
            ActionItemStatus.Completed,
            result[1].Status);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_ShouldOrderHigherPriorityFirst()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var lowPriority = CreateActionItem(
            meeting.Id,
            "Low priority",
            ActionItemPriority.Low);

        var highPriority = CreateActionItem(
            meeting.Id,
            "High priority",
            ActionItemPriority.High);

        await _context.ActionItems.AddRangeAsync(
            lowPriority,
            highPriority);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(
            ActionItemPriority.High,
            result[0].Priority);

        Assert.Equal(
            ActionItemPriority.Low,
            result[1].Priority);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_ShouldOrderEarlierDueDateFirst()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var laterItem = CreateActionItem(
            meeting.Id,
            "Later item",
            ActionItemPriority.Medium,
            DateTime.UtcNow.Date.AddDays(10));

        var earlierItem = CreateActionItem(
            meeting.Id,
            "Earlier item",
            ActionItemPriority.Medium,
            DateTime.UtcNow.Date.AddDays(2));

        await _context.ActionItems.AddRangeAsync(
            laterItem,
            earlierItem);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(
            earlierItem.Id,
            result[0].Id);

        Assert.Equal(
            laterItem.Id,
            result[1].Id);
    }

    private async Task<Meeting> CreateAndPersistMeetingAsync()
    {
        var userResult = User.Create(
            $"actionitem-{Guid.NewGuid():N}@meetmind.test",
            "test-password-hash",
            "Test",
            "User");

        Assert.True(userResult.IsSuccess);

        var user = userResult.Value;

        var meetingResult = Meeting.Create(
            "Action Item Repository Test",
            null,
            user.Id,
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(meetingResult.IsSuccess);

        var meeting = meetingResult.Value;

        await _context.Users.AddAsync(user);
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();

        return meeting;
    }

    private static ActionItem CreateActionItem(
        Guid meetingId,
        string title,
        ActionItemPriority priority,
        DateTime? dueDate = null)
    {
        var result = ActionItem.Create(
            meetingId,
            title,
            null,
            priority,
            dueDate);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
