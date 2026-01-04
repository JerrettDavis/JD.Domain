using JD.Domain.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JD.Domain.Validation;

/// <summary>
/// Fluent builder for creating <see cref="ValidationProblemDetails"/> instances.
/// </summary>
public sealed class ProblemDetailsBuilder
{
    private readonly ValidationProblemDetails _details;

    private ProblemDetailsBuilder()
    {
        _details = new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = $"{ValidationProblemDetails.TypePrefix}validation-failed"
        };
    }

    /// <summary>
    /// Creates a new <see cref="ProblemDetailsBuilder"/> instance.
    /// </summary>
    public static ProblemDetailsBuilder Create() => new();

    /// <summary>
    /// Sets the title of the problem details.
    /// </summary>
    public ProblemDetailsBuilder WithTitle(string title)
    {
        _details.Title = title;
        return this;
    }

    /// <summary>
    /// Sets the HTTP status code.
    /// </summary>
    public ProblemDetailsBuilder WithStatus(int status)
    {
        _details.Status = status;
        return this;
    }

    /// <summary>
    /// Sets the detail message.
    /// </summary>
    public ProblemDetailsBuilder WithDetail(string detail)
    {
        _details.Detail = detail;
        return this;
    }

    /// <summary>
    /// Sets the instance URI (typically the request path).
    /// </summary>
    public ProblemDetailsBuilder WithInstance(string? instance)
    {
        _details.Instance = instance;
        return this;
    }

    /// <summary>
    /// Sets the type URI for the problem.
    /// </summary>
    public ProblemDetailsBuilder WithType(string type)
    {
        _details.Type = type;
        return this;
    }

    /// <summary>
    /// Sets the correlation ID for request tracking.
    /// </summary>
    public ProblemDetailsBuilder WithCorrelationId(string? correlationId)
    {
        _details.CorrelationId = correlationId;
        return this;
    }

    /// <summary>
    /// Populates the problem details from a <see cref="RuleEvaluationResult"/>.
    /// </summary>
    public ProblemDetailsBuilder FromEvaluationResult(RuleEvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var domainErrors = result.Errors
            .Select(DomainValidationError.FromDomainError)
            .ToList();

        _details.DomainErrors = domainErrors;
        _details.RuleSetsEvaluated = result.RuleSetsEvaluated;

        // Group errors by target for ASP.NET Core ModelState compatibility
        var grouped = domainErrors
            .GroupBy(e => e.Target ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray(),
                StringComparer.Ordinal);

        _details.Errors = grouped;

        _details.Detail = result.Errors.Count == 1
            ? result.Errors[0].Message
            : $"Validation failed with {result.Errors.Count} errors.";

        return this;
    }

    /// <summary>
    /// Populates the problem details from a <see cref="DomainValidationException"/>.
    /// </summary>
    public ProblemDetailsBuilder FromException(DomainValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var domainErrors = exception.Errors
            .Select(DomainValidationError.FromDomainError)
            .ToList();

        _details.DomainErrors = domainErrors;
        _details.Detail = exception.Message;

        var grouped = domainErrors
            .GroupBy(e => e.Target ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray(),
                StringComparer.Ordinal);

        _details.Errors = grouped;

        return this;
    }

    /// <summary>
    /// Adds domain errors directly.
    /// </summary>
    public ProblemDetailsBuilder WithErrors(IEnumerable<DomainError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var domainErrors = errors
            .Select(DomainValidationError.FromDomainError)
            .ToList();

        _details.DomainErrors = domainErrors;

        var grouped = domainErrors
            .GroupBy(e => e.Target ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray(),
                StringComparer.Ordinal);

        _details.Errors = grouped;

        return this;
    }

    /// <summary>
    /// Adds an extension property to the problem details.
    /// </summary>
    public ProblemDetailsBuilder WithExtension(string key, object? value)
    {
        _details.Extensions[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the final <see cref="ValidationProblemDetails"/> instance.
    /// </summary>
    public ValidationProblemDetails Build() => _details;
}
