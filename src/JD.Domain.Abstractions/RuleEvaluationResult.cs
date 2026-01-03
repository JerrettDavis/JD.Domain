using System;
using System.Collections.Generic;

namespace JD.Domain.Abstractions;

/// <summary>
/// Represents the result of evaluating domain rules against an instance.
/// </summary>
public sealed class RuleEvaluationResult
{
    /// <summary>
    /// Gets a value indicating whether all rules passed.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the collection of errors from failed rules.
    /// </summary>
    public IReadOnlyList<DomainError> Errors { get; init; } = 
        [];

    /// <summary>
    /// Gets the collection of warnings from rules.
    /// </summary>
    public IReadOnlyList<DomainError> Warnings { get; init; } = 
        [];

    /// <summary>
    /// Gets the collection of informational messages from rules.
    /// </summary>
    public IReadOnlyList<DomainError> Info { get; init; } = 
        [];

    /// <summary>
    /// Gets the total number of rules evaluated.
    /// </summary>
    public int RulesEvaluated { get; init; }

    /// <summary>
    /// Gets the names of rule sets that were evaluated.
    /// </summary>
    public IReadOnlyList<string> RuleSetsEvaluated { get; init; } = 
        [];

    /// <summary>
    /// Gets additional evaluation metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();

    /// <summary>
    /// Creates a successful evaluation result with no errors.
    /// </summary>
    /// <returns>A valid evaluation result.</returns>
    public static RuleEvaluationResult Success()
    {
        return new RuleEvaluationResult
        {
            IsValid = true
        };
    }

    /// <summary>
    /// Creates a failed evaluation result with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>An invalid evaluation result.</returns>
    public static RuleEvaluationResult Failure(IReadOnlyList<DomainError> errors)
    {
        if (errors == null) throw new ArgumentNullException(nameof(errors));
        
        return new RuleEvaluationResult
        {
            IsValid = false,
            Errors = errors
        };
    }

    /// <summary>
    /// Creates a failed evaluation result with the specified errors.
    /// </summary>
    /// <param name="errors">The errors.</param>
    /// <returns>An invalid evaluation result.</returns>
    public static RuleEvaluationResult Failure(params DomainError[] errors)
    {
        if (errors == null) throw new ArgumentNullException(nameof(errors));
        
        return new RuleEvaluationResult
        {
            IsValid = false,
            Errors = errors
        };
    }
}
