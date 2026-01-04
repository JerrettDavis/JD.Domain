using JD.Domain.Abstractions;
using JD.Domain.Snapshot;

namespace JD.Domain.Diff;

/// <summary>
/// Engine for comparing domain snapshots and detecting changes.
/// </summary>
public sealed class DiffEngine
{
    private readonly BreakingChangeClassifier _classifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffEngine"/> class.
    /// </summary>
    public DiffEngine()
    {
        _classifier = new BreakingChangeClassifier();
    }

    /// <summary>
    /// Compares two snapshots and returns the differences.
    /// </summary>
    /// <param name="before">The snapshot before changes.</param>
    /// <param name="after">The snapshot after changes.</param>
    /// <returns>The diff result.</returns>
    public DomainDiff Compare(DomainSnapshot before, DomainSnapshot after)
    {
        if (before == null) throw new ArgumentNullException(nameof(before));
        if (after == null) throw new ArgumentNullException(nameof(after));

        var entityChanges = CompareEntities(before.Manifest.Entities, after.Manifest.Entities);
        var valueObjectChanges = CompareValueObjects(before.Manifest.ValueObjects, after.Manifest.ValueObjects);
        var enumChanges = CompareEnums(before.Manifest.Enums, after.Manifest.Enums);
        var ruleSetChanges = CompareRuleSets(before.Manifest.RuleSets, after.Manifest.RuleSets);
        var configChanges = CompareConfigurations(before.Manifest.Configurations, after.Manifest.Configurations);

        var allChanges = new List<ChangeRecord>();
        allChanges.AddRange(entityChanges);
        allChanges.AddRange(valueObjectChanges);
        allChanges.AddRange(enumChanges);
        allChanges.AddRange(ruleSetChanges);
        allChanges.AddRange(configChanges);

        var breakingDescriptions = allChanges
            .Where(c => c.IsBreaking)
            .Select(c => c.Description)
            .ToList();

        return new DomainDiff
        {
            Before = before,
            After = after,
            EntityChanges = entityChanges,
            ValueObjectChanges = valueObjectChanges,
            EnumChanges = enumChanges,
            RuleSetChanges = ruleSetChanges,
            ConfigurationChanges = configChanges,
            HasBreakingChanges = breakingDescriptions.Count > 0,
            BreakingChangeDescriptions = breakingDescriptions
        };
    }

    private List<EntityChange> CompareEntities(
        IReadOnlyList<EntityManifest> before,
        IReadOnlyList<EntityManifest> after)
    {
        var changes = new List<EntityChange>();
        var beforeDict = before.ToDictionary(e => e.Name, StringComparer.Ordinal);
        var afterDict = after.ToDictionary(e => e.Name, StringComparer.Ordinal);

        // Find removed entities
        foreach (var entity in before)
        {
            if (!afterDict.ContainsKey(entity.Name))
            {
                changes.Add(new EntityChange
                {
                    ChangeType = ChangeType.Removed,
                    EntityName = entity.Name,
                    Description = $"Entity '{entity.Name}' removed",
                    IsBreaking = _classifier.IsEntityRemovalBreaking()
                });
            }
        }

        // Find added entities
        foreach (var entity in after)
        {
            if (!beforeDict.ContainsKey(entity.Name))
            {
                changes.Add(new EntityChange
                {
                    ChangeType = ChangeType.Added,
                    EntityName = entity.Name,
                    Description = $"Entity '{entity.Name}' added",
                    IsBreaking = false
                });
            }
        }

        // Find modified entities
        foreach (var entity in after)
        {
            if (beforeDict.TryGetValue(entity.Name, out var beforeEntity))
            {
                var propertyChanges = CompareProperties(entity.Name, beforeEntity.Properties, entity.Properties);

                if (propertyChanges.Count > 0 || !KeysEqual(beforeEntity.KeyProperties, entity.KeyProperties))
                {
                    var keyChanged = !KeysEqual(beforeEntity.KeyProperties, entity.KeyProperties);
                    var isBreaking = keyChanged || propertyChanges.Any(p => p.IsBreaking);

                    changes.Add(new EntityChange
                    {
                        ChangeType = ChangeType.Modified,
                        EntityName = entity.Name,
                        Description = $"Entity '{entity.Name}' modified",
                        IsBreaking = isBreaking,
                        PropertyChanges = propertyChanges
                    });
                }
            }
        }

        return changes;
    }

