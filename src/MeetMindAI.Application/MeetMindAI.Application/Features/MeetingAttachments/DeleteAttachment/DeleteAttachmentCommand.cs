using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingAttachments.DeleteAttachment;

public sealed record DeleteAttachmentCommand(
    Guid MeetingId,
    Guid AttachmentId)
    : IRequest<Result<DeleteAttachmentResponse>>;
