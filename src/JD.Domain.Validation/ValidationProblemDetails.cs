using Microsoft.AspNetCore.Mvc;

namespace JD.Domain.Validation;

/// <summary>
/// Extended ProblemDetails with domain-specific validation error information.
/// </summary>
public class ValidationProblemDetails : ProblemDetails
{
    /// <summary>
    /// Domain-specific error type URI prefix.
    /// </summary>
    public const string TypePrefix = "https://jd.domain/validation-errors/";

    /// <summary>
    /// Gets or sets the collection of validation errors grouped by target property.
    /// Compatible with ASP.NET Core's ModelState error format.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; set; } =
        new Dictionary<string, string[]>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the collection of domain errors with full metadata.
    /// </summary>
    public IReadOnlyList<DomainValidationError> DomainErrors { get; set; } = [];

    /// <summary>
    /// Gets or sets the correlation ID for request tracking.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the rule sets that were evaluated.
    /// </summary>
    public IReadOnlyList<string> RuleSetsEvaluated { get; set; } = [];
}
