using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Errors;

/// <summary>
/// Provides domain errors related to meetings.
/// </summary>
public static class MeetingErrors
{
    public static readonly Error TitleRequired =
        new(
            "Meeting.TitleRequired",
            "Meeting title is required.");

    public static readonly Error TitleTooLong =
        new(
            "Meeting.TitleTooLong",
            "Meeting title exceeds the maximum allowed length.");

    public static readonly Error DescriptionTooLong =
        new(
            "Meeting.DescriptionTooLong",
            "Meeting description exceeds the maximum allowed length.");

    public static readonly Error OrganizerRequired =
        new(
            "Meeting.OrganizerRequired",
            "Meeting organizer is required.");

    public static readonly Error InvalidDuration =
        new(
            "Meeting.InvalidDuration",
            "Meeting duration must be greater than zero.");

    public static readonly Error InvalidScheduleDate =
        new(
            "Meeting.InvalidScheduleDate",
            "Meeting must be scheduled in the future.");

    public static readonly Error CannotModifyCompletedMeeting =
        new(
            "Meeting.CannotModifyCompleted",
            "A completed meeting cannot be modified.");

    public static readonly Error CannotModifyCancelledMeeting =
        new(
            "Meeting.CannotModifyCancelled",
            "A cancelled meeting cannot be modified.");

    public static readonly Error CannotStartCancelledMeeting =
        new(
            "Meeting.CannotStartCancelled",
            "A cancelled meeting cannot be started.");

    public static readonly Error InvalidStatusTransition =
        new(
            "Meeting.InvalidStatusTransition",
            "The requested meeting status transition is not allowed.");
}
