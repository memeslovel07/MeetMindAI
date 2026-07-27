using MediatR;

using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.UpdateActionItem;

/// <summary>
/// Represents a request to update an action item.
/// </summary>
public sealed record UpdateActionItemCommand(
    Guid Id,
    string Title,
    string? Description,
    ActionItemPriority Priority,
    DateTime? DueDate)
    : IRequest<Result>;
