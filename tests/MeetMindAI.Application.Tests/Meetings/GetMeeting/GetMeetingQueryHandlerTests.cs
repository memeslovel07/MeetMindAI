using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Meetings.GetMeeting;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Errors;

using Moq;

namespace MeetMindAI.Application.Tests.Meetings.GetMeeting;

public sealed class GetMeetingQueryHandlerTests
{
    private readonly Mock<IMeetingRepository>
        _meetingRepositoryMock;

    private readonly GetMeetingQueryHandler _handler;

    public GetMeetingQueryHandlerTests()
    {
        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _handler = new GetMeetingQueryHandler(
            _meetingRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenMeetingExists_ShouldReturnMeetingResponse()
    {
        // Arrange
        var organizerId = Guid.NewGuid();

        var scheduledAtUtc =
            DateTime.UtcNow.AddDays(2);

        var meetingResult = Meeting.Create(
            "Project Planning",
            "Discuss project milestones.",
            organizerId,
            scheduledAtUtc,
            60);

        Assert.True(meetingResult.IsSuccess);

        var meeting = meetingResult.Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var query =
            new GetMeetingQuery(meeting.Id);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value;

        Assert.Equal(
            meeting.Id,
            response.Id);

        Assert.Equal(
            "Project Planning",
            response.Title);

        Assert.Equal(
            "Discuss project milestones.",
            response.Description);

        Assert.Equal(
            organizerId,
            response.OrganizerId);

        Assert.Equal(
            scheduledAtUtc,
            response.ScheduledAtUtc);

        Assert.Equal(
            60,
            response.DurationMinutes);

        Assert.Equal(
            MeetingStatus.Scheduled,
            response.Status);

        _meetingRepositoryMock.Verify(
            x => x.GetByIdAsync(
                meeting.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMeetingDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var meetingId =
            Guid.NewGuid();

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meeting?)null);

        var query =
            new GetMeetingQuery(meetingId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            MeetingErrors.NotFound,
            result.Error);

        _meetingRepositoryMock.Verify(
            x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
