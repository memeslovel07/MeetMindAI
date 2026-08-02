using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Abstractions.Storage;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Common.Models;
using MeetMindAI.Application.Features.MeetingAttachments.Common;
using MeetMindAI.Application.Features.MeetingAttachments.DeleteAttachment;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Microsoft.AspNetCore.Http;

using Moq;

using Xunit;

namespace MeetMindAI.Application.Tests.Features.MeetingAttachments;

public sealed class DeleteAttachmentCommandHandlerTests
{
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IMeetingAttachmentRepository> _attachmentRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly DeleteAttachmentCommandHandler _handler;

   

    public DeleteAttachmentCommandHandlerTests()
    {
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _attachmentRepositoryMock = new Mock<IMeetingAttachmentRepository>();
        _dbContextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();

        _handler = new DeleteAttachmentCommandHandler(
    _meetingRepositoryMock.Object,
    _attachmentRepositoryMock.Object,
    _fileStorageServiceMock.Object,
    _dbContextMock.Object,
    _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenMeetingDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meeting?)null);

        var command = new DeleteAttachmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MeetingErrors.NotFound, result.Error);

        _attachmentRepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _fileStorageServiceMock.Verify(
            x => x.DeleteFileAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMeetingBelongsToAnotherUser_ReturnsForbidden()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        var meeting = Meeting.Create(
            "Sprint Planning",
            null,
            ownerId,
            DateTime.UtcNow.AddDays(1),
            60).Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var command = new DeleteAttachmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Error.Forbidden, result.Error);

        _attachmentRepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _fileStorageServiceMock.Verify(
            x => x.DeleteFileAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAttachmentDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        var meeting = Meeting.Create(
            "Sprint Planning",
            null,
            userId,
            DateTime.UtcNow.AddDays(1),
            60).Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _attachmentRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeetingAttachment?)null);

        var command = new DeleteAttachmentCommand(
            meetingId,
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MeetingAttachmentErrors.NotFound, result.Error);

        _fileStorageServiceMock.Verify(
            x => x.DeleteFileAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAttachmentBelongsToDifferentMeeting_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        var meeting = Meeting.Create(
            "Sprint Planning",
            null,
            userId,
            DateTime.UtcNow.AddDays(1),
            60).Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var attachment = MeetingAttachment.Create(
            Guid.NewGuid(), // Different meeting
            "notes.pdf",
            "stored.pdf",
            "application/pdf",
            ".pdf",
            1024,
            AttachmentType.Document,
            "attachments/stored.pdf").Value;

        _attachmentRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);

        var command = new DeleteAttachmentCommand(
            meetingId,
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MeetingAttachmentErrors.NotFound, result.Error);

        _fileStorageServiceMock.Verify(
            x => x.DeleteFileAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_DeletesAttachmentSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        var meeting = Meeting.Create(
            "Sprint Planning",
            null,
            userId,
            DateTime.UtcNow.AddDays(1),
            60).Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var attachment = MeetingAttachment.Create(
            meetingId,
            "notes.pdf",
            "stored.pdf",
            "application/pdf",
            ".pdf",
            1024,
            AttachmentType.Document,
            "attachments/stored.pdf").Value;

        _attachmentRepositoryMock
            .Setup(x => x.GetByIdAsync(
                attachmentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(attachment);

        var command = new DeleteAttachmentCommand(
            meetingId,
            attachmentId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(attachment.Id, result.Value.AttachmentId);
        _fileStorageServiceMock.Verify(
            x => x.DeleteFileAsync(
                attachment.StorageKey,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _attachmentRepositoryMock.Verify(
            x => x.Remove(attachment),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
