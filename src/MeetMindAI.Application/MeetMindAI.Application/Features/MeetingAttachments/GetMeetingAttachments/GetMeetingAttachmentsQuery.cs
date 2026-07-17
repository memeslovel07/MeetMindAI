using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingAttachments.GetMeetingAttachments;

public sealed record GetMeetingAttachmentsQuery(
    Guid MeetingId)
    : IRequest<Result<IReadOnlyList<GetMeetingAttachmentResponse>>>;
