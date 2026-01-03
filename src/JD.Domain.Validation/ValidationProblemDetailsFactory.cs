using JD.Domain.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JD.Domain.Validation;

/// <summary>
/// Factory for creating <see cref="ValidationProblemDetails"/> from various sources.
/// </summary>
public sealed class ValidationProblemDetailsFactory
{
    /// <summary>
    /// Creates a <see cref="ValidationProblemDetails"/> from a <see cref="RuleEvaluationResult"/>.
    /// </summary>
    /// <param name="result">The rule evaluation result.</param>
    /// <param name="context">Optional HTTP context for request information.</param>
    /// <param name="statusCode">Optional status code override.</param>
    /// <returns>A new <see cref="ValidationProblemDetails"/> instance.</returns>
    public ValidationProblemDetails CreateFromResult(
        RuleEvaluationResult result,
        HttpContext? context = null,
        int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = ProblemDetailsBuilder.Create()
            .FromEvaluationResult(result);

        if (context is not null)
        {
            builder
                .WithInstance(context.Request.Path)
                .WithCorrelationId(context.TraceIdentifier);
        }

        if (statusCode.HasValue)
        {
            builder.WithStatus(statusCode.Value);
        }

        return builder.Build();
    }

    /// <summary>
    /// Creates a <see cref="ValidationProblemDetails"/> from a <see cref="DomainValidationException"/>.
    /// </summary>
    /// <param name="exception">The domain validation exception.</param>
    /// <param name="context">Optional HTTP context for request information.</param>
    /// <param name="statusCode">Optional status code override.</param>
    /// <returns>A new <see cref="ValidationProblemDetails"/> instance.</returns>
    public ValidationProblemDetails CreateFromException(
        DomainValidationException exception,
        HttpContext? context = null,
        int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var builder = ProblemDetailsBuilder.Create()
            .FromException(exception);

        if (context is not null)
        {
            builder
                .WithInstance(context.Request.Path)
                .WithCorrelationId(context.TraceIdentifier);
        }

        if (statusCode.HasValue)
        {
            builder.WithStatus(statusCode.Value);
        }

        return builder.Build();
    }

    /// <summary>
    /// Creates a <see cref="ValidationProblemDetails"/> from a collection of <see cref="DomainError"/>.
    /// </summary>
    /// <param name="errors">The domain errors.</param>
    /// <param name="context">Optional HTTP context for request information.</param>
    /// <param name="statusCode">Optional status code override.</param>
    /// <returns>A new <see cref="ValidationProblemDetails"/> instance.</returns>
    public ValidationProblemDetails CreateFromErrors(
        IEnumerable<DomainError> errors,
        HttpContext? context = null,
        int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorList = errors.ToList();
        var builder = ProblemDetailsBuilder.Create()
            .WithErrors(errorList)
            .WithDetail(errorList.Count == 1
                ? errorList[0].Message
                : $"Validation failed with {errorList.Count} errors.");

        if (context is not null)
        {
            builder
                .WithInstance(context.Request.Path)
                .WithCorrelationId(context.TraceIdentifier);
        }

        if (statusCode.HasValue)
        {
            builder.WithStatus(statusCode.Value);
        }

        return builder.Build();
    }
}
