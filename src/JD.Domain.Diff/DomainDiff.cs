using System;
using System.Collections.Generic;
using JD.Domain.Snapshot;

namespace JD.Domain.Diff;

/// <summary>
/// Represents the difference between two domain snapshots.
/// </summary>
public sealed class DomainDiff
{
    /// <summary>Gets the snapshot before changes.</summary>
    public required DomainSnapshot Before { get; init; }

    /// <summary>Gets the snapshot after changes.</summary>
    public required DomainSnapshot After { get; init; }

    /// <summary>Gets the entity changes.</summary>
    public IReadOnlyList<EntityChange> EntityChanges { get; init; } = Array.Empty<EntityChange>();

    /// <summary>Gets the value object changes.</summary>
    public IReadOnlyList<ValueObjectChange> ValueObjectChanges { get; init; } = Array.Empty<ValueObjectChange>();

    /// <summary>Gets the enum changes.</summary>
    public IReadOnlyList<EnumChange> EnumChanges { get; init; } = Array.Empty<EnumChange>();

    /// <summary>Gets the rule set changes.</summary>
    public IReadOnlyList<RuleSetChange> RuleSetChanges { get; init; } = Array.Empty<RuleSetChange>();

    /// <summary>Gets the configuration changes.</summary>
    public IReadOnlyList<ConfigurationChange> ConfigurationChanges { get; init; } = Array.Empty<ConfigurationChange>();

    /// <summary>Gets whether there are any breaking changes.</summary>
    public bool HasBreakingChanges { get; init; }

    /// <summary>Gets descriptions of all breaking changes.</summary>
    public IReadOnlyList<string> BreakingChangeDescriptions { get; init; } = Array.Empty<string>();

    /// <summary>Gets whether there are any changes at all.</summary>
    public bool HasChanges =>
        EntityChanges.Count > 0 ||
        ValueObjectChanges.Count > 0 ||
        EnumChanges.Count > 0 ||
        RuleSetChanges.Count > 0 ||
        ConfigurationChanges.Count > 0;

    /// <summary>Gets the total number of changes.</summary>
    public int TotalChanges =>
        EntityChanges.Count +
        ValueObjectChanges.Count +
        EnumChanges.Count +
        RuleSetChanges.Count +
        ConfigurationChanges.Count;
}
