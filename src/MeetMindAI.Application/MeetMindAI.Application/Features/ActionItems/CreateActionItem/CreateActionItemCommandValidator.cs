using FluentValidation;

using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Application.Features.ActionItems.CreateActionItem;

/// <summary>
/// Validates <see cref="CreateActionItemCommand"/>.
/// </summary>
public sealed class CreateActionItemCommandValidator
    : AbstractValidator<CreateActionItemCommand>
{
    public CreateActionItemCommandValidator()
    {
        RuleFor(x => x.MeetingId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(ActionItem.MaxTitleLength);

        RuleFor(x => x.Description)
            .MaximumLength(ActionItem.MaxDescriptionLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.DueDate)
            .Must(x => !x.HasValue || x.Value.Date >= DateTime.UtcNow.Date)
            .WithMessage("Due date cannot be earlier than today.");
    }
}
