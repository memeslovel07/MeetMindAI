using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

using MeetMindAI.Domain.Constants;

namespace MeetMindAI.Application.Authentication.Register;

/// <summary>
/// Validates a <see cref="RegisterUserCommand"/>.
/// </summary>
public sealed class RegisterUserCommandValidator
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
           .MaximumLength(ValidationConstants.FirstNameMaxLength)
            .MinimumLength(2);

        RuleFor(x => x.LastName)
            .NotEmpty()
           .MaximumLength(ValidationConstants.LastNameMaxLength)
            .MinimumLength(2);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(ValidationConstants.EmailMaxLength)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
                .WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character.");
    }
}
