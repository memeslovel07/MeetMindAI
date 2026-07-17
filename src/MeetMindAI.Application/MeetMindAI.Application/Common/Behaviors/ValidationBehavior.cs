using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;
using FluentValidation.Results;

using MediatR;

using MeetMindAI.Application.Common.Validation;
using MeetMindAI.Shared.Results;



namespace MeetMindAI.Application.Common.Behaviors;

/// <summary>
/// Validates incoming MediatR requests before they reach their handlers.
/// </summary>
/// <typeparam name="TRequest">
/// The request type.
/// </typeparam>
/// <typeparam name="TResponse">
/// The response type.
/// </typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{

    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators)
    {
        ArgumentNullException.ThrowIfNull(validators);

        _validators = validators;
    }
    /// <inheritdoc />
    public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(
                validator => validator.ValidateAsync(
                    context,
                    cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (failures.Count != 0)
        {
            // We'll handle this in Iteration 3.
            var errors = ValidationErrorMapper.Map(failures);
            var error = errors.First();

            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(error);
            }

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(TResponse).GetMethod(
                    nameof(Result<object>.Failure),
                    [typeof(Error)]);

                return (TResponse)failureMethod!
                    .Invoke(null, [error])!;
            }

            throw new InvalidOperationException(
                $"ValidationBehavior supports only Result and Result<T>. Response type '{typeof(TResponse).Name}' is not supported.");
        }

        return await next();
    }
}
