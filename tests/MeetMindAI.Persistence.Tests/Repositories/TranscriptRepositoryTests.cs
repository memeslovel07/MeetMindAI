using Xunit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Persistence.Persistence;
using MeetMindAI.Persistence.Persistence.Repositories;

namespace MeetMindAI.Persistence.Tests.Repositories;

public sealed class TranscriptRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly TranscriptRepository _repository;

    public TranscriptRepositoryTests()
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
            new TranscriptRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistTranscript()
    {
        // Arrange
        var meeting = await CreateAndPersistMeetingAsync();

        var transcript = CreateTranscript(
            meeting.Id,
            "Persistence test transcript.");

        // Act
        await _repository.AddAsync(transcript);
        await _context.SaveChangesAsync();

        // Assert
        var persistedTranscript =
            await _context.Transcripts
                .SingleOrDefaultAsync(
                    x => x.Id == transcript.Id);

        Assert.NotNull(persistedTranscript);

        Assert.Equal(
            transcript.Id,
            persistedTranscript.Id);

        Assert.Equal(
            meeting.Id,
            persistedTranscript.MeetingId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTranscriptExists_ShouldReturnTranscript()
    {
        // Arrange
        var meeting = await CreateAndPersistMeetingAsync();

        var transcript = CreateTranscript(
            meeting.Id,
            "Transcript by ID.");

        await _context.Transcripts.AddAsync(transcript);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByIdAsync(
                transcript.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            transcript.Id,
            result.Id);

        Assert.Equal(
            "Transcript by ID.",
            result.Content);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTranscriptDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result =
            await _repository.GetByIdAsync(
                Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_WhenTranscriptExists_ShouldReturnTranscript()
    {
        // Arrange
        var meeting = await CreateAndPersistMeetingAsync();

        var transcript = CreateTranscript(
            meeting.Id,
            "Meeting transcript.");

        await _context.Transcripts.AddAsync(transcript);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            transcript.Id,
            result.Id);

        Assert.Equal(
            meeting.Id,
            result.MeetingId);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_WhenTranscriptDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var meeting = await CreateAndPersistMeetingAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Update_ShouldPersistTranscriptChanges()
    {
        // Arrange
        var meeting = await CreateAndPersistMeetingAsync();

        var transcript = CreateTranscript(
            meeting.Id,
            "Original transcript.");

        await _context.Transcripts.AddAsync(transcript);
        await _context.SaveChangesAsync();

        var updateResult =
            transcript.UpdateContent(
                "Updated transcript.",
                "en",
                TimeSpan.FromMinutes(45));

        Assert.True(updateResult.IsSuccess);

        // Act
        _repository.Update(transcript);

        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Assert
        var persistedTranscript =
            await _context.Transcripts
                .SingleAsync(
                    x => x.Id == transcript.Id);

        Assert.Equal(
            "Updated transcript.",
            persistedTranscript.Content);

        Assert.Equal(
            "en",
            persistedTranscript.Language);

        Assert.Equal(
            TimeSpan.FromMinutes(45),
            persistedTranscript.Duration);
    }

    [Fact]
    public async Task Remove_ShouldDeleteTranscript()
    {
        // Arrange
        var meeting = await CreateAndPersistMeetingAsync();

        var transcript = CreateTranscript(
            meeting.Id,
            "Transcript to delete.");

        await _context.Transcripts.AddAsync(transcript);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(transcript);

        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Assert
        var persistedTranscript =
            await _context.Transcripts
                .SingleOrDefaultAsync(
                    x => x.Id == transcript.Id);

        Assert.Null(persistedTranscript);
    }

    private async Task<Meeting> CreateAndPersistMeetingAsync()
    {
        var userResult = User.Create(
            $"transcript-{Guid.NewGuid():N}@meetmind.test",
            "test-password-hash",
            "Test",
            "User");

        Assert.True(userResult.IsSuccess);

        var user = userResult.Value;

        var meetingResult = Meeting.Create(
            "Transcript Test Meeting",
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

    private static Transcript CreateTranscript(
        Guid meetingId,
        string content)
    {
        var result = Transcript.Create(
            meetingId,
            content,
            "en",
            TimeSpan.FromMinutes(30));

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
