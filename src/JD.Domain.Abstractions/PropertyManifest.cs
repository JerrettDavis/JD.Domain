using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JD.Domain.Abstractions;

/// <summary>
/// Represents metadata about a property in a domain entity or value object.
/// </summary>
public sealed class PropertyManifest
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the CLR type name of the property.
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the property is required (non-nullable).
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets a value indicating whether the property is a collection.
    /// </summary>
    public bool IsCollection { get; init; }

    /// <summary>
    /// Gets the maximum length constraint, if applicable.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Gets the precision for numeric types, if applicable.
    /// </summary>
    public int? Precision { get; init; }

    /// <summary>
    /// Gets the scale for numeric types, if applicable.
    /// </summary>
    public int? Scale { get; init; }

    /// <summary>
    /// Gets a value indicating whether the property is a concurrency token.
    /// </summary>
    public bool IsConcurrencyToken { get; init; }

    /// <summary>
    /// Gets a value indicating whether the property is computed.
    /// </summary>
    public bool IsComputed { get; init; }

    /// <summary>
    /// Gets additional metadata about the property.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
