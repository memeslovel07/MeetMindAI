<<<<<<< HEAD
using MeetMindAI.Domain.Common;
using MeetMindAI.Domain.Enums;
using MeetMindAI.Domain.Constants;
using MeetMindAI.Domain.Errors;
using MeetMindAI.Shared.Results;

namespace MeetMindAI.Domain.Entities.Meetings;

/// <summary>
/// Represents a meeting within the MeetMind AI platform.
/// </summary>
public sealed class Meeting : AggregateRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Meeting"/> class.
    /// Required by Entity Framework Core.
    /// </summary>
    private Meeting()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Meeting"/> class.
    /// </summary>
    private Meeting(
        string title,
        string? description,
        Guid organizerId,
        DateTime? scheduledAtUtc,
        int durationMinutes)
    {
        Title = title;
        Description = description;
        OrganizerId = organizerId;
        ScheduledAtUtc = scheduledAtUtc;
        DurationMinutes = durationMinutes;

        Status = scheduledAtUtc.HasValue
            ? MeetingStatus.Scheduled
            : MeetingStatus.Draft;
    }

    /// <summary>
    /// Gets the meeting title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the meeting description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the organizer identifier.
    /// </summary>
    public Guid OrganizerId { get; private set; }

    /// <summary>
    /// Gets the organizer.
    /// </summary>
    public Users.User Organizer { get; private set; } = null!;

    /// <summary>
    /// Gets the scheduled UTC date and time.
    /// </summary>
    public DateTime? ScheduledAtUtc { get; private set; }

    /// <summary>
    /// Gets the duration of the meeting in minutes.
    /// </summary>
    public int DurationMinutes { get; private set; }

    /// <summary>
    /// Gets the current meeting status.
    /// </summary>
    public MeetingStatus Status { get; private set; }

    public Transcript? Transcript { get; private set; }

    /// <summary>
    /// Creates a new meeting.
    /// </summary>
    public static Result<Meeting> Create(
        string title,
        string? description,
        Guid organizerId,
        DateTime? scheduledAtUtc,
        int durationMinutes)
    {
        title = title.Trim();

        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        var validationResult = Validate(
            title,
            description,
            organizerId,
            scheduledAtUtc,
            durationMinutes);

        if (validationResult.IsFailure)
        {
            return Result<Meeting>.Failure(
                validationResult.Error);
        }

        var meeting = new Meeting(
            title,
            description,
            organizerId,
            scheduledAtUtc,
            durationMinutes);

        return Result<Meeting>.Success(meeting);
    }

    private static Result Validate(
    string title,
    string? description,
    Guid organizerId,
    DateTime? scheduledAtUtc,
    int durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(
                MeetingErrors.TitleRequired);
        }

        if (title.Length >
            ValidationConstants.MeetingTitleMaxLength)
        {
            return Result.Failure(
                MeetingErrors.TitleTooLong);
        }

        if (!string.IsNullOrWhiteSpace(description) &&
            description.Length >
            ValidationConstants.MeetingDescriptionMaxLength)
        {
            return Result.Failure(
                MeetingErrors.DescriptionTooLong);
        }

        if (organizerId == Guid.Empty)
        {
            return Result.Failure(
                MeetingErrors.OrganizerRequired);
        }

        if (durationMinutes <
                ValidationConstants.MeetingMinDurationMinutes ||
            durationMinutes >
                ValidationConstants.MeetingMaxDurationMinutes)
        {
            return Result.Failure(
                MeetingErrors.InvalidDuration);
        }

        if (scheduledAtUtc.HasValue &&
            scheduledAtUtc.Value <= DateTime.UtcNow)
        {
            return Result.Failure(
                MeetingErrors.InvalidScheduleDate);
        }

        return Result.Success();
    }

    /// <summary>
    /// Updates the meeting details.
    /// </summary>
    public Result Update(
        string title,
        string? description,
        DateTime? scheduledAtUtc,
        int durationMinutes)
    {
        if (Status is MeetingStatus.Completed or MeetingStatus.Cancelled)
        {
            return Result.Failure(
                MeetingErrors.CannotModifyCompletedMeeting);
        }

        title = title.Trim();

        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        var validationResult = Validate(
            title,
            description,
            OrganizerId,
            scheduledAtUtc,
            durationMinutes);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        Title = title;
        Description = description;
        ScheduledAtUtc = scheduledAtUtc;
        DurationMinutes = durationMinutes;

        return Result.Success();
    }

    /// <summary>
    /// Schedules the meeting.
    /// </summary>
    public Result Schedule(DateTime scheduledAtUtc)
    {
        if (Status != MeetingStatus.Draft)
        {
            return Result.Failure(
                MeetingErrors.InvalidStatusTransition);
        }

        if (scheduledAtUtc <= DateTime.UtcNow)
        {
            return Result.Failure(
                MeetingErrors.InvalidScheduleDate);
        }

        ScheduledAtUtc = scheduledAtUtc;
        Status = MeetingStatus.Scheduled;

        return Result.Success();
    }

    /// <summary>
    /// Starts the meeting.
    /// </summary>
    public Result Start()
    {
        if (Status != MeetingStatus.Scheduled)
        {
            return Result.Failure(
                MeetingErrors.InvalidStatusTransition);
        }

        Status = MeetingStatus.InProgress;

        return Result.Success();
    }

    /// <summary>
    /// Completes the meeting.
    /// </summary>
    public Result Complete()
    {
        if (Status != MeetingStatus.InProgress)
        {
            return Result.Failure(
                MeetingErrors.InvalidStatusTransition);
        }

        Status = MeetingStatus.Completed;

        return Result.Success();
    }

    /// <summary>
    /// Cancels the meeting.
    /// </summary>
    public Result Cancel()
    {
        if (Status is MeetingStatus.InProgress or MeetingStatus.Completed)
        {
            return Result.Failure(
                MeetingErrors.InvalidStatusTransition);
        }

        Status = MeetingStatus.Cancelled;

        return Result.Success();
    }

    /// <summary>
    /// Soft deletes the meeting.
    /// </summary>
    public Result Delete(
        Guid? deletedBy,
        DateTime deletedAtUtc)
    {
        if (Status == MeetingStatus.InProgress)
        {
            return Result.Failure(
                MeetingErrors.CannotDeleteInProgressMeeting);
        }

        if (IsDeleted)
        {
            return Result.Success();
        }

        MarkAsDeleted(
            deletedBy,
            deletedAtUtc);

        return Result.Success();
    }

    private readonly List<MeetingAttachment> _attachments = new();

    public IReadOnlyCollection<MeetingAttachment> Attachments => _attachments.AsReadOnly();

=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Domain.Entities.Meeting;
internal class Meeting
{
>>>>>>> ae56db5 (shared on process)
}
