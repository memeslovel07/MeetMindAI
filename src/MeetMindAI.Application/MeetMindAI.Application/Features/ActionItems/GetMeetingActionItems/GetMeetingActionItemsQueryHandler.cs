using MediatR;

using MeetMindAI.Application.Common.Abstractions.Persistence;
using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.GetMeetingActionItems;

public sealed class GetMeetingActionItemsQueryHandler
    : IRequestHandler<
        GetMeetingActionItemsQuery,
        Result<IReadOnlyList<ActionItemResponse>>>
{
    private readonly IActionItemRepository _actionItemRepository;
    private readonly IMeetingRepository _meetingRepository;

    public GetMeetingActionItemsQueryHandler(
        IActionItemRepository actionItemRepository,
        IMeetingRepository meetingRepository)
    {
        ArgumentNullException.ThrowIfNull(actionItemRepository);
        ArgumentNullException.ThrowIfNull(meetingRepository);

        _actionItemRepository = actionItemRepository;
        _meetingRepository = meetingRepository;
    }

    public async Task<Result<IReadOnlyList<ActionItemResponse>>> Handle(
        GetMeetingActionItemsQuery request,
        CancellationToken cancellationToken)
    {
        var meeting = await _meetingRepository.GetByIdAsync(
            request.MeetingId,
            cancellationToken);

        if (meeting is null)
        {
            return Result<IReadOnlyList<ActionItemResponse>>.Failure(
                MeetingErrors.NotFound);
        }

        var actionItems =
            await _actionItemRepository.GetByMeetingIdAsync(
                request.MeetingId,
                cancellationToken);

        IReadOnlyList<ActionItemResponse> response =
            actionItems
                .Select(actionItem =>
                    new ActionItemResponse(
                        actionItem.Id,
                        actionItem.MeetingId,
                        actionItem.Title,
                        actionItem.Description,
                        actionItem.Priority,
                        actionItem.Status,
                        actionItem.AssignedUserId,
                        actionItem.DueDate,
                        actionItem.CompletedAt,
                        actionItem.CreatedAtUtc,
                        actionItem.UpdatedAtUtc))
                .ToList();

        return Result<IReadOnlyList<ActionItemResponse>>.Success(
            response);
    }
}
