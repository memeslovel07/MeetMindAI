using FluentValidation;

using MeetMindAI.Domain.Constants;

namespace MeetMindAI.Application.Meetings.UpdateMeeting;

/// <summary>
/// Validates <see cref="UpdateMeetingCommand"/>.
/// </summary>
public sealed class UpdateMeetingCommandValidator
    : AbstractValidator<UpdateMeetingCommand>
{
    public UpdateMeetingCommandValidator()
    {
        RuleFor(x => x.MeetingId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(
                ValidationConstants.MeetingTitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(
                ValidationConstants.MeetingDescriptionMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(
                ValidationConstants.MeetingMinDurationMinutes,
                ValidationConstants.MeetingMaxDurationMinutes);

        RuleFor(x => x.ScheduledAtUtc)
            .Must(x => !x.HasValue || x.Value > DateTime.UtcNow)
            .WithMessage("Scheduled date must be in the future.");
    }
}
