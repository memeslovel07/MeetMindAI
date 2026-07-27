using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.DeleteActionItem;

public sealed record DeleteActionItemCommand(
    Guid Id,
    Guid? DeletedBy)
    : IRequest<Result>;
