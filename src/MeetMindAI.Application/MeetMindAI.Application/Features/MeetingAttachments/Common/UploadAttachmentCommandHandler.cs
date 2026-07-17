using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
using MeetMindAI.Application.Common.Abstractions.Storage;
using MeetMindAI.Application.Common.Helpers;
using MeetMindAI.Application.Common.Interfaces.Persistence;

using MeetMindAI.Domain.Entities.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingAttachments.Common;

public sealed class UploadAttachmentCommandHandler
    : IRequestHandler<
        UploadAttachmentCommand,
        Result<MeetingAttachmentResponse>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingAttachmentRepository _attachmentRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UploadAttachmentCommandHandler(
        IMeetingRepository meetingRepository,
        IMeetingAttachmentRepository attachmentRepository,
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(attachmentRepository);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(currentUserService);
        ArgumentNullException.ThrowIfNull(fileStorageService);

        _meetingRepository = meetingRepository;
        _attachmentRepository = attachmentRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<MeetingAttachmentResponse>> Handle(
        UploadAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid currentUserId)
        {
            return Result<MeetingAttachmentResponse>.Failure(
                Error.Unauthorized);
        }

        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<MeetingAttachmentResponse>.Failure(
                MeetingErrors.NotFound);
        }



        var storedFile = await _fileStorageService.SaveFileAsync(
            request.File,
            cancellationToken);

        var attachmentType = AttachmentTypeHelper.GetAttachmentType(
            storedFile.ContentType);

        var attachmentResult = MeetingAttachment.Create(
            request.MeetingId,
            storedFile.OriginalFileName,
            storedFile.StoredFileName,
            storedFile.ContentType,
            storedFile.Extension,
            storedFile.SizeInBytes,
            attachmentType,
            storedFile.StorageKey);

        if (attachmentResult.IsFailure)
        {
            await _fileStorageService.DeleteFileAsync(
                storedFile.StorageKey,
                cancellationToken);

            return Result<MeetingAttachmentResponse>.Failure(
                attachmentResult.Error);
        }

        var attachment = attachmentResult.Value;

        // If your auditing interceptor does NOT populate these automatically,
        // keep these assignments. Otherwise, remove them.
        attachment.CreatedBy = currentUserId;
        attachment.CreatedAtUtc = DateTime.UtcNow;

        await _attachmentRepository.AddAsync(
            attachment,
            cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(
                storedFile.StorageKey,
                cancellationToken);

            throw;
        }

        return Result<MeetingAttachmentResponse>.Success(
            new MeetingAttachmentResponse(
                attachment.Id,
                attachment.MeetingId,
                attachment.OriginalFileName,
                attachment.ContentType,
                attachment.SizeInBytes,
                attachment.AttachmentType.ToString(),
                attachment.CreatedAtUtc));
    }
}
