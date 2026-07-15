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
/// <param name="Target">
/// The field or member associated with the error, if applicable.
/// Used primarily for validation errors.
/// </param>
public sealed record Error(
    string Code,
    string Description,
    string? Target = null)
{
    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static readonly Error None = new(
        string.Empty,
        string.Empty);

    public static readonly Error Unauthorized =
    new(
        "General.Unauthorized",
        "The current user is not authorized to perform this operation.");
}
