using FluentValidation;

using MeetMindAI.Domain.Constants;

namespace MeetMindAI.Application.Meetings.CreateMeeting;

/// <summary>
/// Validates <see cref="CreateMeetingCommand"/>.
/// </summary>
public sealed class CreateMeetingCommandValidator
    : AbstractValidator<CreateMeetingCommand>
{
    public CreateMeetingCommandValidator()
    {
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
