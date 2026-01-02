using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JD.Domain.Abstractions;

/// <summary>
/// Represents metadata about entity configuration (EF Core or other persistence mappings).
/// </summary>
public sealed class ConfigurationManifest
{
    /// <summary>
    /// Gets the name of the entity this configuration applies to.
    /// </summary>
    public required string EntityName { get; init; }

    /// <summary>
    /// Gets the type name of the entity this configuration applies to.
    /// </summary>
    public required string EntityTypeName { get; init; }

    /// <summary>
    /// Gets the table name, if applicable.
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Gets the schema name, if applicable.
    /// </summary>
    public string? SchemaName { get; init; }

    /// <summary>
    /// Gets the key configuration.
    /// </summary>
    public IReadOnlyList<string> KeyProperties { get; init; } = 
        Array.Empty<string>();

    /// <summary>
    /// Gets the property configurations.
    /// </summary>
    public IReadOnlyDictionary<string, PropertyConfigurationManifest> PropertyConfigurations { get; init; } = 
        new Dictionary<string, PropertyConfigurationManifest>();

    /// <summary>
    /// Gets the index configurations.
    /// </summary>
    public IReadOnlyList<IndexManifest> Indexes { get; init; } = 
        Array.Empty<IndexManifest>();

    /// <summary>
    /// Gets the relationship configurations.
    /// </summary>
    public IReadOnlyList<RelationshipManifest> Relationships { get; init; } = 
        Array.Empty<RelationshipManifest>();

    /// <summary>
    /// Gets additional configuration metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
