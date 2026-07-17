using Microsoft.Extensions.Options;

namespace MeetMindAI.Application.Common.Options;

public sealed class AiOptionsValidator : IValidateOptions<AiOptions>
{
    public ValidateOptionsResult Validate(string? name, AiOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PromptVersion))
        {
            return ValidateOptionsResult.Fail(
                "Ai:PromptVersion is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
