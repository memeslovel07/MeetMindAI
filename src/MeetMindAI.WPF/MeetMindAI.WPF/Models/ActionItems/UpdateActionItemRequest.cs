namespace MeetMindAI.WPF.Models.ActionItems;

public sealed record UpdateActionItemRequest(
    string Title,
    string? Description,
    ActionItemPriority Priority,
    DateTime? DueDate);
