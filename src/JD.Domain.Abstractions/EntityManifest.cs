namespace JD.Domain.Abstractions;

/// <summary>
/// Represents metadata about an entity in the domain model.
/// </summary>
public sealed class EntityManifest
{
    /// <summary>
    /// Gets the name of the entity.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the CLR type name of the entity.
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Gets the namespace of the entity type.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the properties of the entity.
    /// </summary>
    public IReadOnlyList<PropertyManifest> Properties { get; init; } = 
        Array.Empty<PropertyManifest>();

    /// <summary>
    /// Gets the names of key properties.
    /// </summary>
    public IReadOnlyList<string> KeyProperties { get; init; } = 
        Array.Empty<string>();

    /// <summary>
    /// Gets the table name for persistence, if applicable.
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Gets the schema name for persistence, if applicable.
    /// </summary>
    public string? SchemaName { get; init; }

    /// <summary>
    /// Gets additional metadata about the entity.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
