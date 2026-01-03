using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JD.Domain.Abstractions;

namespace JD.Domain.Rules;

/// <summary>
/// Fluent builder for configuring individual rules.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class RuleBuilder<T> where T : class
{
    private readonly RuleSetBuilder<T> _ruleSetBuilder;
    private RuleManifest _rule;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleBuilder{T}"/> class.
    /// </summary>
    /// <param name="ruleSetBuilder">The parent rule set builder.</param>
    /// <param name="rule">The rule being configured.</param>
    internal RuleBuilder(RuleSetBuilder<T> ruleSetBuilder, RuleManifest rule)
    {
        _ruleSetBuilder = ruleSetBuilder;
        _rule = rule;
    }

    /// <summary>
    /// Sets the error message for rule violations.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>The rule builder for chaining.</returns>
    public RuleBuilder<T> WithMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));

        _rule = new RuleManifest
        {
            Id = _rule.Id,
            Category = _rule.Category,
            TargetType = _rule.TargetType,
            Message = message,
            Severity = _rule.Severity,
            Tags = _rule.Tags,
            Expression = _rule.Expression,
            Metadata = _rule.Metadata
        };

        _ruleSetBuilder.ReplaceRule(_rule);
        return this;
    }

    /// <summary>
    /// Sets the severity of rule violations.
    /// </summary>
    /// <param name="severity">The severity level.</param>
    /// <returns>The rule builder for chaining.</returns>
    public RuleBuilder<T> WithSeverity(RuleSeverity severity)
    {
        _rule = new RuleManifest
        {
            Id = _rule.Id,
            Category = _rule.Category,
            TargetType = _rule.TargetType,
            Message = _rule.Message,
            Severity = severity,
            Tags = _rule.Tags,
            Expression = _rule.Expression,
            Metadata = _rule.Metadata
        };

        _ruleSetBuilder.ReplaceRule(_rule);
        return this;
    }

    /// <summary>
    /// Adds a tag to the rule for categorization.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    /// <returns>The rule builder for chaining.</returns>
    public RuleBuilder<T> WithTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(tag));

        var tags = new List<string>(_rule.Tags) { tag };
        _rule = new RuleManifest
        {
            Id = _rule.Id,
            Category = _rule.Category,
            TargetType = _rule.TargetType,
            Message = _rule.Message,
            Severity = _rule.Severity,
            Tags = tags.ToList().AsReadOnly(),
            Expression = _rule.Expression,
            Metadata = _rule.Metadata
        };

        _ruleSetBuilder.ReplaceRule(_rule);
        return this;
    }

    /// <summary>
    /// Adds metadata to the rule.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The rule builder for chaining.</returns>
    public RuleBuilder<T> WithMetadata(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

        var metadata = new Dictionary<string, object?>(_rule.Metadata.ToDictionary(x => x.Key, x => x.Value))
        {
            [key] = value
        };

        _rule = new RuleManifest
        {
            Id = _rule.Id,
            Category = _rule.Category,
            TargetType = _rule.TargetType,
            Message = _rule.Message,
            Severity = _rule.Severity,
            Tags = _rule.Tags,
            Expression = _rule.Expression,
            Metadata = metadata.ToDictionary(x => x.Key, x => x.Value)
        };

        _ruleSetBuilder.ReplaceRule(_rule);
        return this;
    }

    /// <summary>
    /// Adds another invariant rule to the rule set.
    /// </summary>
    /// <param name="id">The unique identifier for the rule.</param>
    /// <param name="predicate">The rule predicate expression.</param>
    /// <returns>A rule builder for further configuration.</returns>
    public RuleBuilder<T> Invariant(string id, Expression<Func<T, bool>> predicate)
    {
        return _ruleSetBuilder.Invariant(id, predicate);
    }

    /// <summary>
    /// Adds a validator rule to the rule set.
    /// </summary>
    /// <param name="id">The unique identifier for the rule.</param>
    /// <param name="predicate">The rule predicate expression.</param>
    /// <returns>A rule builder for further configuration.</returns>
    public RuleBuilder<T> Validator(string id, Expression<Func<T, bool>> predicate)
    {
        return _ruleSetBuilder.Validator(id, predicate);
    }

    /// <summary>
    /// Builds the rule set manifest.
    /// </summary>
    /// <returns>The constructed rule set manifest.</returns>
    public RuleSetManifest Build()
    {
        return _ruleSetBuilder.Build();
    }
}
