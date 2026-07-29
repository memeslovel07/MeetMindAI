using Xunit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Persistence.Persistence;
using MeetMindAI.Persistence.Repositories;

namespace MeetMindAI.Persistence.Tests.Repositories;

public sealed class MeetingAttachmentRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly MeetingAttachmentRepository _repository;

    public MeetingAttachmentRepositoryTests()
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
            new MeetingAttachmentRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAttachment()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var attachment =
            CreateAttachment(
                meeting.Id,
                "notes.pdf");

        // Act
        await _repository.AddAsync(attachment);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Assert
        var persisted =
            await _context.MeetingAttachments
                .SingleOrDefaultAsync(
                    x => x.Id == attachment.Id);

        Assert.NotNull(persisted);

        Assert.Equal(
            attachment.Id,
            persisted.Id);

        Assert.Equal(
            meeting.Id,
            persisted.MeetingId);

        Assert.Equal(
            "notes.pdf",
            persisted.OriginalFileName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAttachmentExists_ShouldReturnAttachment()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var attachment =
            CreateAttachment(
                meeting.Id,
                "meeting.pdf");

        await _context.MeetingAttachments
            .AddAsync(attachment);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByIdAsync(
                attachment.Id);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            attachment.Id,
            result.Id);

        Assert.Equal(
            "meeting.pdf",
            result.OriginalFileName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAttachmentDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result =
            await _repository.GetByIdAsync(
                Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_ShouldReturnOnlyMeetingsAttachments()
    {
        // Arrange
        var firstMeeting =
            await CreateAndPersistMeetingAsync();

        var secondMeeting =
            await CreateAndPersistMeetingAsync();

        var firstAttachment =
            CreateAttachment(
                firstMeeting.Id,
                "first.pdf");

        var secondAttachment =
            CreateAttachment(
                firstMeeting.Id,
                "second.pdf");

        var otherAttachment =
            CreateAttachment(
                secondMeeting.Id,
                "other.pdf");

        await _context.MeetingAttachments.AddRangeAsync(
            firstAttachment,
            secondAttachment,
            otherAttachment);

        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                firstMeeting.Id);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            attachment =>
                Assert.Equal(
                    firstMeeting.Id,
                    attachment.MeetingId));

        Assert.DoesNotContain(
            result,
            attachment =>
                attachment.Id == otherAttachment.Id);
    }

    [Fact]
    public async Task GetByMeetingIdAsync_WhenNoAttachmentsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        // Act
        var result =
            await _repository.GetByMeetingIdAsync(
                meeting.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Remove_ShouldDeleteAttachment()
    {
        // Arrange
        var meeting =
            await CreateAndPersistMeetingAsync();

        var attachment =
            CreateAttachment(
                meeting.Id,
                "delete.pdf");

        await _context.MeetingAttachments
            .AddAsync(attachment);

        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(attachment);

        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Assert
        var persisted =
            await _context.MeetingAttachments
                .SingleOrDefaultAsync(
                    x => x.Id == attachment.Id);

        Assert.Null(persisted);
    }

    private async Task<Meeting> CreateAndPersistMeetingAsync()
    {
        var userResult = User.Create(
            $"attachment-{Guid.NewGuid():N}@meetmind.test",
            "test-password-hash",
            "Test",
            "User");

        Assert.True(userResult.IsSuccess);

        var user = userResult.Value;

        var meetingResult = Meeting.Create(
            "Attachment Test Meeting",
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

    private static MeetingAttachment CreateAttachment(
        Guid meetingId,
        string fileName)
    {
        var result = MeetingAttachment.Create(
            meetingId,
            fileName,
            $"{Guid.NewGuid():N}.pdf",
            "application/pdf",
            ".pdf",
            1024,
            AttachmentType.Document,
            $"meetings/{meetingId}/{Guid.NewGuid():N}.pdf");

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
