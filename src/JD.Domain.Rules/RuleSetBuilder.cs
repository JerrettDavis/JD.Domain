using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JD.Domain.Abstractions;

namespace JD.Domain.Rules;

/// <summary>
/// Fluent builder for constructing rule sets.
/// </summary>
/// <typeparam name="T">The entity type this rule set applies to.</typeparam>
public sealed class RuleSetBuilder<T> where T : class
{
    private readonly string _name;
    private readonly Type _targetType = typeof(T);
    private readonly List<RuleManifest> _rules = [];
    private readonly Dictionary<string, Func<T, bool>> _compiledPredicates = new();
    private readonly List<string> _includes = [];
    private readonly Dictionary<string, object?> _metadata = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleSetBuilder{T}"/> class with default name.
    /// </summary>
    public RuleSetBuilder()
        : this("Default")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleSetBuilder{T}"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name of the rule set.</param>
    public RuleSetBuilder(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        _name = name;
    }

    /// <summary>
    /// Adds an invariant rule that must always be true.
    /// </summary>
    /// <param name="id">The unique identifier for the rule.</param>
    /// <param name="predicate">The rule predicate expression.</param>
    /// <returns>A rule builder for further configuration.</returns>
    public RuleBuilder<T> Invariant(string id, Expression<Func<T, bool>> predicate)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var rule = new RuleManifest
        {
            Id = id,
            Category = "Invariant",
            TargetType = _targetType.FullName ?? _targetType.Name,
            Expression = predicate.ToString()
        };

        _rules.Add(rule);
        _compiledPredicates[id] = predicate.Compile();
        return new RuleBuilder<T>(this, rule);
    }

    /// <summary>
    /// Adds a validator rule for input validation.
    /// </summary>
    /// <param name="id">The unique identifier for the rule.</param>
    /// <param name="predicate">The rule predicate expression.</param>
    /// <returns>A rule builder for further configuration.</returns>
    public RuleBuilder<T> Validator(string id, Expression<Func<T, bool>> predicate)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var rule = new RuleManifest
        {
            Id = id,
            Category = "Validator",
            TargetType = _targetType.FullName ?? _targetType.Name,
            Expression = predicate.ToString()
        };

        _rules.Add(rule);
        _compiledPredicates[id] = predicate.Compile();
        return new RuleBuilder<T>(this, rule);
    }

    /// <summary>
    /// Includes another rule set by name.
    /// </summary>
    /// <param name="ruleSetName">The name of the rule set to include.</param>
    /// <returns>The rule set builder for chaining.</returns>
    public RuleSetBuilder<T> Include(string ruleSetName)
    {
        if (string.IsNullOrWhiteSpace(ruleSetName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(ruleSetName));
        
        if (!_includes.Contains(ruleSetName))
        {
            _includes.Add(ruleSetName);
        }

        return this;
    }

    /// <summary>
    /// Adds metadata to the rule set.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The rule set builder for chaining.</returns>
    public RuleSetBuilder<T> WithMetadata(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the rule set manifest.
    /// </summary>
    /// <returns>The constructed rule set manifest.</returns>
    public RuleSetManifest Build()
    {
        return new RuleSetManifest
        {
            Name = _name,
            TargetType = _targetType.FullName ?? _targetType.Name,
            Rules = _rules.ToList().AsReadOnly(),
            Includes = _includes.ToList().AsReadOnly(),
            Metadata = _metadata.ToDictionary(x => x.Key, x => x.Value) as IReadOnlyDictionary<string, object?>
        };
    }

    /// <summary>
    /// Builds a compiled rule set that can be evaluated at runtime.
    /// </summary>
    /// <returns>The compiled rule set.</returns>
    public CompiledRuleSet<T> BuildCompiled()
    {
        var compiledRules = _rules
            .Where(r => _compiledPredicates.ContainsKey(r.Id))
            .Select(r => new CompiledRule<T>(r, _compiledPredicates[r.Id]))
            .ToList();

        return new CompiledRuleSet<T>(_name, compiledRules);
    }

    /// <summary>
    /// Replaces the last added rule with the updated version.
    /// </summary>
    /// <param name="updatedRule">The updated rule.</param>
    internal void ReplaceRule(RuleManifest updatedRule)
    {
        if (_rules.Count > 0)
        {
            var lastIndex = _rules.Count - 1;
            if (_rules[lastIndex].Id == updatedRule.Id)
            {
                _rules[lastIndex] = updatedRule;
            }
        }
    }
}
