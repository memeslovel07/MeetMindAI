using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.MeetingAttachments.GetMeetingAttachments;

public sealed class GetMeetingAttachmentsQueryHandler
    : IRequestHandler<
        GetMeetingAttachmentsQuery,
        Result<IReadOnlyList<GetMeetingAttachmentResponse>>>
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly IMeetingAttachmentRepository _attachmentRepository;

    public GetMeetingAttachmentsQueryHandler(
        IMeetingRepository meetingRepository,
        IMeetingAttachmentRepository attachmentRepository)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(attachmentRepository);

        _meetingRepository = meetingRepository;
        _attachmentRepository = attachmentRepository;
    }

    public async Task<Result<IReadOnlyList<GetMeetingAttachmentResponse>>> Handle(
        GetMeetingAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<IReadOnlyList<GetMeetingAttachmentResponse>>
                .Failure(MeetingErrors.NotFound);
        }

        var attachments = await _attachmentRepository.GetByMeetingIdAsync(
            request.MeetingId,
            cancellationToken);

        var response = attachments
            .Select(a => new GetMeetingAttachmentResponse(
                a.Id,
                a.OriginalFileName,
                a.ContentType,
                a.SizeInBytes,
                a.AttachmentType.ToString(),
                a.CreatedAtUtc))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<GetMeetingAttachmentResponse>>
            .Success(response);
    }
}
