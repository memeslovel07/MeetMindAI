using FluentValidation;

namespace MeetMindAI.Application.Features.ActionItems.CompleteActionItem;

/// <summary>
/// Validates <see cref="CompleteActionItemCommand"/>.
/// </summary>
public sealed class CompleteActionItemCommandValidator
    : AbstractValidator<CompleteActionItemCommand>
{
    public CompleteActionItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
