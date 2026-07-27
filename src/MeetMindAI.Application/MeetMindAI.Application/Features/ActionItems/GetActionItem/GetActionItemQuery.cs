using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.GetActionItem;

public sealed record GetActionItemQuery(
    Guid Id)
    : IRequest<Result<GetActionItemResponse>>;
