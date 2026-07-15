using FluentValidation;

namespace MeetMindAI.Application.Meetings.DeleteMeeting;

/// <summary>
/// Validates <see cref="DeleteMeetingCommand"/>.
/// </summary>
public sealed class DeleteMeetingCommandValidator
    : AbstractValidator<DeleteMeetingCommand>
{
    public DeleteMeetingCommandValidator()
    {
        RuleFor(x => x.MeetingId)
            .NotEmpty();
    }
}
