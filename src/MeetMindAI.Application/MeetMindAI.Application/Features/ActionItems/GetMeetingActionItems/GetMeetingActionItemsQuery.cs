using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.GetMeetingActionItems;

public sealed record GetMeetingActionItemsQuery(
    Guid MeetingId)
    : IRequest<Result<IReadOnlyList<ActionItemResponse>>>;
