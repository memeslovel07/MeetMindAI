using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Abstractions.Storage;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Common.Models;
using MeetMindAI.Application.Features.MeetingAttachments.DownloadAttachment;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Moq;

using Xunit;

namespace MeetMindAI.Application.Tests.Features.MeetingAttachments;

public sealed class DownloadAttachmentQueryHandlerTests
{

    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IMeetingAttachmentRepository> _attachmentRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    private readonly DownloadAttachmentQueryHandler _handler;

    public DownloadAttachmentQueryHandlerTests()
    {
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _attachmentRepositoryMock = new Mock<IMeetingAttachmentRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new DownloadAttachmentQueryHandler(
            _meetingRepositoryMock.Object,
            _attachmentRepositoryMock.Object,
            _fileStorageServiceMock.Object,
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

        var query = new DownloadAttachmentQuery(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            query,
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
            x => x.OpenReadAsync(
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

        var query = new DownloadAttachmentQuery(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            query,
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
            x => x.OpenReadAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAttachmentDoesNotExist_ReturnsAttachmentNotFound()
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

        var query = new DownloadAttachmentQuery(
            meetingId,
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MeetingAttachmentErrors.NotFound, result.Error);

        _fileStorageServiceMock.Verify(
            x => x.OpenReadAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAttachmentBelongsToDifferentMeeting_ReturnsAttachmentNotFound()
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
            Guid.NewGuid(),              // Different MeetingId
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

        var query = new DownloadAttachmentQuery(
            meetingId,
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MeetingAttachmentErrors.NotFound, result.Error);

        _fileStorageServiceMock.Verify(
            x => x.OpenReadAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStoredFileDoesNotExist_ReturnsFileNotFound()
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

        _fileStorageServiceMock
            .Setup(x => x.OpenReadAsync(
                attachment.StorageKey,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileDownloadResult?)null);

        var query = new DownloadAttachmentQuery(
            meetingId,
            attachmentId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MeetingAttachmentErrors.FileNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_ReturnsFileDownload()
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

        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var storageFile = new FileDownloadResult
        {
            Stream = stream,
            FileName = "stored.pdf",          // Storage filename (handler replaces this)
            ContentType = "application/pdf"
        };

        _fileStorageServiceMock
            .Setup(x => x.OpenReadAsync(
                attachment.StorageKey,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageFile);

        var query = new DownloadAttachmentQuery(
            meetingId,
            attachmentId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Same(stream, result.Value.Stream);

        // Handler intentionally returns the ORIGINAL filename
        Assert.Equal("notes.pdf", result.Value.FileName);

        Assert.Equal("application/pdf", result.Value.ContentType);

        _fileStorageServiceMock.Verify(
            x => x.OpenReadAsync(
                attachment.StorageKey,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
