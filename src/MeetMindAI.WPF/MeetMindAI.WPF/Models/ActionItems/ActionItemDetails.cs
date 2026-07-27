namespace MeetMindAI.WPF.Models.ActionItems;

public sealed record ActionItemDetails(
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
    DateTime? UpdatedAtUtc)
{
    public DateTime CreatedAtLocal =>
        CreatedAtUtc.ToLocalTime();

    public DateTime? CompletedAtLocal =>
        CompletedAt?.ToLocalTime();

    public DateTime? DueDateLocal =>
        DueDate?.ToLocalTime();
}
