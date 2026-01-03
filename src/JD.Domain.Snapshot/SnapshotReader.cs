using System.Text.Json;
using JD.Domain.Abstractions;

namespace JD.Domain.Snapshot;

/// <summary>
/// Reads domain snapshots from JSON format.
/// </summary>
public sealed class SnapshotReader
{
    /// <summary>
    /// Deserializes a snapshot from JSON.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized snapshot.</returns>
    public DomainSnapshot Deserialize(string json)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentException("JSON cannot be null or empty.", nameof(json));

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var name = root.GetProperty("name").GetString()!;
        var version = Version.Parse(root.GetProperty("version").GetString()!);
        var hash = root.GetProperty("hash").GetString()!;
        var createdAt = DateTimeOffset.Parse(root.GetProperty("createdAt").GetString()!);
        var manifest = ReadManifest(root.GetProperty("manifest"));

        return new DomainSnapshot
        {
            Name = name,
            Version = version,
            Hash = hash,
            CreatedAt = createdAt,
            Manifest = manifest
        };
    }

    /// <summary>
    /// Deserializes a manifest directly from JSON.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized manifest.</returns>
    public DomainManifest DeserializeManifest(string json)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentException("JSON cannot be null or empty.", nameof(json));

        using var doc = JsonDocument.Parse(json);
        return ReadManifest(doc.RootElement);
    }

    private DomainManifest ReadManifest(JsonElement element)
    {
        return new DomainManifest
        {
            Name = element.GetProperty("name").GetString()!,
            Version = Version.Parse(element.GetProperty("version").GetString()!),
            Hash = element.TryGetProperty("hash", out var hashEl) ? hashEl.GetString() : null,
            CreatedAt = DateTimeOffset.Parse(element.GetProperty("createdAt").GetString()!),
            Entities = ReadArray(element, "entities", ReadEntity),
            ValueObjects = ReadArray(element, "valueObjects", ReadValueObject),
            Enums = ReadArray(element, "enums", ReadEnum),
            RuleSets = ReadArray(element, "ruleSets", ReadRuleSet),
            Configurations = ReadArray(element, "configurations", ReadConfiguration),
            Sources = ReadArray(element, "sources", ReadSource),
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private EntityManifest ReadEntity(JsonElement element)
    {
        return new EntityManifest
        {
            Name = element.GetProperty("name").GetString()!,
            TypeName = element.GetProperty("typeName").GetString()!,
            Namespace = element.TryGetProperty("namespace", out var ns) ? ns.GetString() : null,
            Properties = ReadArray(element, "properties", ReadProperty),
            KeyProperties = ReadStringArray(element, "keyProperties"),
            TableName = element.TryGetProperty("tableName", out var tn) ? tn.GetString() : null,
            SchemaName = element.TryGetProperty("schemaName", out var sn) ? sn.GetString() : null,
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private PropertyManifest ReadProperty(JsonElement element)
    {
        return new PropertyManifest
        {
            Name = element.GetProperty("name").GetString()!,
            TypeName = element.GetProperty("typeName").GetString()!,
            IsRequired = element.TryGetProperty("isRequired", out var ir) && ir.GetBoolean(),
            IsCollection = element.TryGetProperty("isCollection", out var ic) && ic.GetBoolean(),
            MaxLength = element.TryGetProperty("maxLength", out var ml) ? ml.GetInt32() : null,
            Precision = element.TryGetProperty("precision", out var pr) ? pr.GetInt32() : null,
            Scale = element.TryGetProperty("scale", out var sc) ? sc.GetInt32() : null,
            IsConcurrencyToken = element.TryGetProperty("isConcurrencyToken", out var ct) && ct.GetBoolean(),
            IsComputed = element.TryGetProperty("isComputed", out var comp) && comp.GetBoolean(),
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private ValueObjectManifest ReadValueObject(JsonElement element)
    {
        return new ValueObjectManifest
        {
            Name = element.GetProperty("name").GetString()!,
            TypeName = element.GetProperty("typeName").GetString()!,
            Namespace = element.TryGetProperty("namespace", out var ns) ? ns.GetString() : null,
            Properties = ReadArray(element, "properties", ReadProperty),
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private EnumManifest ReadEnum(JsonElement element)
    {
        var values = new Dictionary<string, object>();
        if (element.TryGetProperty("values", out var valuesEl))
        {
            foreach (var prop in valuesEl.EnumerateObject())
            {
                values[prop.Name] = prop.Value.ValueKind == JsonValueKind.Number
                    ? prop.Value.GetInt32()
                    : prop.Value.GetString()!;
            }
        }

        return new EnumManifest
        {
            Name = element.GetProperty("name").GetString()!,
            TypeName = element.GetProperty("typeName").GetString()!,
            Namespace = element.TryGetProperty("namespace", out var ns) ? ns.GetString() : null,
            UnderlyingType = element.TryGetProperty("underlyingType", out var ut) ? ut.GetString()! : "System.Int32",
            Values = values,
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private RuleSetManifest ReadRuleSet(JsonElement element)
    {
        return new RuleSetManifest
        {
            Name = element.GetProperty("name").GetString()!,
            TargetType = element.GetProperty("targetType").GetString()!,
            Rules = ReadArray(element, "rules", ReadRule),
            Includes = ReadStringArray(element, "includes"),
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private RuleManifest ReadRule(JsonElement element)
    {
        var severityStr = element.TryGetProperty("severity", out var sev) ? sev.GetString() : "Error";
        var severity = Enum.TryParse<RuleSeverity>(severityStr, out var s) ? s : RuleSeverity.Error;

        return new RuleManifest
        {
            Id = element.GetProperty("id").GetString()!,
            Category = element.GetProperty("category").GetString()!,
            TargetType = element.GetProperty("targetType").GetString()!,
            Message = element.TryGetProperty("message", out var msg) ? msg.GetString() : null,
            Severity = severity,
            Tags = ReadStringArray(element, "tags"),
            Expression = element.TryGetProperty("expression", out var expr) ? expr.GetString() : null,
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private ConfigurationManifest ReadConfiguration(JsonElement element)
    {
        var propConfigs = new Dictionary<string, PropertyConfigurationManifest>();
        if (element.TryGetProperty("propertyConfigurations", out var pcEl))
        {
            foreach (var prop in pcEl.EnumerateObject())
            {
                propConfigs[prop.Name] = ReadPropertyConfig(prop.Value);
            }
        }

        return new ConfigurationManifest
        {
            EntityName = element.GetProperty("entityName").GetString()!,
            EntityTypeName = element.GetProperty("entityTypeName").GetString()!,
            TableName = element.TryGetProperty("tableName", out var tn) ? tn.GetString() : null,
            SchemaName = element.TryGetProperty("schemaName", out var sn) ? sn.GetString() : null,
            KeyProperties = ReadStringArray(element, "keyProperties"),
            PropertyConfigurations = propConfigs,
            Indexes = ReadArray(element, "indexes", ReadIndex),
            Relationships = ReadArray(element, "relationships", ReadRelationship),
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private PropertyConfigurationManifest ReadPropertyConfig(JsonElement element)
    {
        return new PropertyConfigurationManifest
        {
            PropertyName = element.GetProperty("propertyName").GetString()!,
            ColumnName = element.TryGetProperty("columnName", out var cn) ? cn.GetString() : null,
            ColumnType = element.TryGetProperty("columnType", out var ct) ? ct.GetString() : null,
            IsRequired = element.TryGetProperty("isRequired", out var ir) && ir.GetBoolean(),
            MaxLength = element.TryGetProperty("maxLength", out var ml) ? ml.GetInt32() : null,
            Precision = element.TryGetProperty("precision", out var pr) ? pr.GetInt32() : null,
            Scale = element.TryGetProperty("scale", out var sc) ? sc.GetInt32() : null,
            IsConcurrencyToken = element.TryGetProperty("isConcurrencyToken", out var cct) && cct.GetBoolean(),
            IsUnicode = element.TryGetProperty("isUnicode", out var iu) ? iu.GetBoolean() : null,
            ValueGenerated = element.TryGetProperty("valueGenerated", out var vg) ? vg.GetString() : null,
            DefaultValue = element.TryGetProperty("defaultValue", out var dv) ? dv.GetString() : null,
            DefaultValueSql = element.TryGetProperty("defaultValueSql", out var dvs) ? dvs.GetString() : null,
            ComputedColumnSql = element.TryGetProperty("computedColumnSql", out var ccs) ? ccs.GetString() : null,
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private IndexManifest ReadIndex(JsonElement element)
    {
        return new IndexManifest
        {
            Name = element.TryGetProperty("name", out var n) ? n.GetString() : null,
            Properties = ReadStringArray(element, "properties"),
            IsUnique = element.TryGetProperty("isUnique", out var iu) && iu.GetBoolean(),
            Filter = element.TryGetProperty("filter", out var f) ? f.GetString() : null,
            IncludedProperties = ReadStringArray(element, "includedProperties"),
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private RelationshipManifest ReadRelationship(JsonElement element)
    {
        return new RelationshipManifest
        {
            PrincipalEntity = element.GetProperty("principalEntity").GetString()!,
            DependentEntity = element.GetProperty("dependentEntity").GetString()!,
            RelationshipType = element.GetProperty("relationshipType").GetString()!,
            PrincipalNavigation = element.TryGetProperty("principalNavigation", out var pn) ? pn.GetString() : null,
            DependentNavigation = element.TryGetProperty("dependentNavigation", out var dn) ? dn.GetString() : null,
            ForeignKeyProperties = ReadStringArray(element, "foreignKeyProperties"),
            IsRequired = element.TryGetProperty("isRequired", out var ir) && ir.GetBoolean(),
            DeleteBehavior = element.TryGetProperty("deleteBehavior", out var db) ? db.GetString() : null,
            JoinEntity = element.TryGetProperty("joinEntity", out var je) ? je.GetString() : null,
            Metadata = ReadMetadata(element, "metadata")
        };
    }

    private SourceInfo ReadSource(JsonElement element)
    {
        var metadata = new Dictionary<string, string>();
        if (element.TryGetProperty("metadata", out var metaEl))
        {
            foreach (var prop in metaEl.EnumerateObject())
            {
                metadata[prop.Name] = prop.Value.GetString()!;
            }
        }

        return new SourceInfo
        {
            Type = element.GetProperty("type").GetString()!,
            Location = element.GetProperty("location").GetString()!,
            Timestamp = element.TryGetProperty("timestamp", out var ts) ? DateTimeOffset.Parse(ts.GetString()!) : null,
            Metadata = metadata
        };
    }

    private static IReadOnlyList<T> ReadArray<T>(JsonElement parent, string propertyName, Func<JsonElement, T> reader)
    {
        if (!parent.TryGetProperty(propertyName, out var arrayEl))
            return Array.Empty<T>();

        return arrayEl.EnumerateArray().Select(reader).ToList();
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var arrayEl))
            return Array.Empty<string>();

        return arrayEl.EnumerateArray().Select(e => e.GetString()!).ToList();
    }

    private static IReadOnlyDictionary<string, object?> ReadMetadata(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var metaEl))
            return new Dictionary<string, object?>();

        var result = new Dictionary<string, object?>();
        foreach (var prop in metaEl.EnumerateObject())
        {
            result[prop.Name] = ReadJsonValue(prop.Value);
        }
        return result;
    }

    private static object? ReadJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ReadJsonValue).ToList(),
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => ReadJsonValue(p.Value)),
            _ => element.GetRawText()
        };
    }
}