    private List<PropertyChange> CompareProperties(
        string entityName,
        IReadOnlyList<PropertyManifest> before,
        IReadOnlyList<PropertyManifest> after)
    {
        var changes = new List<PropertyChange>();
        var beforeDict = before.ToDictionary(p => p.Name, StringComparer.Ordinal);
        var afterDict = after.ToDictionary(p => p.Name, StringComparer.Ordinal);

        // Find removed properties
        foreach (var prop in before)
        {
            if (!afterDict.ContainsKey(prop.Name))
            {
                changes.Add(new PropertyChange
                {
                    ChangeType = ChangeType.Removed,
                    EntityName = entityName,
                    PropertyName = prop.Name,
                    Description = $"Property '{entityName}.{prop.Name}' removed",
                    IsBreaking = _classifier.IsPropertyRemovalBreaking()
                });
            }
        }

        // Find added properties
        foreach (var prop in after)
        {
            if (!beforeDict.ContainsKey(prop.Name))
            {
                var isBreaking = _classifier.IsPropertyAdditionBreaking(prop.IsRequired);
                changes.Add(new PropertyChange
                {
                    ChangeType = ChangeType.Added,
                    EntityName = entityName,
                    PropertyName = prop.Name,
                    Description = $"Property '{entityName}.{prop.Name}' added" +
                        (prop.IsRequired ? " (required)" : ""),
                    IsBreaking = isBreaking
                });
            }
        }

        // Find modified properties
        foreach (var prop in after)
        {
            if (beforeDict.TryGetValue(prop.Name, out var beforeProp))
            {
                if (prop.TypeName != beforeProp.TypeName)
                {
                    changes.Add(new PropertyChange
                    {
                        ChangeType = ChangeType.Modified,
                        EntityName = entityName,
                        PropertyName = prop.Name,
                        OldValue = beforeProp.TypeName,
                        NewValue = prop.TypeName,
                        Description = $"Property '{entityName}.{prop.Name}' type changed from '{beforeProp.TypeName}' to '{prop.TypeName}'",
                        IsBreaking = _classifier.IsPropertyTypeChangeBreaking()
                    });
                }
                else if (prop.IsRequired != beforeProp.IsRequired)
                {
                    var isBreaking = _classifier.IsRequiredChangeBreaking(!beforeProp.IsRequired, prop.IsRequired);
                    changes.Add(new PropertyChange
                    {
                        ChangeType = ChangeType.Modified,
                        EntityName = entityName,
                        PropertyName = prop.Name,
                        OldValue = beforeProp.IsRequired ? "required" : "optional",
                        NewValue = prop.IsRequired ? "required" : "optional",
                        Description = $"Property '{entityName}.{prop.Name}' changed from {(beforeProp.IsRequired ? "required" : "optional")} to {(prop.IsRequired ? "required" : "optional")}",
                        IsBreaking = isBreaking
                    });
                }
            }
        }

        return changes;
    }

    private List<ValueObjectChange> CompareValueObjects(
        IReadOnlyList<ValueObjectManifest> before,
        IReadOnlyList<ValueObjectManifest> after)
    {
        var changes = new List<ValueObjectChange>();
        var beforeDict = before.ToDictionary(v => v.Name, StringComparer.Ordinal);
        var afterDict = after.ToDictionary(v => v.Name, StringComparer.Ordinal);

        foreach (var vo in before)
        {
            if (!afterDict.ContainsKey(vo.Name))
            {
                changes.Add(new ValueObjectChange
                {
                    ChangeType = ChangeType.Removed,
                    ValueObjectName = vo.Name,
                    Description = $"Value object '{vo.Name}' removed",
                    IsBreaking = true
                });
            }
        }

        foreach (var vo in after)
        {
            if (!beforeDict.ContainsKey(vo.Name))
            {
                changes.Add(new ValueObjectChange
                {
                    ChangeType = ChangeType.Added,
                    ValueObjectName = vo.Name,
                    Description = $"Value object '{vo.Name}' added",
                    IsBreaking = false
                });
            }
        }

        return changes;
    }

