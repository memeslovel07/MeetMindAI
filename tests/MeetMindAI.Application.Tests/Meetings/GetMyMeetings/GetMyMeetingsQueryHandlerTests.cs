using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Meetings.GetMyMeetings;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Shared.Results;

using Moq;

namespace MeetMindAI.Application.Tests.Meetings.GetMyMeetings;

public sealed class GetMyMeetingsQueryHandlerTests
{
    private readonly Mock<IMeetingRepository>
        _meetingRepositoryMock;

    private readonly Mock<ICurrentUserService>
        _currentUserServiceMock;

    private readonly GetMyMeetingsQueryHandler _handler;

    public GetMyMeetingsQueryHandlerTests()
    {
        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _handler = new GetMyMeetingsQueryHandler(
            _meetingRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnUsersMeetings()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var firstScheduledAt =
            DateTime.UtcNow.AddDays(1);

        var secondScheduledAt =
            DateTime.UtcNow.AddDays(2);

        var firstMeeting = CreateMeeting(
            "Project Planning",
            organizerId,
            firstScheduledAt,
            60);

        var secondMeeting = CreateMeeting(
            "Client Review",
            organizerId,
            secondScheduledAt,
            45);

        IReadOnlyList<Meeting> meetings =
            new List<Meeting>
            {
                firstMeeting,
                secondMeeting
            };

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        _meetingRepositoryMock
            .Setup(x => x.GetByOrganizerIdAsync(
                organizerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meetings);

        var query =
            new GetMyMeetingsQuery();

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        var firstResponse = result.Value[0];

        Assert.Equal(
            firstMeeting.Id,
            firstResponse.Id);

        Assert.Equal(
            "Project Planning",
            firstResponse.Title);

        Assert.Equal(
            firstScheduledAt,
            firstResponse.ScheduledAtUtc);

        Assert.Equal(
            60,
            firstResponse.DurationMinutes);

        Assert.Equal(
            MeetingStatus.Scheduled,
            firstResponse.Status);

        var secondResponse = result.Value[1];

        Assert.Equal(
            secondMeeting.Id,
            secondResponse.Id);

        Assert.Equal(
            "Client Review",
            secondResponse.Title);

        Assert.Equal(
            secondScheduledAt,
            secondResponse.ScheduledAtUtc);

        Assert.Equal(
            45,
            secondResponse.DurationMinutes);

        Assert.Equal(
            MeetingStatus.Scheduled,
            secondResponse.Status);

        _meetingRepositoryMock.Verify(
            x => x.GetByOrganizerIdAsync(
                organizerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var query =
            new GetMyMeetingsQuery();

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            Error.Unauthorized,
            result.Error);

        _meetingRepositoryMock.Verify(
            x => x.GetByOrganizerIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoMeetings_ShouldReturnEmptyCollection()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(organizerId);

        _meetingRepositoryMock
            .Setup(x => x.GetByOrganizerIdAsync(
                organizerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Array.Empty<Meeting>());

        var query =
            new GetMyMeetingsQuery();

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);

        _meetingRepositoryMock.Verify(
            x => x.GetByOrganizerIdAsync(
                organizerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Meeting CreateMeeting(
        string title,
        Guid organizerId,
        DateTime scheduledAtUtc,
        int durationMinutes)
    {
        var result = Meeting.Create(
            title,
            null,
            organizerId,
            scheduledAtUtc,
            durationMinutes);

        Assert.True(result.IsSuccess);

        return result.Value;
    }
}
