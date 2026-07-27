using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Errors;

namespace MeetMindAI.Domain.Tests.Meetings;

public sealed class MeetingTests
{
    [Fact]
    public void Create_WithValidScheduledMeeting_ShouldCreateMeeting()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var scheduledAtUtc = DateTime.UtcNow.AddDays(1);

        // Act
        var result = Meeting.Create(
            "Team Planning",
            "Discuss the next release.",
            organizerId,
            scheduledAtUtc,
            60);

        // Assert
        Assert.True(result.IsSuccess);

        var meeting = result.Value;

        Assert.Equal("Team Planning", meeting.Title);
        Assert.Equal(
            "Discuss the next release.",
            meeting.Description);

        Assert.Equal(organizerId, meeting.OrganizerId);
        Assert.Equal(scheduledAtUtc, meeting.ScheduledAtUtc);
        Assert.Equal(60, meeting.DurationMinutes);
        Assert.Equal(MeetingStatus.Scheduled, meeting.Status);
    }

    [Fact]
    public void Create_WithoutSchedule_ShouldCreateDraftMeeting()
    {
        // Act
        var result = Meeting.Create(
            "Unscheduled Meeting",
            null,
            Guid.NewGuid(),
            null,
            30);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            MeetingStatus.Draft,
            result.Value.Status);

        Assert.Null(result.Value.ScheduledAtUtc);
    }

    [Fact]
    public void Create_ShouldTrimTitleAndDescription()
    {
        // Act
        var result = Meeting.Create(
            "  Architecture Review  ",
            "  Review Clean Architecture decisions.  ",
            Guid.NewGuid(),
            null,
            30);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            "Architecture Review",
            result.Value.Title);

        Assert.Equal(
            "Review Clean Architecture decisions.",
            result.Value.Description);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldFail()
    {
        // Act
        var result = Meeting.Create(
            "   ",
            null,
            Guid.NewGuid(),
            null,
            30);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            MeetingErrors.TitleRequired,
            result.Error);
    }

    [Fact]
    public void Create_WithEmptyOrganizerId_ShouldFail()
    {
        // Act
        var result = Meeting.Create(
            "Planning Meeting",
            null,
            Guid.Empty,
            null,
            30);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            MeetingErrors.OrganizerRequired,
            result.Error);
    }

    [Fact]
    public void Create_WithPastSchedule_ShouldFail()
    {
        // Act
        var result = Meeting.Create(
            "Planning Meeting",
            null,
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(-10),
            30);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            MeetingErrors.InvalidScheduleDate,
            result.Error);
    }

    [Fact]
    public void Schedule_DraftMeeting_ShouldChangeStatusToScheduled()
    {
        // Arrange
        var meeting = CreateDraftMeeting();
        var scheduledAtUtc = DateTime.UtcNow.AddDays(1);

        // Act
        var result = meeting.Schedule(scheduledAtUtc);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            MeetingStatus.Scheduled,
            meeting.Status);

        Assert.Equal(
            scheduledAtUtc,
            meeting.ScheduledAtUtc);
    }

    [Fact]
    public void Start_ScheduledMeeting_ShouldChangeStatusToInProgress()
    {
        // Arrange
        var meeting = CreateScheduledMeeting();

        // Act
        var result = meeting.Start();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            MeetingStatus.InProgress,
            meeting.Status);
    }

    [Fact]
    public void Complete_InProgressMeeting_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var meeting = CreateScheduledMeeting();

        var startResult = meeting.Start();

        Assert.True(startResult.IsSuccess);

        // Act
        var result = meeting.Complete();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            MeetingStatus.Completed,
            meeting.Status);
    }

    [Fact]
    public void Start_DraftMeeting_ShouldFail()
    {
        // Arrange
        var meeting = CreateDraftMeeting();

        // Act
        var result = meeting.Start();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            MeetingErrors.InvalidStatusTransition,
            result.Error);

        Assert.Equal(
            MeetingStatus.Draft,
            meeting.Status);
    }

    [Fact]
    public void Delete_InProgressMeeting_ShouldFail()
    {
        // Arrange
        var meeting = CreateScheduledMeeting();

        var startResult = meeting.Start();

        Assert.True(startResult.IsSuccess);

        // Act
        var result = meeting.Delete(
            Guid.NewGuid(),
            DateTime.UtcNow);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            MeetingErrors.CannotDeleteInProgressMeeting,
            result.Error);

        Assert.False(meeting.IsDeleted);
    }

    [Fact]
    public void Delete_ValidMeeting_ShouldSoftDeleteMeeting()
    {
        // Arrange
        var meeting = CreateDraftMeeting();
        var deletedBy = Guid.NewGuid();
        var deletedAtUtc = DateTime.UtcNow;

        // Act
        var result = meeting.Delete(
            deletedBy,
            deletedAtUtc);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(meeting.IsDeleted);
        Assert.Equal(deletedBy, meeting.DeletedBy);
        Assert.Equal(deletedAtUtc, meeting.DeletedAtUtc);
    }

    private static Meeting CreateDraftMeeting()
    {
        var result = Meeting.Create(
            "Draft Meeting",
            null,
            Guid.NewGuid(),
            null,
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static Meeting CreateScheduledMeeting()
    {
        var result = Meeting.Create(
            "Scheduled Meeting",
            null,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
