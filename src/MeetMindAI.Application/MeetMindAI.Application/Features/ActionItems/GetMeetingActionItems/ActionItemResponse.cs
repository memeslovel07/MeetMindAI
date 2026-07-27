using MeetMindAI.Domain.Enums.Meetings;

namespace MeetMindAI.Application.Features.ActionItems.GetMeetingActionItems;

public sealed record ActionItemResponse(
    Guid Id,
    Guid MeetingId,
    string Title,
    string? Description,
    ActionItemPriority Priority,
    ActionItemStatus Status,
    Guid? AssignedUserId,
    DateTime? DueDate,
    DateTime? CompletedAt,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
