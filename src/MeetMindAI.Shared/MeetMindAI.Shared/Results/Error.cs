<<<<<<< HEAD
=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

>>>>>>> ae56db5 (shared on process)
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
<<<<<<< HEAD
/// <param name="Target">
/// The field or member associated with the error, if applicable.
/// Used primarily for validation errors.
/// </param>
public sealed record Error(
    string Code,
    string Description,
    string? Target = null)
=======
public sealed record Error(
    string Code,
    string Description)
>>>>>>> ae56db5 (shared on process)
{
    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static readonly Error None = new(
        string.Empty,
        string.Empty);
<<<<<<< HEAD

    public static readonly Error Unauthorized =
    new(
        "General.Unauthorized",
        "The current user is not authorized to perform this operation.");

    public static readonly Error Forbidden =
    new(
        "General.Forbidden",
        "You are not allowed to perform this operation.");

    public static readonly Error NotFound =
        new(
        "General.NotFound",
        "The requested resource was not found.");

    public static readonly Error Conflict =
        new(
        "General.Conflict",
        "The request could not be completed due to a conflict with the current state of the resource.");

    public static readonly Error ValidationError =
        new(
        "General.ValidationError",
        "One or more validation errors occurred.");

    public static readonly Error InternalServerError =
        new(
        "General.InternalServerError",
        "An unexpected error occurred while processing the request.");

    

=======
>>>>>>> ae56db5 (shared on process)
}
