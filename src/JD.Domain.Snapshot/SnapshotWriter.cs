using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JD.Domain.Abstractions;

namespace JD.Domain.Snapshot;

/// <summary>
/// Writes domain snapshots to canonical JSON format.
/// </summary>
public sealed class SnapshotWriter
{
    private readonly SnapshotOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotWriter"/> class.
    /// </summary>
    /// <param name="options">The snapshot options.</param>
    public SnapshotWriter(SnapshotOptions? options = null)
    {
        _options = options ?? new SnapshotOptions();
        _jsonOptions = CreateJsonOptions();
    }

    /// <summary>
    /// Creates a snapshot from a manifest.
    /// </summary>
    /// <param name="manifest">The manifest to snapshot.</param>
    /// <returns>The created snapshot with computed hash.</returns>
    public DomainSnapshot CreateSnapshot(DomainManifest manifest)
    {
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));

        // Compute hash from canonical JSON of manifest only
        var canonicalManifest = SerializeManifest(manifest);
        var hash = ComputeHash(canonicalManifest);

        return DomainSnapshot.Create(manifest, hash);
    }

    /// <summary>
    /// Serializes a snapshot to JSON.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>The JSON string.</returns>
    public string Serialize(DomainSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        var document = CreateSnapshotDocument(snapshot);
        return JsonSerializer.Serialize(document, _jsonOptions);
    }

    /// <summary>
    /// Serializes a manifest directly to canonical JSON.
    /// </summary>
    /// <param name="manifest">The manifest to serialize.</param>
    /// <returns>The canonical JSON string.</returns>
    public string SerializeManifest(DomainManifest manifest)
    {
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));

        var document = CreateManifestDocument(manifest);
        return JsonSerializer.Serialize(document, _jsonOptions);
    }

    private Dictionary<string, object?> CreateSnapshotDocument(DomainSnapshot snapshot)
    {
        var doc = new Dictionary<string, object?>();

        if (_options.IncludeSchema)
        {
            doc["$schema"] = DomainSnapshot.SchemaUri;
        }

        doc["name"] = snapshot.Name;
        doc["version"] = snapshot.Version.ToString();
        doc["hash"] = snapshot.Hash;
        doc["createdAt"] = snapshot.CreatedAt.ToString("O");
        doc["manifest"] = CreateManifestDocument(snapshot.Manifest);

        return doc;
    }

    private Dictionary<string, object?> CreateManifestDocument(DomainManifest manifest)
    {
        var doc = new Dictionary<string, object?>
        {
            ["name"] = manifest.Name,
            ["version"] = manifest.Version.ToString(),
            ["createdAt"] = manifest.CreatedAt.ToString("O")
        };

        if (!string.IsNullOrEmpty(manifest.Hash))
        {
            doc["hash"] = manifest.Hash;
        }

        // Entities (sorted by Name)
        if (manifest.Entities.Count > 0)
        {
            doc["entities"] = manifest.Entities
                .OrderBy(e => e.Name, StringComparer.Ordinal)
                .Select(CreateEntityDocument)
                .ToList();
        }

        // ValueObjects (sorted by Name)
        if (manifest.ValueObjects.Count > 0)
        {
            doc["valueObjects"] = manifest.ValueObjects
                .OrderBy(v => v.Name, StringComparer.Ordinal)
                .Select(CreateValueObjectDocument)
                .ToList();
        }

        // Enums (sorted by Name)
        if (manifest.Enums.Count > 0)
        {
            doc["enums"] = manifest.Enums
                .OrderBy(e => e.Name, StringComparer.Ordinal)
                .Select(CreateEnumDocument)
                .ToList();
        }

        // RuleSets (sorted by Name, then TargetType)
        if (manifest.RuleSets.Count > 0)
        {
            doc["ruleSets"] = manifest.RuleSets
                .OrderBy(r => r.Name, StringComparer.Ordinal)
                .ThenBy(r => r.TargetType, StringComparer.Ordinal)
                .Select(CreateRuleSetDocument)
                .ToList();
        }

        // Configurations (sorted by EntityName)
        if (manifest.Configurations.Count > 0)
        {
            doc["configurations"] = manifest.Configurations
                .OrderBy(c => c.EntityName, StringComparer.Ordinal)
                .Select(CreateConfigurationDocument)
                .ToList();
        }

        // Sources
        if (manifest.Sources.Count > 0)
        {
            doc["sources"] = manifest.Sources
                .OrderBy(s => s.Type, StringComparer.Ordinal)
                .ThenBy(s => s.Location, StringComparer.Ordinal)
                .Select(CreateSourceDocument)
                .ToList();
        }

        // Metadata (sorted by key)
        if (manifest.Metadata.Count > 0)
        {
            doc["metadata"] = CreateSortedMetadata(manifest.Metadata);
        }

        return doc;
    }

    private Dictionary<string, object?> CreateEntityDocument(EntityManifest entity)
    {
        var doc = new Dictionary<string, object?>
        {
            ["name"] = entity.Name,
            ["typeName"] = entity.TypeName
        };

        if (!string.IsNullOrEmpty(entity.Namespace))
            doc["namespace"] = entity.Namespace;

        if (entity.Properties.Count > 0)
        {
            doc["properties"] = entity.Properties
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(CreatePropertyDocument)
                .ToList();
        }

        if (entity.KeyProperties.Count > 0)
        {
            doc["keyProperties"] = entity.KeyProperties.OrderBy(k => k, StringComparer.Ordinal).ToList();
        }

        if (!string.IsNullOrEmpty(entity.TableName))
            doc["tableName"] = entity.TableName;

        if (!string.IsNullOrEmpty(entity.SchemaName))
            doc["schemaName"] = entity.SchemaName;

        if (entity.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(entity.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreatePropertyDocument(PropertyManifest property)
    {
        var doc = new Dictionary<string, object?>
        {
            ["name"] = property.Name,
            ["typeName"] = property.TypeName
        };

        if (property.IsRequired)
            doc["isRequired"] = true;

        if (property.IsCollection)
            doc["isCollection"] = true;

        if (property.MaxLength.HasValue)
            doc["maxLength"] = property.MaxLength.Value;

        if (property.Precision.HasValue)
            doc["precision"] = property.Precision.Value;

        if (property.Scale.HasValue)
            doc["scale"] = property.Scale.Value;

        if (property.IsConcurrencyToken)
            doc["isConcurrencyToken"] = true;

        if (property.IsComputed)
            doc["isComputed"] = true;

        if (property.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(property.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreateValueObjectDocument(ValueObjectManifest vo)
    {
        var doc = new Dictionary<string, object?>
        {
            ["name"] = vo.Name,
            ["typeName"] = vo.TypeName
        };

        if (!string.IsNullOrEmpty(vo.Namespace))
            doc["namespace"] = vo.Namespace;

        if (vo.Properties.Count > 0)
        {
            doc["properties"] = vo.Properties
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(CreatePropertyDocument)
                .ToList();
        }

        if (vo.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(vo.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreateEnumDocument(EnumManifest enumManifest)
    {
        var doc = new Dictionary<string, object?>
        {
            ["name"] = enumManifest.Name,
            ["typeName"] = enumManifest.TypeName
        };

        if (!string.IsNullOrEmpty(enumManifest.Namespace))
            doc["namespace"] = enumManifest.Namespace;

        if (enumManifest.UnderlyingType != "System.Int32")
            doc["underlyingType"] = enumManifest.UnderlyingType;

        if (enumManifest.Values.Count > 0)
        {
            doc["values"] = enumManifest.Values
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        if (enumManifest.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(enumManifest.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreateRuleSetDocument(RuleSetManifest ruleSet)
    {
        var doc = new Dictionary<string, object?>
        {
            ["name"] = ruleSet.Name,
            ["targetType"] = ruleSet.TargetType
        };

        if (ruleSet.Rules.Count > 0)
        {
            doc["rules"] = ruleSet.Rules
                .OrderBy(r => r.Id, StringComparer.Ordinal)
                .Select(CreateRuleDocument)
                .ToList();
        }

        if (ruleSet.Includes.Count > 0)
        {
            doc["includes"] = ruleSet.Includes.OrderBy(i => i, StringComparer.Ordinal).ToList();
        }

        if (ruleSet.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(ruleSet.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreateRuleDocument(RuleManifest rule)
    {
        var doc = new Dictionary<string, object?>
        {
            ["id"] = rule.Id,
            ["category"] = rule.Category,
            ["targetType"] = rule.TargetType
        };

        if (!string.IsNullOrEmpty(rule.Message))
            doc["message"] = rule.Message;

        if (rule.Severity != RuleSeverity.Error)
            doc["severity"] = rule.Severity.ToString();

        if (rule.Tags.Count > 0)
            doc["tags"] = rule.Tags.OrderBy(t => t, StringComparer.Ordinal).ToList();

        if (!string.IsNullOrEmpty(rule.Expression))
            doc["expression"] = rule.Expression;

        if (rule.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(rule.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreateConfigurationDocument(ConfigurationManifest config)
    {
        var doc = new Dictionary<string, object?>
        {
            ["entityName"] = config.EntityName,
            ["entityTypeName"] = config.EntityTypeName
        };

        if (!string.IsNullOrEmpty(config.TableName))
            doc["tableName"] = config.TableName;

        if (!string.IsNullOrEmpty(config.SchemaName))
            doc["schemaName"] = config.SchemaName;

        if (config.KeyProperties.Count > 0)
            doc["keyProperties"] = config.KeyProperties.OrderBy(k => k, StringComparer.Ordinal).ToList();

        if (config.PropertyConfigurations.Count > 0)
        {
            doc["propertyConfigurations"] = config.PropertyConfigurations
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .ToDictionary(kv => kv.Key, kv => CreatePropertyConfigDocument(kv.Value));
        }

        if (config.Indexes.Count > 0)
        {
            doc["indexes"] = config.Indexes
                .OrderBy(i => i.Name ?? string.Join(",", i.Properties), StringComparer.Ordinal)
                .Select(CreateIndexDocument)
                .ToList();
        }

        if (config.Relationships.Count > 0)
        {
            doc["relationships"] = config.Relationships
                .OrderBy(r => r.PrincipalEntity, StringComparer.Ordinal)
                .ThenBy(r => r.DependentEntity, StringComparer.Ordinal)
                .Select(CreateRelationshipDocument)
                .ToList();
        }

        if (config.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(config.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreatePropertyConfigDocument(PropertyConfigurationManifest propConfig)
    {
        var doc = new Dictionary<string, object?>
        {
            ["propertyName"] = propConfig.PropertyName
        };

        if (!string.IsNullOrEmpty(propConfig.ColumnName))
            doc["columnName"] = propConfig.ColumnName;

        if (!string.IsNullOrEmpty(propConfig.ColumnType))
            doc["columnType"] = propConfig.ColumnType;

        if (propConfig.IsRequired)
            doc["isRequired"] = true;

        if (propConfig.MaxLength.HasValue)
            doc["maxLength"] = propConfig.MaxLength.Value;

        if (propConfig.Precision.HasValue)
            doc["precision"] = propConfig.Precision.Value;

        if (propConfig.Scale.HasValue)
            doc["scale"] = propConfig.Scale.Value;

        if (propConfig.IsConcurrencyToken)
            doc["isConcurrencyToken"] = true;

        if (propConfig.IsUnicode.HasValue)
            doc["isUnicode"] = propConfig.IsUnicode.Value;

        if (!string.IsNullOrEmpty(propConfig.ValueGenerated))
            doc["valueGenerated"] = propConfig.ValueGenerated;

        if (!string.IsNullOrEmpty(propConfig.DefaultValue))
            doc["defaultValue"] = propConfig.DefaultValue;

        if (!string.IsNullOrEmpty(propConfig.DefaultValueSql))
            doc["defaultValueSql"] = propConfig.DefaultValueSql;

        if (!string.IsNullOrEmpty(propConfig.ComputedColumnSql))
            doc["computedColumnSql"] = propConfig.ComputedColumnSql;

        if (propConfig.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(propConfig.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreateIndexDocument(IndexManifest index)
    {
        var doc = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(index.Name))
            doc["name"] = index.Name;

        if (index.Properties.Count > 0)
            doc["properties"] = index.Properties.ToList();

        if (index.IsUnique)
            doc["isUnique"] = true;

        if (!string.IsNullOrEmpty(index.Filter))
            doc["filter"] = index.Filter;

        if (index.IncludedProperties.Count > 0)
            doc["includedProperties"] = index.IncludedProperties.ToList();

        if (index.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(index.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreateRelationshipDocument(RelationshipManifest rel)
    {
        var doc = new Dictionary<string, object?>
        {
            ["principalEntity"] = rel.PrincipalEntity,
            ["dependentEntity"] = rel.DependentEntity,
            ["relationshipType"] = rel.RelationshipType
        };

        if (!string.IsNullOrEmpty(rel.PrincipalNavigation))
            doc["principalNavigation"] = rel.PrincipalNavigation;

        if (!string.IsNullOrEmpty(rel.DependentNavigation))
            doc["dependentNavigation"] = rel.DependentNavigation;

        if (rel.ForeignKeyProperties.Count > 0)
            doc["foreignKeyProperties"] = rel.ForeignKeyProperties.ToList();

        if (rel.IsRequired)
            doc["isRequired"] = true;

        if (!string.IsNullOrEmpty(rel.DeleteBehavior))
            doc["deleteBehavior"] = rel.DeleteBehavior;

        if (!string.IsNullOrEmpty(rel.JoinEntity))
            doc["joinEntity"] = rel.JoinEntity;

        if (rel.Metadata.Count > 0)
            doc["metadata"] = CreateSortedMetadata(rel.Metadata);

        return doc;
    }

    private Dictionary<string, object?> CreateSourceDocument(SourceInfo source)
    {
        var doc = new Dictionary<string, object?>
        {
            ["type"] = source.Type,
            ["location"] = source.Location
        };

        if (source.Timestamp.HasValue)
            doc["timestamp"] = source.Timestamp.Value.ToString("O");

        if (source.Metadata.Count > 0)
        {
            doc["metadata"] = source.Metadata
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .ToDictionary(kv => kv.Key, kv => (object?)kv.Value);
        }

        return doc;
    }

    private static Dictionary<string, object?> CreateSortedMetadata(IReadOnlyDictionary<string, object?> metadata)
    {
        return metadata
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = _options.IndentedJson,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private static string ComputeHash(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = XxHash64.HashToUInt64(bytes);
        return hash.ToString("x16");
    }
}
