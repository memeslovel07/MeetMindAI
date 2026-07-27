using FluentValidation;

namespace MeetMindAI.Application.Features.ActionItems.DeleteActionItem;

/// <summary>
/// Validates <see cref="DeleteActionItemCommand"/>.
/// </summary>
public sealed class DeleteActionItemCommandValidator
    : AbstractValidator<DeleteActionItemCommand>
{
    public DeleteActionItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
