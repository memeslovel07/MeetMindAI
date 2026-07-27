namespace MeetMindAI.WPF.Models.ActionItems;

public sealed record CreateActionItemRequest(
    string Title,
    string? Description,
    ActionItemPriority Priority,
    DateTime? DueDate);
