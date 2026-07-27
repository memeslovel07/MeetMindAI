using MeetMindAI.Domain.Enums.Meetings;

namespace MeetMindAI.Application.Common.Interfaces.AI;

public sealed record ExtractedActionItem(
    string Title,
    string? Description,
    ActionItemPriority Priority,
    DateTime? DueDate);
