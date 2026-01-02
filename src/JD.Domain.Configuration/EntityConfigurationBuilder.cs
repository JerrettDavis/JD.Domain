using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JD.Domain.Abstractions;

namespace JD.Domain.Configuration;

/// <summary>
/// Fluent builder for constructing entity configurations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class EntityConfigurationBuilder<T> where T : class
{
    private readonly Type _entityType = typeof(T);
    private readonly List<IndexManifest> _indexes = new();
    private readonly List<RelationshipManifest> _relationships = new();
    private readonly Dictionary<string, object?> _metadata = new();
    private string? _tableName;
    private string? _schemaName;

    /// <summary>
    /// Configures the table name for the entity.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="schemaName">The optional schema name.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public EntityConfigurationBuilder<T> ToTable(string tableName, string? schemaName = null)
    {
        if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(tableName));
        _tableName = tableName;
        _schemaName = schemaName;
        return this;
    }

    /// <summary>
    /// Configures an index on the entity.
    /// </summary>
    /// <param name="propertyNames">The property names to include in the index.</param>
    /// <returns>An index builder for further configuration.</returns>
    public IndexBuilder<T> HasIndex(params string[] propertyNames)
    {
        if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
        if (propertyNames.Length == 0)
        {
            throw new ArgumentException("At least one property must be specified", nameof(propertyNames));
        }

        var index = new IndexManifest
        {
            Properties = propertyNames.ToList().AsReadOnly()
        };

        _indexes.Add(index);
        return new IndexBuilder<T>(this, index);
    }

    /// <summary>
    /// Adds metadata to the configuration.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public EntityConfigurationBuilder<T> WithMetadata(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the configuration manifest.
    /// </summary>
    /// <returns>The constructed configuration manifest.</returns>
    internal ConfigurationManifest Build()
    {
        return new ConfigurationManifest
        {
            EntityName = _entityType.Name,
            EntityTypeName = _entityType.FullName ?? _entityType.Name,
            TableName = _tableName,
            SchemaName = _schemaName,
            Indexes = _indexes.ToList().AsReadOnly(),
            Relationships = _relationships.ToList().AsReadOnly(),
            Metadata = _metadata.ToDictionary(x => x.Key, x => x.Value) as IReadOnlyDictionary<string, object?>
        };
    }

    /// <summary>
    /// Replaces an index in the configuration.
    /// </summary>
    /// <param name="updatedIndex">The updated index.</param>
    internal void ReplaceIndex(IndexManifest updatedIndex)
    {
        if (_indexes.Count > 0)
        {
            var lastIndex = _indexes.Count - 1;
            _indexes[lastIndex] = updatedIndex;
        }
    }
}
