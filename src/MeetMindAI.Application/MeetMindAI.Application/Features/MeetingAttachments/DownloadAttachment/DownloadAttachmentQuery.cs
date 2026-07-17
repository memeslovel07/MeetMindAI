using MediatR;

using MeetMindAI.Application.Common.Models;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingAttachments.DownloadAttachment;

public sealed record DownloadAttachmentQuery(
    Guid MeetingId,
    Guid AttachmentId)
    : IRequest<Result<FileDownloadResult>>;
