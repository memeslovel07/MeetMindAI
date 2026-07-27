using FluentValidation;

namespace MeetMindAI.Application.Features.ActionItems.GetActionItem;

/// <summary>
/// Validates <see cref="GetActionItemQuery"/>.
/// </summary>
public sealed class GetActionItemQueryValidator
    : AbstractValidator<GetActionItemQuery>
{
    public GetActionItemQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
