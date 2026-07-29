using Xunit;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Meetings.CreateMeeting;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Moq;

namespace MeetMindAI.Application.Tests.Meetings.CreateMeeting;

public sealed class CreateMeetingCommandHandlerTests
{
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly CreateMeetingCommandHandler _handler;

    public CreateMeetingCommandHandlerTests()
    {
        _meetingRepositoryMock =
            new Mock<IMeetingRepository>();

        _dbContextMock =
            new Mock<IApplicationDbContext>();

        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        _handler = new CreateMeetingCommandHandler(
            _meetingRepositoryMock.Object,
            _dbContextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateMeeting()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        _meetingRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Meeting>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateMeetingCommand(
            "Planning Meeting",
            "Discuss the next release.",
            DateTime.UtcNow.AddDays(1),
            60);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(
            Guid.Empty,
            result.Value.MeetingId);

        _meetingRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Meeting>(meeting =>
                    meeting.OrganizerId == userId &&
                    meeting.Title == "Planning Meeting"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutAuthenticatedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var command = new CreateMeetingCommand(
            "Planning Meeting",
            null,
            DateTime.UtcNow.AddDays(1),
            30);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            Error.Unauthorized,
            result.Error);

        _meetingRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Meeting>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidMeeting_ShouldReturnDomainError()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        var command = new CreateMeetingCommand(
            "   ",
            null,
            null,
            30);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            MeetingErrors.TitleRequired,
            result.Error);

        _meetingRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Meeting>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
