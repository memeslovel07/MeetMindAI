using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Abstractions.Storage;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Common.Models;
using MeetMindAI.Application.Features.MeetingAttachments.Common;
using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

using Microsoft.AspNetCore.Http;

using Moq;

using Xunit;

namespace MeetMindAI.Application.Tests.Features.MeetingAttachments;

public sealed class UploadAttachmentCommandHandlerTests
{
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IMeetingAttachmentRepository> _attachmentRepositoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;

    private readonly UploadAttachmentCommandHandler _handler;

    public UploadAttachmentCommandHandlerTests()
    {
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _attachmentRepositoryMock = new Mock<IMeetingAttachmentRepository>();
        _dbContextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();

        _handler = new UploadAttachmentCommandHandler(
            _meetingRepositoryMock.Object,
            _attachmentRepositoryMock.Object,
            _dbContextMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var fileMock = new Mock<IFormFile>();

        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            fileMock.Object);

        // Act
        Result<MeetingAttachmentResponse> result =
            await _handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Error.Unauthorized, result.Error);

        _meetingRepositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _fileStorageServiceMock.Verify(
            x => x.SaveFileAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _attachmentRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Domain.Entities.Meetings.MeetingAttachment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMeetingBelongsToAnotherUser_ReturnsForbidden()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var meetingOwnerId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(currentUserId);

        var meetingResult = Meeting.Create(
            "Sprint Planning",
            null,
            meetingOwnerId,
            DateTime.UtcNow.AddDays(1),
            60);

        Assert.True(meetingResult.IsSuccess);

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meetingResult.Value);

        var fileMock = new Mock<IFormFile>();

        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            fileMock.Object);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Error.Forbidden, result.Error);

        _fileStorageServiceMock.Verify(
            x => x.SaveFileAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _attachmentRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Domain.Entities.Meetings.MeetingAttachment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMeetingDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(Guid.NewGuid());

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meeting?)null);

        var fileMock = new Mock<IFormFile>();

        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            fileMock.Object);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(MeetingErrors.NotFound, result.Error);

        _fileStorageServiceMock.Verify(
            x => x.SaveFileAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _attachmentRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<MeetingAttachment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_CreatesAttachmentSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();

        _currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns(userId);

        var meetingResult = Meeting.Create(
            "Sprint Planning",
            "Discuss sprint backlog",
            userId,
            DateTime.UtcNow.AddDays(1),
            60);

        Assert.True(meetingResult.IsSuccess);

        var meeting = meetingResult.Value;

        _meetingRepositoryMock
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var fileMock = new Mock<IFormFile>();

        var storageResult = new FileStorageResult
        {
            OriginalFileName = "notes.pdf",
            StoredFileName = "12345.pdf",
            StorageKey = "attachments/12345.pdf",
            ContentType = "application/pdf",
            Extension = ".pdf",
            SizeInBytes = 1024
        };

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(
                fileMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageResult);

        var command = new UploadAttachmentCommand(
            meetingId,
            fileMock.Object);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(meetingId, result.Value.MeetingId);
        Assert.Equal("notes.pdf", result.Value.OriginalFileName);
        Assert.Equal("application/pdf", result.Value.ContentType);
        Assert.Equal(1024, result.Value.SizeInBytes);

        _attachmentRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<MeetingAttachment>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _dbContextMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _fileStorageServiceMock.Verify(
            x => x.SaveFileAsync(
                fileMock.Object,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _fileStorageServiceMock.Verify(
            x => x.DeleteFileAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_DeletesStoredFileAndThrows()
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

        var fileMock = new Mock<IFormFile>();

        var storageResult = new FileStorageResult
        {
            OriginalFileName = "notes.pdf",
            StoredFileName = "12345.pdf",
            StorageKey = "attachments/12345.pdf",
            ContentType = "application/pdf",
            Extension = ".pdf",
            SizeInBytes = 2048
        };

        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(
                fileMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageResult);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database failure"));

        var command = new UploadAttachmentCommand(
            meetingId,
            fileMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));

        _fileStorageServiceMock.Verify(
            x => x.DeleteFileAsync(
                storageResult.StorageKey,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


}

