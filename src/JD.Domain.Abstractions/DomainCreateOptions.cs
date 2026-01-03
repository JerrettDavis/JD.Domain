namespace JD.Domain.Abstractions;

/// <summary>
/// Represents options for creating domain instances.
/// </summary>
public sealed class DomainCreateOptions
{
    /// <summary>
    /// Gets the rule set to use for validation during creation.
    /// </summary>
    public string? RuleSet { get; init; }

    /// <summary>
    /// Gets the domain context for the creation operation.
    /// </summary>
    public DomainContext? Context { get; init; }

    /// <summary>
    /// Gets a value indicating whether to throw exceptions on validation failure.
    /// </summary>
    public bool ThrowOnFailure { get; init; }

    /// <summary>
    /// Gets additional creation options.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Properties { get; init; } =
        new Dictionary<string, object?>();

    /// <summary>
    /// Gets the default creation options.
    /// </summary>
    public static DomainCreateOptions Default { get; } = new();
}
