using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Meetings.UpdateMeeting;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Moq;

namespace MeetMindAI.Application.Tests.Meetings.UpdateMeeting;

public sealed class UpdateMeetingCommandHandlerTests
{
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly UpdateMeetingCommandHandler _handler;

    public UpdateMeetingCommandHandlerTests()
    {
        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _handler = new UpdateMeetingCommandHandler(
            _meetingRepositoryMock.Object,
            _dbContextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateMeeting()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var meeting = CreateMeeting(organizerId);

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var scheduledAtUtc =
            DateTime.UtcNow.AddDays(3);

        var command = new UpdateMeetingCommand(
            meeting.Id,
            "Updated Planning Meeting",
            "Updated description.",
            scheduledAtUtc,
            90);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            meeting.Id,
            result.Value.MeetingId);

        Assert.Equal(
            "Updated Planning Meeting",
            meeting.Title);

        Assert.Equal(
            "Updated description.",
            meeting.Description);

        Assert.Equal(
            scheduledAtUtc,
            meeting.ScheduledAtUtc);

        Assert.Equal(
            90,
            meeting.DurationMinutes);

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

        var command = new UpdateMeetingCommand(
            meetingId,
            "Updated Meeting",
            null,
            DateTime.UtcNow.AddDays(2),
            60);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            MeetingErrors.NotFound,
            result.Error);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotOrganizer_ShouldReturnForbidden()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var meeting = CreateMeeting(
            organizerId);

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(differentUserId);

        var command = new UpdateMeetingCommand(
            meeting.Id,
            "Unauthorized Update",
            null,
            DateTime.UtcNow.AddDays(2),
            60);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            Error.Forbidden,
            result.Error);

        Assert.Equal(
            "Original Meeting",
            meeting.Title);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidUpdate_ShouldReturnDomainError()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var meeting = CreateMeeting(organizerId);

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        var command = new UpdateMeetingCommand(
            meeting.Id,
            "   ",
            null,
            DateTime.UtcNow.AddDays(2),
            60);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            MeetingErrors.TitleRequired,
            result.Error);

        // Failed domain update must not partially
        // mutate the existing meeting.
        Assert.Equal(
            "Original Meeting",
            meeting.Title);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Meeting CreateMeeting(
        Guid organizerId)
    {
        var result = Meeting.Create(
            "Original Meeting",
            "Original description.",
            organizerId,
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
