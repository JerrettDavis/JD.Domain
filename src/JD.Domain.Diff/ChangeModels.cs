namespace JD.Domain.Diff;

/// <summary>
/// Represents the type of change detected.
/// </summary>
public enum ChangeType
{
    /// <summary>Item was added.</summary>
    Added,
    /// <summary>Item was removed.</summary>
    Removed,
    /// <summary>Item was modified.</summary>
    Modified
}

/// <summary>
/// Base class for all change records.
/// </summary>
public abstract class ChangeRecord
{
    /// <summary>Gets the type of change.</summary>
    public required ChangeType ChangeType { get; init; }

    /// <summary>Gets whether this is a breaking change.</summary>
    public bool IsBreaking { get; init; }

    /// <summary>Gets the description of the change.</summary>
    public required string Description { get; init; }
}

/// <summary>
/// Represents a change to an entity.
/// </summary>
public sealed class EntityChange : ChangeRecord
{
    /// <summary>Gets the entity name.</summary>
    public required string EntityName { get; init; }

    /// <summary>Gets the property changes within this entity.</summary>
    public IReadOnlyList<PropertyChange> PropertyChanges { get; init; } = Array.Empty<PropertyChange>();
}

/// <summary>
/// Represents a change to a property.
/// </summary>
public sealed class PropertyChange : ChangeRecord
{
    /// <summary>Gets the entity name containing the property.</summary>
    public required string EntityName { get; init; }

    /// <summary>Gets the property name.</summary>
    public required string PropertyName { get; init; }

    /// <summary>Gets the old value (for modifications).</summary>
    public string? OldValue { get; init; }

    /// <summary>Gets the new value (for modifications).</summary>
    public string? NewValue { get; init; }
}

/// <summary>
/// Represents a change to a rule set.
/// </summary>
public sealed class RuleSetChange : ChangeRecord
{
    /// <summary>Gets the rule set name.</summary>
    public required string RuleSetName { get; init; }

    /// <summary>Gets the target type.</summary>
    public required string TargetType { get; init; }

    /// <summary>Gets the rule changes within this rule set.</summary>
    public IReadOnlyList<RuleChange> RuleChanges { get; init; } = Array.Empty<RuleChange>();
}

/// <summary>
/// Represents a change to a rule.
/// </summary>
public sealed class RuleChange : ChangeRecord
{
    /// <summary>Gets the rule ID.</summary>
    public required string RuleId { get; init; }

    /// <summary>Gets the rule set name.</summary>
    public required string RuleSetName { get; init; }

    /// <summary>Gets the target type.</summary>
    public required string TargetType { get; init; }
}

/// <summary>
/// Represents a change to configuration.
/// </summary>
public sealed class ConfigurationChange : ChangeRecord
{
    /// <summary>Gets the entity name.</summary>
    public required string EntityName { get; init; }

    /// <summary>Gets the specific configuration aspect that changed.</summary>
    public string? Aspect { get; init; }

    /// <summary>Gets the old value (for modifications).</summary>
    public string? OldValue { get; init; }

    /// <summary>Gets the new value (for modifications).</summary>
    public string? NewValue { get; init; }
}

/// <summary>
/// Represents a change to a value object.
/// </summary>
public sealed class ValueObjectChange : ChangeRecord
{
    /// <summary>Gets the value object name.</summary>
    public required string ValueObjectName { get; init; }

    /// <summary>Gets the property changes within this value object.</summary>
    public IReadOnlyList<PropertyChange> PropertyChanges { get; init; } = Array.Empty<PropertyChange>();
}

/// <summary>
/// Represents a change to an enum.
/// </summary>
public sealed class EnumChange : ChangeRecord
{
    /// <summary>Gets the enum name.</summary>
    public required string EnumName { get; init; }

    /// <summary>Gets the value changes within this enum.</summary>
    public IReadOnlyList<string> ValueChanges { get; init; } = Array.Empty<string>();
}
