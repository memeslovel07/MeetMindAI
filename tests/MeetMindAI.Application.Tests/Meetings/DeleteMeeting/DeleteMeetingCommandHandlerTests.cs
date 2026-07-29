using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Meetings.DeleteMeeting;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Moq;


namespace MeetMindAI.Application.Tests.Meetings.DeleteMeeting;

public sealed class DeleteMeetingCommandHandlerTests
{
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

    private readonly DeleteMeetingCommandHandler _handler;

    public DeleteMeetingCommandHandlerTests()
    {
        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _dateTimeProviderMock =
            new Mock<IDateTimeProvider>();

        _handler = new DeleteMeetingCommandHandler(
            _meetingRepositoryMock.Object,
            _dbContextMock.Object,
            _currentUserServiceMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidOwner_ShouldSoftDeleteMeeting()
    {
        // Arrange
        var organizerId = Guid.NewGuid();
        var deletedAtUtc =
            new DateTime(2026, 7, 29, 10, 0, 0, DateTimeKind.Utc);

        var meeting = CreateDraftMeeting(
            organizerId);

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        _dateTimeProviderMock
            .Setup(x => x.UtcNow)
            .Returns(deletedAtUtc);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command =
            new DeleteMeetingCommand(meeting.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(
            meeting.Id,
            result.Value.MeetingId);

        Assert.True(meeting.IsDeleted);

        Assert.Equal(
            organizerId,
            meeting.DeletedBy);

        Assert.Equal(
            deletedAtUtc,
            meeting.DeletedAtUtc);

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

        var command =
            new DeleteMeetingCommand(meetingId);

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

        var meeting = CreateDraftMeeting(
            organizerId);

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(differentUserId);

        var command =
            new DeleteMeetingCommand(meeting.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            Error.Forbidden,
            result.Error);

        Assert.False(meeting.IsDeleted);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMeetingIsInProgress_ShouldReturnDomainError()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var meeting = CreateScheduledMeeting(
            organizerId);

        var startResult = meeting.Start();

        Assert.True(startResult.IsSuccess);
        Assert.Equal(
            MeetingStatus.InProgress,
            meeting.Status);

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        _dateTimeProviderMock
            .Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        var command =
            new DeleteMeetingCommand(meeting.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            MeetingErrors.CannotDeleteInProgressMeeting,
            result.Error);

        Assert.False(meeting.IsDeleted);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Meeting CreateDraftMeeting(
        Guid organizerId)
    {
        var result = Meeting.Create(
            "Draft Meeting",
            null,
            organizerId,
            null,
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }

    private static Meeting CreateScheduledMeeting(
        Guid organizerId)
    {
        var result = Meeting.Create(
            "Scheduled Meeting",
            null,
            organizerId,
            DateTime.UtcNow.AddDays(1),
            30);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
