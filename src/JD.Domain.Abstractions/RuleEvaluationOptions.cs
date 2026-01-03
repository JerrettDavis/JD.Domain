using System.Collections.Generic;

namespace JD.Domain.Abstractions;

/// <summary>
/// Represents the evaluation options for domain rules.
/// </summary>
public sealed class RuleEvaluationOptions
{
    /// <summary>
    /// Gets the name of the rule set to evaluate. If null, evaluates all rules.
    /// </summary>
    public string? RuleSet { get; init; }

    /// <summary>
    /// Gets the name of a specific property to evaluate rules for.
    /// If null, evaluates rules for all properties.
    /// </summary>
    public string? PropertyName { get; init; }

    /// <summary>
    /// Gets a value indicating whether to stop on first error.
    /// </summary>
    public bool StopOnFirstError { get; init; }

    /// <summary>
    /// Gets a value indicating whether to include informational results.
    /// </summary>
    public bool IncludeInfo { get; init; }

    /// <summary>
    /// Gets additional context data for rule evaluation.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Context { get; init; } = 
        new Dictionary<string, object?>();

    /// <summary>
    /// Gets the default evaluation options.
    /// </summary>
    public static RuleEvaluationOptions Default { get; } = new();
}
