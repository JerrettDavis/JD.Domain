using JD.Domain.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JD.Domain.AspNetCore;

/// <summary>
/// Configuration options for domain validation in ASP.NET Core.
/// </summary>
public sealed class DomainValidationOptions
{
    /// <summary>
    /// Gets or sets the default rule set to evaluate. Null means all rules.
    /// </summary>
    public string? DefaultRuleSet { get; set; }

    /// <summary>
    /// Gets or sets whether to stop on first error during validation.
    /// </summary>
    public bool StopOnFirstError { get; set; }

    /// <summary>
    /// Gets or sets whether to include info-level messages in responses.
    /// </summary>
    public bool IncludeInfo { get; set; }

    /// <summary>
    /// Gets or sets whether to include warnings in the response.
    /// </summary>
    public bool IncludeWarnings { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to suppress validation for GET requests.
    /// </summary>
    public bool SuppressGetRequestValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets the HTTP status code for validation failures.
    /// Default is 400 Bad Request.
    /// </summary>
    public int ValidationFailureStatusCode { get; set; } = StatusCodes.Status400BadRequest;

    /// <summary>
    /// Gets or sets whether to handle <see cref="DomainValidationException"/> globally.
    /// </summary>
    public bool HandleExceptionsGlobally { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom factory for creating <see cref="DomainContext"/> from <see cref="HttpContext"/>.
    /// </summary>
    public Func<HttpContext, DomainContext>? DomainContextFactory { get; set; }

    /// <summary>
    /// Gets additional context properties to include in rule evaluations.
    /// </summary>
    public Dictionary<string, object?> AdditionalContext { get; } = new();
}
