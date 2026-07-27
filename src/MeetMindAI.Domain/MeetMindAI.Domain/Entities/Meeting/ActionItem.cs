using MeetMindAI.Domain.Common;
using MeetMindAI.Domain.Enums.Meetings;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;
using MeetMindAI.Domain.Entities.Users;
namespace MeetMindAI.Domain.Entities.Meetings;

public sealed class ActionItem : SoftDeletableEntity
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    private ActionItem()
    {
    }

    private ActionItem(
        Guid meetingId,
        string title,
        string? description,
        ActionItemPriority priority,
        DateTime? dueDate)
    {
        Id = Guid.NewGuid();

        MeetingId = meetingId;
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;

        Status = ActionItemStatus.Pending;
    }

    public Guid MeetingId { get; private set; }

    public Meeting Meeting { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ActionItemPriority Priority { get; private set; }

    public ActionItemStatus Status { get; private set; }

    public DateTime? DueDate { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public Guid? AssignedUserId { get; private set; }

    public MeetMindAI.Domain.Entities.Users.User? AssignedUser { get; private set; }

    public ActionItemSource Source { get; private set; }

    public static Result<ActionItem> Create(
    Guid meetingId,
    string title,
    string? description,
    ActionItemPriority priority,
    DateTime? dueDate,
    ActionItemSource source = ActionItemSource.Manual)
    {
        title = title.Trim();

        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        var validation = Validate(
            meetingId,
            title,
            description,
            dueDate);

        if (validation.IsFailure)
        {
            return Result<ActionItem>.Failure(
                validation.Error);
        }

        return Result<ActionItem>.Success(
            new ActionItem(
                meetingId,
                title,
                description,
                priority,
                dueDate,
                source));
    }

    public Result Update(
        string title,
        string? description,
        ActionItemPriority priority,
        DateTime? dueDate)
    {
        title = title.Trim();

        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        var validation = Validate(
            MeetingId,
            title,
            description,
            dueDate);

        if (validation.IsFailure)
        {
            return validation;
        }

        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;

        return Result.Success();
    }

    public Result MarkCompleted()
    {
        if (Status == ActionItemStatus.Completed)
        {
            return Result.Failure(
                ActionItemErrors.AlreadyCompleted);
        }

        Status = ActionItemStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void Reopen()
    {
        Status = ActionItemStatus.Pending;
        CompletedAt = null;
    }

    private static Result Validate(
        Guid meetingId,
        string title,
        string? description,
        DateTime? dueDate)
    {
        if (meetingId == Guid.Empty)
        {
            return Result.Failure(
                MeetingSummaryErrors.InvalidMeetingId);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(
                ActionItemErrors.TitleRequired);
        }

        if (title.Length > MaxTitleLength)
        {
            return Result.Failure(
                ActionItemErrors.TitleTooLong);
        }

        if (!string.IsNullOrWhiteSpace(description) &&
            description.Length > MaxDescriptionLength)
        {
            return Result.Failure(
                ActionItemErrors.DescriptionTooLong);
        }

        if (dueDate.HasValue &&
            dueDate.Value.Date < DateTime.UtcNow.Date)
        {
            return Result.Failure(
                ActionItemErrors.InvalidDueDate);
        }

        return Result.Success();
    }

    public void Delete(Guid? deletedBy)
    {
        MarkAsDeleted(
            deletedBy,
            DateTime.UtcNow);
    }

    public Result Delete(
    Guid? deletedBy,
    DateTime deletedAtUtc)
    {
        if (IsDeleted)
        {
            return Result.Failure(
                ActionItemErrors.AlreadyDeleted);
        }

        MarkAsDeleted(
            deletedBy,
            deletedAtUtc);

        return Result.Success();
    }

    private ActionItem(
    Guid meetingId,
    string title,
    string? description,
    ActionItemPriority priority,
    DateTime? dueDate,
    ActionItemSource source)
    {
        Id = Guid.NewGuid();

        MeetingId = meetingId;
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        Source = source;

        Status = ActionItemStatus.Pending;
    }

}
