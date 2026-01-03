namespace JD.Domain.Abstractions;

/// <summary>
/// Represents a set of related domain rules.
/// </summary>
public sealed class RuleSetManifest
{
    /// <summary>
    /// Gets the name of the rule set (e.g., "Create", "Update", "Default").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the name of the entity or type this rule set applies to.
    /// </summary>
    public required string TargetType { get; init; }

    /// <summary>
    /// Gets the rules in this set.
    /// </summary>
    public IReadOnlyList<RuleManifest> Rules { get; init; } = 
        [];

    /// <summary>
    /// Gets the names of other rule sets that this set includes.
    /// </summary>
    public IReadOnlyList<string> Includes { get; init; } = 
        [];

    /// <summary>
    /// Gets additional metadata about the rule set.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
