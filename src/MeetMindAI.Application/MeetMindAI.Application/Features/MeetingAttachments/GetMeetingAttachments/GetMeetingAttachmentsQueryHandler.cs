using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Abstractions.Services;
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
    private readonly ICurrentUserService _currentUserService;

    public GetMeetingAttachmentsQueryHandler(
     IMeetingRepository meetingRepository,
     IMeetingAttachmentRepository attachmentRepository,
     ICurrentUserService currentUserService)
    {
        ArgumentNullException.ThrowIfNull(meetingRepository);
        ArgumentNullException.ThrowIfNull(attachmentRepository);
        ArgumentNullException.ThrowIfNull(currentUserService);

        _meetingRepository = meetingRepository;
        _attachmentRepository = attachmentRepository;
        _currentUserService = currentUserService;
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

        if (_currentUserService.UserId != meeting.OrganizerId)
        {
            return Result<IReadOnlyList<GetMeetingAttachmentResponse>>
                .Failure(Error.Forbidden);
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
