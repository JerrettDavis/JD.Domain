namespace JD.Domain.Abstractions;

/// <summary>
/// Represents metadata about a value object in the domain model.
/// </summary>
public sealed class ValueObjectManifest
{
    /// <summary>
    /// Gets the name of the value object.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the CLR type name of the value object.
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Gets the namespace of the value object type.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the properties of the value object.
    /// </summary>
    public IReadOnlyList<PropertyManifest> Properties { get; init; } = 
        [];

    /// <summary>
    /// Gets additional metadata about the value object.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
