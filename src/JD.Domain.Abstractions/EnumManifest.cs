namespace JD.Domain.Abstractions;

/// <summary>
/// Represents metadata about an enumeration in the domain model.
/// </summary>
public sealed class EnumManifest
{
    /// <summary>
    /// Gets the name of the enumeration.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the CLR type name of the enumeration.
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Gets the namespace of the enumeration type.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the underlying type of the enumeration.
    /// </summary>
    public string UnderlyingType { get; init; } = "System.Int32";

    /// <summary>
    /// Gets the enumeration values and their names.
    /// </summary>
    public IReadOnlyDictionary<string, object> Values { get; init; } =
        new Dictionary<string, object>();

    /// <summary>
    /// Gets additional metadata about the enumeration.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } =
        new Dictionary<string, object?>();
}
