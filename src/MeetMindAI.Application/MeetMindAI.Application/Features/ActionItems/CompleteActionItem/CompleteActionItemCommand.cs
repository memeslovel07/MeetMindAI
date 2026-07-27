using MediatR;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.CompleteActionItem;

/// <summary>
/// Represents a request to mark an action item as completed.
/// </summary>
public sealed record CompleteActionItemCommand(
    Guid Id)
    : IRequest<Result>;
