using System;
using System.Collections.Generic;

namespace JD.Domain.Abstractions;

/// <summary>
/// Represents metadata about a domain rule.
/// </summary>
public sealed class RuleManifest
{
    /// <summary>
    /// Gets the unique identifier of the rule.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the category of the rule (Invariant, Validator, Policy, Derivation, StateTransition).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Gets the name of the entity or type this rule applies to.
    /// </summary>
    public required string TargetType { get; init; }

    /// <summary>
    /// Gets the error message template for rule violations.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the severity of rule violations.
    /// </summary>
    public RuleSeverity Severity { get; init; } = RuleSeverity.Error;

    /// <summary>
    /// Gets the tags associated with this rule.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the expression representation of the rule, if serializable.
    /// </summary>
    public string? Expression { get; init; }

    /// <summary>
    /// Gets additional metadata about the rule.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
