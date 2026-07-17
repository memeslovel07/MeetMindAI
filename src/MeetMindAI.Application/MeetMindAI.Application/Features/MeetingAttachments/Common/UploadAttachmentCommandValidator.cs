using FluentValidation;

namespace MeetMindAI.Application.Features.MeetingAttachments.Common;

public sealed class UploadAttachmentCommandValidator
    : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.MeetingId)
            .NotEmpty();

        

        RuleFor(x => x.File)
            .NotNull();

        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .When(x => x.File != null);
    }
}
