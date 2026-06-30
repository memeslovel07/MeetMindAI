using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMindAI.Shared.Results;

/// <summary>
/// Represents a business or application error.
/// </summary>
/// <param name="Code">
/// A unique machine-readable error code.
/// </param>
/// <param name="Description">
/// A human-readable description of the error.
/// </param>
public sealed record Error(
    string Code,
    string Description)
{
    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static readonly Error None = new(
        string.Empty,
        string.Empty);
}
