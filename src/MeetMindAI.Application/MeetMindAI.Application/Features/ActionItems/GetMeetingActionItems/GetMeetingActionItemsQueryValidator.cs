using FluentValidation;

namespace MeetMindAI.Application.Features.ActionItems.GetMeetingActionItems;

/// <summary>
/// Validates <see cref="GetMeetingActionItemsQuery"/>.
/// </summary>
public sealed class GetMeetingActionItemsQueryValidator
    : AbstractValidator<GetMeetingActionItemsQuery>
{
    public GetMeetingActionItemsQueryValidator()
    {
        RuleFor(x => x.MeetingId)
            .NotEmpty();
    }
}
