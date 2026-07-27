using MeetMindAI.Domain.Enums.Meetings;

namespace MeetMindAI.API.Contracts.ActionItems;

public sealed record CreateActionItemRequest(
    string Title,
    string? Description,
    ActionItemPriority Priority,
    DateTime? DueDate);
