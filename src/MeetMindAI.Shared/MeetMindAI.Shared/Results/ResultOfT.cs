using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace MeetMindAI.Shared.Results;

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// </summary>
/// <typeparam name="T">
/// The type of the value returned by the operation.
/// </typeparam>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(
        T? value,
        bool isSuccess,
        Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value of the successful result.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to access the value of a failed result.
    /// </exception>
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "The value of a failed result cannot be accessed.");

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result<T> Success(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new Result<T>(
            value,
            true,
            Error.None);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public new static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<T>(
            default,
            false,
            error);
    }
}
