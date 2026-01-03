namespace JD.Domain.Abstractions;

/// <summary>
/// Provides information about the source of domain model definitions.
/// </summary>
public sealed class SourceInfo
{
    /// <summary>
    /// Gets the type of source (e.g., "DSL", "EF", "Reflection").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the location or identifier of the source.
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Gets additional metadata about the source.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Gets the timestamp when this source was processed.
    /// </summary>
    public DateTimeOffset? Timestamp { get; init; }
}
