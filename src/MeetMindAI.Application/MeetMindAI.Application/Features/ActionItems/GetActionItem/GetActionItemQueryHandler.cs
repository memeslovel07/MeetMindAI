using MediatR;

using MeetMindAI.Application.Common.Interfaces.Persistence;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.GetActionItem;

public sealed class GetActionItemQueryHandler
    : IRequestHandler<
        GetActionItemQuery,
        Result<GetActionItemResponse>>
{
    private readonly IActionItemRepository _actionItemRepository;

    public GetActionItemQueryHandler(
        IActionItemRepository actionItemRepository)
    {
        ArgumentNullException.ThrowIfNull(
            actionItemRepository);

        _actionItemRepository = actionItemRepository;
    }

    public async Task<Result<GetActionItemResponse>> Handle(
        GetActionItemQuery request,
        CancellationToken cancellationToken)
    {
        var actionItem =
            await _actionItemRepository.GetByIdAsync(
                request.Id,
                cancellationToken);

        if (actionItem is null)
        {
            return Result<GetActionItemResponse>.Failure(
                ActionItemErrors.NotFound);
        }

        var response = new GetActionItemResponse(
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
            actionItem.UpdatedAtUtc);

        return Result<GetActionItemResponse>.Success(
            response);
    }
}
