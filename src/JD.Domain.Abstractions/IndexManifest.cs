namespace JD.Domain.Abstractions;

/// <summary>
/// Represents an index configuration.
/// </summary>
public sealed class IndexManifest
{
    /// <summary>
    /// Gets the name of the index.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the properties included in the index.
    /// </summary>
    public IReadOnlyList<string> Properties { get; init; } = 
        Array.Empty<string>();

    /// <summary>
    /// Gets a value indicating whether the index is unique.
    /// </summary>
    public bool IsUnique { get; init; }

    /// <summary>
    /// Gets the filter expression for the index.
    /// </summary>
    public string? Filter { get; init; }

    /// <summary>
    /// Gets the included properties (for covering indexes).
    /// </summary>
    public IReadOnlyList<string> IncludedProperties { get; init; } = 
        Array.Empty<string>();

    /// <summary>
    /// Gets additional index metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
