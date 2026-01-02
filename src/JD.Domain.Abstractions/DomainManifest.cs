namespace JD.Domain.Abstractions;

/// <summary>
/// Represents a complete domain model manifest including entities, rules, and configurations.
/// This is the central data structure that captures all domain modeling information.
/// </summary>
public sealed class DomainManifest
{
    /// <summary>
    /// Gets the name of the domain.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the version of the domain model.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    /// Gets the hash of the manifest for change detection.
    /// </summary>
    public string? Hash { get; init; }

    /// <summary>
    /// Gets the entities in the domain model.
    /// </summary>
    public IReadOnlyList<EntityManifest> Entities { get; init; } = 
        Array.Empty<EntityManifest>();

    /// <summary>
    /// Gets the value objects in the domain model.
    /// </summary>
    public IReadOnlyList<ValueObjectManifest> ValueObjects { get; init; } = 
        Array.Empty<ValueObjectManifest>();

    /// <summary>
    /// Gets the enumerations in the domain model.
    /// </summary>
    public IReadOnlyList<EnumManifest> Enums { get; init; } = 
        Array.Empty<EnumManifest>();

    /// <summary>
    /// Gets the rule sets defined for the domain.
    /// </summary>
    public IReadOnlyList<RuleSetManifest> RuleSets { get; init; } = 
        Array.Empty<RuleSetManifest>();

    /// <summary>
    /// Gets the entity configurations for the domain.
    /// </summary>
    public IReadOnlyList<ConfigurationManifest> Configurations { get; init; } = 
        Array.Empty<ConfigurationManifest>();

    /// <summary>
    /// Gets the source information describing where the domain model came from.
    /// </summary>
    public IReadOnlyList<SourceInfo> Sources { get; init; } = 
        Array.Empty<SourceInfo>();

    /// <summary>
    /// Gets additional metadata about the domain.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();

    /// <summary>
    /// Gets the timestamp when this manifest was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
