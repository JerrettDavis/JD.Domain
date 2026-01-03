using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JD.Domain.Diff;

/// <summary>
/// Formats diff results as Markdown or JSON.
/// </summary>
public sealed class DiffFormatter
{
    /// <summary>
    /// Formats a diff as Markdown.
    /// </summary>
    /// <param name="diff">The diff to format.</param>
    /// <returns>Markdown string.</returns>
    public string FormatAsMarkdown(DomainDiff diff)
    {
        if (diff == null) throw new ArgumentNullException(nameof(diff));

        var sb = new StringBuilder();

        sb.AppendLine($"# Domain Diff: {diff.Before.Name}");
        sb.AppendLine();
        sb.AppendLine($"**Version**: {diff.Before.Version} → {diff.After.Version}");
        sb.AppendLine($"**Total Changes**: {diff.TotalChanges}");
        sb.AppendLine($"**Breaking Changes**: {(diff.HasBreakingChanges ? "Yes" : "No")}");
        sb.AppendLine();

        if (diff.HasBreakingChanges)
        {
            sb.AppendLine("## ⚠️ Breaking Changes");
            sb.AppendLine();
            foreach (var desc in diff.BreakingChangeDescriptions)
            {
                sb.AppendLine($"- {desc}");
            }
            sb.AppendLine();
        }

        if (diff.EntityChanges.Count > 0)
        {
            sb.AppendLine("## Entity Changes");
            sb.AppendLine();
            foreach (var change in diff.EntityChanges)
            {
                var icon = GetChangeIcon(change.ChangeType, change.IsBreaking);
                sb.AppendLine($"- {icon} {change.Description}");

                foreach (var propChange in change.PropertyChanges)
                {
                    var propIcon = GetChangeIcon(propChange.ChangeType, propChange.IsBreaking);
                    sb.AppendLine($"  - {propIcon} {propChange.Description}");
                }
            }
            sb.AppendLine();
        }

        if (diff.ValueObjectChanges.Count > 0)
        {
            sb.AppendLine("## Value Object Changes");
            sb.AppendLine();
            foreach (var change in diff.ValueObjectChanges)
            {
                var icon = GetChangeIcon(change.ChangeType, change.IsBreaking);
                sb.AppendLine($"- {icon} {change.Description}");
            }
            sb.AppendLine();
        }

        if (diff.EnumChanges.Count > 0)
        {
            sb.AppendLine("## Enum Changes");
            sb.AppendLine();
            foreach (var change in diff.EnumChanges)
            {
                var icon = GetChangeIcon(change.ChangeType, change.IsBreaking);
                sb.AppendLine($"- {icon} {change.Description}");
            }
            sb.AppendLine();
        }

        if (diff.RuleSetChanges.Count > 0)
        {
            sb.AppendLine("## Rule Set Changes");
            sb.AppendLine();
            foreach (var change in diff.RuleSetChanges)
            {
                var icon = GetChangeIcon(change.ChangeType, change.IsBreaking);
                sb.AppendLine($"- {icon} {change.Description}");
            }
            sb.AppendLine();
        }

        if (diff.ConfigurationChanges.Count > 0)
        {
            sb.AppendLine("## Configuration Changes");
            sb.AppendLine();
            foreach (var change in diff.ConfigurationChanges)
            {
                var icon = GetChangeIcon(change.ChangeType, change.IsBreaking);
                sb.AppendLine($"- {icon} {change.Description}");
            }
            sb.AppendLine();
        }

        if (!diff.HasChanges)
        {
            sb.AppendLine("No changes detected.");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a diff as JSON.
    /// </summary>
    /// <param name="diff">The diff to format.</param>
    /// <param name="indented">Whether to indent the JSON.</param>
    /// <returns>JSON string.</returns>
    public string FormatAsJson(DomainDiff diff, bool indented = true)
    {
        if (diff == null) throw new ArgumentNullException(nameof(diff));

        var summary = new
        {
            domain = diff.Before.Name,
            beforeVersion = diff.Before.Version.ToString(),
            afterVersion = diff.After.Version.ToString(),
            beforeHash = diff.Before.Hash,
            afterHash = diff.After.Hash,
            totalChanges = diff.TotalChanges,
            hasBreakingChanges = diff.HasBreakingChanges,
            breakingChangeDescriptions = diff.BreakingChangeDescriptions,
            entityChanges = diff.EntityChanges.Select(FormatEntityChange),
            valueObjectChanges = diff.ValueObjectChanges.Select(FormatValueObjectChange),
            enumChanges = diff.EnumChanges.Select(FormatEnumChange),
            ruleSetChanges = diff.RuleSetChanges.Select(FormatRuleSetChange),
            configurationChanges = diff.ConfigurationChanges.Select(FormatConfigChange)
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(summary, options);
    }

    private static string GetChangeIcon(ChangeType changeType, bool isBreaking)
    {
        if (isBreaking) return "⚠️";

        return changeType switch
        {
            ChangeType.Added => "✅",
            ChangeType.Removed => "❌",
            ChangeType.Modified => "📝",
            _ => "•"
        };
    }

    private static object FormatEntityChange(EntityChange change) => new
    {
        changeType = change.ChangeType.ToString(),
        entityName = change.EntityName,
        description = change.Description,
        isBreaking = change.IsBreaking,
        propertyChanges = change.PropertyChanges.Select(FormatPropertyChange)
    };

    private static object FormatPropertyChange(PropertyChange change) => new
    {
        changeType = change.ChangeType.ToString(),
        entityName = change.EntityName,
        propertyName = change.PropertyName,
        description = change.Description,
        isBreaking = change.IsBreaking,
        oldValue = change.OldValue,
        newValue = change.NewValue
    };

    private static object FormatValueObjectChange(ValueObjectChange change) => new
    {
        changeType = change.ChangeType.ToString(),
        valueObjectName = change.ValueObjectName,
        description = change.Description,
        isBreaking = change.IsBreaking
    };

    private static object FormatEnumChange(EnumChange change) => new
    {
        changeType = change.ChangeType.ToString(),
        enumName = change.EnumName,
        description = change.Description,
        isBreaking = change.IsBreaking
    };

    private static object FormatRuleSetChange(RuleSetChange change) => new
    {
        changeType = change.ChangeType.ToString(),
        ruleSetName = change.RuleSetName,
        targetType = change.TargetType,
        description = change.Description,
        isBreaking = change.IsBreaking
    };

    private static object FormatConfigChange(ConfigurationChange change) => new
    {
        changeType = change.ChangeType.ToString(),
        entityName = change.EntityName,
        aspect = change.Aspect,
        description = change.Description,
        isBreaking = change.IsBreaking,
        oldValue = change.OldValue,
        newValue = change.NewValue
    };
}
