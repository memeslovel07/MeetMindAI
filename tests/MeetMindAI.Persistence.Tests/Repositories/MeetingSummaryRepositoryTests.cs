using Xunit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Persistence.Persistence;
using MeetMindAI.Persistence.Persistence.Repositories;

namespace MeetMindAI.Persistence.Tests.Repositories;

public sealed class MeetingSummaryRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly MeetingSummaryRepository _repository;

    public MeetingSummaryRepositoryTests()
    {
        _connection =
            new SqliteConnection("DataSource=:memory:");

        _connection.Open();

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

        _context =
            new ApplicationDbContext(options);

        _context.Database.EnsureCreated();

        _repository =
            new MeetingSummaryRepository(_context);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_WhenSummaryExists_ShouldReturnSummary()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var summary =
            CreateSummary(meeting.Id);

        await _context.MeetingSummaries
            .AddAsync(summary);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            summary.Id,
            result.Id);

        Assert.Equal(
            meeting.Id,
            result.MeetingId);

        Assert.Equal(
            "AI generated meeting summary.",
            result.Summary);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_WhenSummaryDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_ShouldReturnOnlyRequestedMeetingsSummary()
    {
        // Arrange
        var firstMeeting =
            await CreateAndPersistMeetingAsync();

        var secondMeeting =
            await CreateAndPersistMeetingAsync();

        var firstSummary =
            CreateSummary(firstMeeting.Id);

        var secondSummary =
            CreateSummary(secondMeeting.Id);

        await _context.MeetingSummaries.AddRangeAsync(
            firstSummary,
            secondSummary);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                firstMeeting.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            firstSummary.Id,
            result.Id);

        Assert.NotEqual(
            secondSummary.Id,
            result.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistMeetingSummary()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var summary =
            CreateSummary(meeting.Id);

        // Act
        await _repository.AddAsync(summary);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Assert
        var persisted =
            await _context.MeetingSummaries
                .SingleOrDefaultAsync(
                    x => x.Id == summary.Id);

        Assert.NotNull(persisted);

        Assert.Equal(
            "AI generated meeting summary.",
            persisted.Summary);

        Assert.Equal(
            "Gemini",
            persisted.Provider);

        Assert.Equal(
            "gemini-test",
            persisted.Model);

        Assert.Equal(
            "v1",
            persisted.PromptVersion);
    }

    [Fact]
    public async Task Update_ShouldPersistRegeneratedSummary()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var summary =
            CreateSummary(meeting.Id);

        await _context.MeetingSummaries
            .AddAsync(summary);

        await _context.SaveChangesAsync();

        var regenerateResult =
            summary.Regenerate(
                "Regenerated meeting summary.",
                "Gemini",
                "gemini-test-v2",
                "v2");

        Assert.True(regenerateResult.IsSuccess);

        // Act
        _repository.Update(summary);

        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Assert
        var persisted =
            await _context.MeetingSummaries
                .SingleAsync(
                    x => x.Id == summary.Id);

        Assert.Equal(
            "Regenerated meeting summary.",
            persisted.Summary);

        Assert.Equal(
            "gemini-test-v2",
            persisted.Model);

        Assert.Equal(
            "v2",
            persisted.PromptVersion);

        Assert.True(persisted.IsRegenerated);
    }

    private async Task<Meeting> CreateAndPersistMeetingAsync()
    {
        var userResult = User.Create(
            $"summary-{Guid.NewGuid():N}@meetmind.test",
            "test-password-hash",
            "Test",
            "User");

        Assert.True(userResult.IsSuccess);

        var user = userResult.Value;

        var meetingResult = Meeting.Create(
            "Summary Test Meeting",
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

    private static MeetingSummary CreateSummary(
        Guid meetingId)
    {
        var result = MeetingSummary.Create(
            meetingId,
            "AI generated meeting summary.",
            "Gemini",
            "gemini-test",
            "v1");

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
