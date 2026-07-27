using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Errors;

public static class ActionItemErrors
{
    public static readonly Error TitleRequired =
        Error.Validation(
            "ActionItem.TitleRequired",
            "Action item title is required.");

    public static readonly Error TitleTooLong =
        Error.Validation(
            "ActionItem.TitleTooLong",
            "Action item title cannot exceed 200 characters.");

    public static readonly Error DescriptionTooLong =
        Error.Validation(
            "ActionItem.DescriptionTooLong",
            "Action item description cannot exceed 2000 characters.");

    public static readonly Error AlreadyCompleted =
        new(
            "ActionItem.AlreadyCompleted",
            "The action item has already been completed.");

    public static readonly Error InvalidDueDate =
        Error.Validation(
            "ActionItem.InvalidDueDate",
            "The due date cannot be earlier than today.");

    public static readonly Error NotFound =
        new(
            "ActionItem.NotFound",
            "The requested action item was not found.");

    public static readonly Error AlreadyDeleted =
    new(
        "ActionItem.AlreadyDeleted",
        "The action item has already been deleted.");

}