    private List<EnumChange> CompareEnums(
        IReadOnlyList<EnumManifest> before,
        IReadOnlyList<EnumManifest> after)
    {
        var changes = new List<EnumChange>();
        var beforeDict = before.ToDictionary(e => e.Name, StringComparer.Ordinal);
        var afterDict = after.ToDictionary(e => e.Name, StringComparer.Ordinal);

        foreach (var e in before)
        {
            if (!afterDict.ContainsKey(e.Name))
            {
                changes.Add(new EnumChange
                {
                    ChangeType = ChangeType.Removed,
                    EnumName = e.Name,
                    Description = $"Enum '{e.Name}' removed",
                    IsBreaking = true
                });
            }
        }

        foreach (var e in after)
        {
            if (!beforeDict.ContainsKey(e.Name))
            {
                changes.Add(new EnumChange
                {
                    ChangeType = ChangeType.Added,
                    EnumName = e.Name,
                    Description = $"Enum '{e.Name}' added",
                    IsBreaking = false
                });
            }
        }

        return changes;
    }

    private List<RuleSetChange> CompareRuleSets(
        IReadOnlyList<RuleSetManifest> before,
        IReadOnlyList<RuleSetManifest> after)
    {
        var changes = new List<RuleSetChange>();
        var beforeDict = before.ToDictionary(r => $"{r.Name}:{r.TargetType}", StringComparer.Ordinal);
        var afterDict = after.ToDictionary(r => $"{r.Name}:{r.TargetType}", StringComparer.Ordinal);

        foreach (var rs in before)
        {
            var key = $"{rs.Name}:{rs.TargetType}";
            if (!afterDict.ContainsKey(key))
            {
                changes.Add(new RuleSetChange
                {
                    ChangeType = ChangeType.Removed,
                    RuleSetName = rs.Name,
                    TargetType = rs.TargetType,
                    Description = $"Rule set '{rs.Name}' for '{rs.TargetType}' removed",
                    IsBreaking = false
                });
            }
        }

        foreach (var rs in after)
        {
            var key = $"{rs.Name}:{rs.TargetType}";
            if (!beforeDict.ContainsKey(key))
            {
                changes.Add(new RuleSetChange
                {
                    ChangeType = ChangeType.Added,
                    RuleSetName = rs.Name,
                    TargetType = rs.TargetType,
                    Description = $"Rule set '{rs.Name}' for '{rs.TargetType}' added",
                    IsBreaking = false
                });
            }
        }

        return changes;
    }

    private List<ConfigurationChange> CompareConfigurations(
        IReadOnlyList<ConfigurationManifest> before,
        IReadOnlyList<ConfigurationManifest> after)
    {
        var changes = new List<ConfigurationChange>();
        var beforeDict = before.ToDictionary(c => c.EntityName, StringComparer.Ordinal);
        var afterDict = after.ToDictionary(c => c.EntityName, StringComparer.Ordinal);

        foreach (var config in before)
        {
            if (!afterDict.ContainsKey(config.EntityName))
            {
                changes.Add(new ConfigurationChange
                {
                    ChangeType = ChangeType.Removed,
                    EntityName = config.EntityName,
                    Description = $"Configuration for '{config.EntityName}' removed",
                    IsBreaking = false
                });
            }
        }

        foreach (var config in after)
        {
            if (!beforeDict.ContainsKey(config.EntityName))
            {
                changes.Add(new ConfigurationChange
                {
                    ChangeType = ChangeType.Added,
                    EntityName = config.EntityName,
                    Description = $"Configuration for '{config.EntityName}' added",
                    IsBreaking = false
                });
            }
            else if (beforeDict.TryGetValue(config.EntityName, out var beforeConfig))
            {
                if (config.TableName != beforeConfig.TableName)
                {
                    changes.Add(new ConfigurationChange
                    {
                        ChangeType = ChangeType.Modified,
                        EntityName = config.EntityName,
                        Aspect = "TableName",
                        OldValue = beforeConfig.TableName,
                        NewValue = config.TableName,
                        Description = $"Table name for '{config.EntityName}' changed from '{beforeConfig.TableName}' to '{config.TableName}'",
                        IsBreaking = false
                    });
                }
            }
        }

        return changes;
    }

    private static bool KeysEqual(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        if (a.Count != b.Count) return false;
        return a.OrderBy(k => k).SequenceEqual(b.OrderBy(k => k));
    }
}
