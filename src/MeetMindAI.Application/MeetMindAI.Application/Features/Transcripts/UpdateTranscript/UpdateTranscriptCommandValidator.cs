using FluentValidation;

using MeetMindAI.Application.Features.Transcripts.CreateTranscript;

namespace MeetMindAI.Application.Features.Transcripts.CreateTranscript;

/// <summary>
/// Validates transcript creation requests.
/// </summary>
public sealed class UpdateTranscriptCommandValidator
    : AbstractValidator<CreateTranscriptCommand>
{
    public UpdateTranscriptCommandValidator()
    {
        RuleFor(x => x.MeetingId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(100000);

        RuleFor(x => x.Language)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Language));

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero)
            .When(x => x.Duration.HasValue);
    }
}
