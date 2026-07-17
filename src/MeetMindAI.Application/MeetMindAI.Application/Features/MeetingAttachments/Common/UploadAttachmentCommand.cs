using MediatR;
using Microsoft.AspNetCore.Http;


using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingAttachments.Common;

public sealed record UploadAttachmentCommand(
    Guid MeetingId,
    IFormFile File)
    : IRequest<Result<MeetingAttachmentResponse>>;
