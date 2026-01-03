using System;
using JD.Domain.Abstractions;

namespace JD.Domain.Rules;

/// <summary>
/// Represents a compiled rule that can be evaluated at runtime.
/// </summary>
/// <typeparam name="T">The type this rule applies to.</typeparam>
public sealed class CompiledRule<T> where T : class
{
    /// <summary>
    /// Gets the rule manifest containing metadata about the rule.
    /// </summary>
    public RuleManifest Manifest { get; }

    /// <summary>
    /// Gets the compiled predicate that evaluates the rule.
    /// </summary>
    public Func<T, bool> Predicate { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledRule{T}"/> class.
    /// </summary>
    /// <param name="manifest">The rule manifest.</param>
    /// <param name="predicate">The compiled predicate.</param>
    public CompiledRule(RuleManifest manifest, Func<T, bool> predicate)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    /// <summary>
    /// Evaluates the rule against the specified instance.
    /// </summary>
    /// <param name="instance">The instance to evaluate.</param>
    /// <returns>True if the rule passes; otherwise, false.</returns>
    public bool Evaluate(T instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        return Predicate(instance);
    }
}
