using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Storage;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Application.Common.Models;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingAttachments.DownloadAttachment;

public sealed class DownloadAttachmentQueryHandler
    : IRequestHandler<
        DownloadAttachmentQuery,
        Result<FileDownloadResult>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingAttachmentRepository _attachmentRepository;
    private readonly IFileStorageService _fileStorageService;

    public DownloadAttachmentQueryHandler(
        IMeetingRepository meetingRepository,
        IMeetingAttachmentRepository attachmentRepository,
        IFileStorageService fileStorageService)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(attachmentRepository);
        ArgumentNullException.ThrowIfNull(fileStorageService);

        _meetingRepository = meetingRepository;
        _attachmentRepository = attachmentRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<FileDownloadResult>> Handle(
        DownloadAttachmentQuery request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<FileDownloadResult>.Failure(
                MeetingErrors.NotFound);
        }

        var attachment = await _attachmentRepository.GetByIdAsync(
            request.AttachmentId,
            cancellationToken);

        if (attachment is null)
        {
            return Result<FileDownloadResult>.Failure(
                MeetingAttachmentErrors.NotFound);
        }

        if (attachment.MeetingId != request.MeetingId)
        {
            return Result<FileDownloadResult>.Failure(
                MeetingAttachmentErrors.NotFound);
        }

        var file = await _fileStorageService.OpenReadAsync(
            attachment.StorageKey,
            cancellationToken);

        if (file is null)
        {
            return Result<FileDownloadResult>.Failure(
                MeetingAttachmentErrors.FileNotFound);
        }

        return Result<FileDownloadResult>.Success(
     new FileDownloadResult
     {
         Stream = file.Stream,
         FileName = attachment.OriginalFileName,
         ContentType = attachment.ContentType
     });
    }
}
