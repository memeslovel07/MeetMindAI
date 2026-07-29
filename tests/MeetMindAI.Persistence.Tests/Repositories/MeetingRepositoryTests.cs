using Xunit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Persistence.Persistence;
using MeetMindAI.Persistence.Persistence.Repositories;

namespace MeetMindAI.Persistence.Tests.Repositories;

public sealed class MeetingRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly MeetingRepository _repository;

    public MeetingRepositoryTests()
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
            new MeetingRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMeetingExists_ShouldReturnMeeting()
    {
        // Arrange
        var organizer = CreateUser();

        var meeting = CreateMeeting(
            organizer.Id,
            "Persistence Test Meeting");

        await _context.Users.AddAsync(organizer);
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByIdAsync(meeting.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(meeting.Id, result.Id);
        Assert.Equal(
            "Persistence Test Meeting",
            result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMeetingDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result =
            await _repository.GetByIdAsync(
                Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMeetingIsDeleted_ShouldReturnNull()
    {
        // Arrange
        var organizer = CreateUser();

        var meeting = CreateMeeting(
            organizer.Id,
            "Deleted Meeting");

        var deleteResult = meeting.Delete(
            organizer.Id,
            DateTime.UtcNow);

        Assert.True(deleteResult.IsSuccess);

        await _context.Users.AddAsync(organizer);
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByIdAsync(
                meeting.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByOrganizerIdAsync_ShouldReturnOnlyOrganizersMeetings()
    {
        // Arrange
        var organizer = CreateUser();
        var otherOrganizer = CreateUser();

        var firstMeeting = CreateMeeting(
            organizer.Id,
            "First Meeting");

        var secondMeeting = CreateMeeting(
            organizer.Id,
            "Second Meeting");

        var otherMeeting = CreateMeeting(
            otherOrganizer.Id,
            "Other User Meeting");

        await _context.Users.AddRangeAsync(
            organizer,
            otherOrganizer);

        await _context.Meetings.AddRangeAsync(
            firstMeeting,
            secondMeeting,
            otherMeeting);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByOrganizerIdAsync(
                organizer.Id);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            item =>
                Assert.Equal(
                    organizer.Id,
                    item.OrganizerId));

        Assert.DoesNotContain(
            result,
            item => item.Id == otherMeeting.Id);
    }

    [Fact]
    public async Task GetByOrganizerIdAsync_ShouldExcludeDeletedMeetings()
    {
        // Arrange
        var organizer = CreateUser();

        var activeMeeting = CreateMeeting(
            organizer.Id,
            "Active Meeting");

        var deletedMeeting = CreateMeeting(
            organizer.Id,
            "Deleted Meeting");

        var deleteResult = deletedMeeting.Delete(
            organizer.Id,
            DateTime.UtcNow);

        Assert.True(deleteResult.IsSuccess);

        await _context.Users.AddAsync(organizer);

        await _context.Meetings.AddRangeAsync(
            activeMeeting,
            deletedMeeting);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByOrganizerIdAsync(
                organizer.Id);

        // Assert
        Assert.Single(result);

        Assert.Equal(
            activeMeeting.Id,
            result[0].Id);

        Assert.DoesNotContain(
            result,
            item => item.Id == deletedMeeting.Id);
    }

    //[Fact]
    //public async Task GetByOrganizerIdAsync_ShouldReturnNewestMeetingsFirst()
    //{
    //    // Arrange
    //    var organizer = CreateUser();

    //    await _context.Users.AddAsync(organizer);
    //    await _context.SaveChangesAsync();

    //    var olderMeeting = CreateMeeting(
    //        organizer.Id,
    //        "Older Meeting");

    //    await _context.Meetings.AddAsync(
    //        olderMeeting);

    //    await _context.SaveChangesAsync();

    //    await Task.Delay(20);

    //    var newerMeeting = CreateMeeting(
    //        organizer.Id,
    //        "Newer Meeting");

    //    await _context.Meetings.AddAsync(
    //        newerMeeting);

    //    await _context.SaveChangesAsync();

    //    // Act
    //    var result =
    //        await _repository.GetByOrganizerIdAsync(
    //            organizer.Id);

    //    // Assert
    //    Assert.Equal(2, result.Count);

    //    Assert.Equal(
    //        newerMeeting.Id,
    //        result[0].Id);

    //    Assert.Equal(
    //        olderMeeting.Id,
    //        result[1].Id);


    //    Console.WriteLine(
    //$"Older: {olderMeeting.CreatedAtUtc:O}");

    //    Console.WriteLine(
    //        $"Newer: {newerMeeting.CreatedAtUtc:O}");

    //    Console.WriteLine(
    //        $"Result 0: {result[0].Title} - {result[0].CreatedAtUtc:O}");

    //    Console.WriteLine(
    //        $"Result 1: {result[1].Title} - {result[1].CreatedAtUtc:O}");

    //}

    private static User CreateUser()
    {
        var result = User.Create(
            $"test-{Guid.NewGuid():N}@meetmind.test",
            "test-password-hash",
            "Test",
            "User");

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static Meeting CreateMeeting(
        Guid organizerId,
        string title)
    {
        var result = Meeting.Create(
            title,
            null,
            organizerId,
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }


}
