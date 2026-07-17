using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Storage;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingAttachments.DeleteAttachment;

public sealed class DeleteAttachmentCommandHandler
    : IRequestHandler<
        DeleteAttachmentCommand,
        Result<DeleteAttachmentResponse>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingAttachmentRepository _attachmentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationDbContext _dbContext;

    public DeleteAttachmentCommandHandler(
        IMeetingRepository meetingRepository,
        IMeetingAttachmentRepository attachmentRepository,
        IFileStorageService fileStorageService,
        IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(attachmentRepository);
        ArgumentNullException.ThrowIfNull(fileStorageService);
        ArgumentNullException.ThrowIfNull(dbContext);

        _meetingRepository = meetingRepository;
        _attachmentRepository = attachmentRepository;
        _fileStorageService = fileStorageService;
        _dbContext = dbContext;
    }

    public async Task<Result<DeleteAttachmentResponse>> Handle(
        DeleteAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<DeleteAttachmentResponse>.Failure(
                MeetingErrors.NotFound);
        }

        var attachment = await _attachmentRepository.GetByIdAsync(
            request.AttachmentId,
            cancellationToken);

        if (attachment is null ||
            attachment.MeetingId != request.MeetingId)
        {
            return Result<DeleteAttachmentResponse>.Failure(
                MeetingAttachmentErrors.NotFound);
        }

        await _fileStorageService.DeleteFileAsync(
            attachment.StorageKey,
            cancellationToken);

        _attachmentRepository.Remove(attachment);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result<DeleteAttachmentResponse>.Success(
            new DeleteAttachmentResponse(
                attachment.Id));
    }
}
