using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation.Results;

using MeetMindAI.Shared.Results;

namespace MeetMindAI.Application.Common.Validation;

/// <summary>
/// Maps FluentValidation failures to application errors.
/// </summary>
internal static class ValidationErrorMapper
{
    /// <summary>
    /// Converts validation failures into application errors.
    /// </summary>
    public static IReadOnlyList<Error> Map(
        IEnumerable<ValidationFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);

        return failures
            .Select(failure => new Error(
                Code: string.IsNullOrWhiteSpace(failure.ErrorCode)
                    ? "Validation"
                    : failure.ErrorCode,
                Description: failure.ErrorMessage,
                Target: failure.PropertyName))
            .ToList();
    }
}
