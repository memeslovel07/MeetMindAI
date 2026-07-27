using MediatR;

using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Features.ActionItems.CreateActionItem;

/// <summary>
/// Represents a request to create an action item for a meeting.
/// </summary>
public sealed record CreateActionItemCommand(
    Guid MeetingId,
    string Title,
    string? Description,
    ActionItemPriority Priority,
    DateTime? DueDate)
    : IRequest<Result<CreateActionItemResponse>>;
