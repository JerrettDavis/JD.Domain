using System;
using System.Linq;
using System.Text;

namespace JD.Domain.Diff;

/// <summary>
/// Generates migration plans from domain diffs.
/// </summary>
public sealed class MigrationPlanGenerator
{
    /// <summary>
    /// Generates a migration plan from a diff.
    /// </summary>
    /// <param name="diff">The diff to generate a plan from.</param>
    /// <returns>Markdown migration plan.</returns>
    public string Generate(DomainDiff diff)
    {
        if (diff == null) throw new ArgumentNullException(nameof(diff));

        var sb = new StringBuilder();

        sb.AppendLine($"# Migration Plan: {diff.Before.Name} {diff.Before.Version} → {diff.After.Version}");
        sb.AppendLine();
        sb.AppendLine($"Generated: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        if (!diff.HasChanges)
        {
            sb.AppendLine("No changes detected. No migration required.");
            return sb.ToString();
        }

        // Summary
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"- **Total Changes**: {diff.TotalChanges}");
        sb.AppendLine($"- **Breaking Changes**: {diff.BreakingChangeDescriptions.Count}");
        sb.AppendLine($"- **Entity Changes**: {diff.EntityChanges.Count}");
        sb.AppendLine($"- **Configuration Changes**: {diff.ConfigurationChanges.Count}");
        sb.AppendLine($"- **Rule Changes**: {diff.RuleSetChanges.Count}");
        sb.AppendLine();

        // Breaking changes
        if (diff.HasBreakingChanges)
        {
            sb.AppendLine("## ⚠️ Breaking Changes");
            sb.AppendLine();
            sb.AppendLine("The following changes may require data migration or code updates:");
            sb.AppendLine();
            foreach (var desc in diff.BreakingChangeDescriptions)
            {
                sb.AppendLine($"- {desc}");
            }
            sb.AppendLine();
        }

        // Non-breaking changes
        var nonBreakingChanges = diff.EntityChanges.Where(c => !c.IsBreaking)
            .Cast<ChangeRecord>()
            .Concat(diff.ValueObjectChanges.Where(c => !c.IsBreaking))
            .Concat(diff.EnumChanges.Where(c => !c.IsBreaking))
            .Concat(diff.RuleSetChanges.Where(c => !c.IsBreaking))
            .Concat(diff.ConfigurationChanges.Where(c => !c.IsBreaking))
            .ToList();

        if (nonBreakingChanges.Count > 0)
        {
            sb.AppendLine("## ✅ Non-Breaking Changes");
            sb.AppendLine();
            foreach (var change in nonBreakingChanges)
            {
                sb.AppendLine($"- {change.Description}");
            }
            sb.AppendLine();
        }

        // Recommended actions
        sb.AppendLine("## Recommended Actions");
        sb.AppendLine();

        var actionIndex = 1;

        // Database schema changes
        var schemaChanges = diff.EntityChanges
            .Where(e => e.ChangeType == ChangeType.Removed ||
                       e.PropertyChanges.Any(p => p.ChangeType == ChangeType.Removed || p.IsBreaking))
            .ToList();

        if (schemaChanges.Count > 0)
        {
            sb.AppendLine($"{actionIndex}. **Database Schema Migration**");
            sb.AppendLine();
            sb.AppendLine("   Create an EF Core migration to update the database schema:");
            sb.AppendLine();
            sb.AppendLine("   ```bash");
            sb.AppendLine($"   dotnet ef migrations add {diff.Before.Name}_V{diff.After.Version.ToString().Replace(".", "_")}");
            sb.AppendLine("   ```");
            sb.AppendLine();

            foreach (var entity in schemaChanges)
            {
                if (entity.ChangeType == ChangeType.Removed)
                {
                    sb.AppendLine($"   - Drop table for `{entity.EntityName}`");
                }
                else
                {
                    foreach (var prop in entity.PropertyChanges.Where(p => p.ChangeType == ChangeType.Removed))
                    {
                        sb.AppendLine($"   - Drop column `{prop.PropertyName}` from `{entity.EntityName}`");
                    }
                    foreach (var prop in entity.PropertyChanges.Where(p => p.ChangeType == ChangeType.Modified && p.IsBreaking))
                    {
                        sb.AppendLine($"   - Alter column `{prop.PropertyName}` in `{entity.EntityName}`");
                    }
                }
            }
            sb.AppendLine();
            actionIndex++;
        }

        // Data migration
        var dataChanges = diff.EntityChanges
            .SelectMany(e => e.PropertyChanges)
            .Where(p => p.ChangeType == ChangeType.Modified &&
                       (p.OldValue?.Contains("?") == true && p.NewValue?.Contains("?") == false))
            .ToList();

        if (dataChanges.Count > 0)
        {
            sb.AppendLine($"{actionIndex}. **Data Migration**");
            sb.AppendLine();
            sb.AppendLine("   The following properties changed from nullable to non-nullable. Migrate existing null values:");
            sb.AppendLine();
            foreach (var prop in dataChanges)
            {
                sb.AppendLine($"   - `{prop.EntityName}.{prop.PropertyName}`: Set default value for existing null records");
            }
            sb.AppendLine();
            actionIndex++;
        }

        // New required properties
        var newRequiredProps = diff.EntityChanges
            .SelectMany(e => e.PropertyChanges)
            .Where(p => p.ChangeType == ChangeType.Added && p.IsBreaking)
            .ToList();

        if (newRequiredProps.Count > 0)
        {
            sb.AppendLine($"{actionIndex}. **Handle New Required Properties**");
            sb.AppendLine();
            sb.AppendLine("   The following required properties were added. Provide default values:");
            sb.AppendLine();
            foreach (var prop in newRequiredProps)
            {
                sb.AppendLine($"   - `{prop.EntityName}.{prop.PropertyName}`");
            }
            sb.AppendLine();
            actionIndex++;
        }

        // Code updates
        if (diff.HasBreakingChanges)
        {
            sb.AppendLine($"{actionIndex}. **Code Updates**");
            sb.AppendLine();
            sb.AppendLine("   Update application code to handle the breaking changes:");
            sb.AppendLine();

            foreach (var entity in diff.EntityChanges.Where(e => e.ChangeType == ChangeType.Removed))
            {
                sb.AppendLine($"   - Remove references to `{entity.EntityName}`");
            }

            foreach (var prop in diff.EntityChanges.SelectMany(e => e.PropertyChanges).Where(p => p.ChangeType == ChangeType.Removed))
            {
                sb.AppendLine($"   - Remove references to `{prop.EntityName}.{prop.PropertyName}`");
            }
            sb.AppendLine();
            actionIndex++;
        }

        // Testing
        sb.AppendLine($"{actionIndex}. **Testing**");
        sb.AppendLine();
        sb.AppendLine("   After applying changes:");
        sb.AppendLine();
        sb.AppendLine("   - Run all unit tests");
        sb.AppendLine("   - Run integration tests");
        sb.AppendLine("   - Verify domain validation rules still work correctly");
        sb.AppendLine("   - Test any affected API endpoints");
        sb.AppendLine();

        // Footer
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"*Generated by JD.Domain.Diff*");

        return sb.ToString();
    }
}
