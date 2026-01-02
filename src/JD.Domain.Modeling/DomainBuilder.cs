using JD.Domain.Abstractions;

namespace JD.Domain.Modeling;

/// <summary>
/// Fluent builder for constructing a domain manifest.
/// </summary>
public sealed class DomainBuilder
{
    private readonly string _name;
    private Version _version = new(1, 0, 0);
    private readonly List<EntityManifest> _entities = new();
    private readonly List<ValueObjectManifest> _valueObjects = new();
    private readonly List<EnumManifest> _enums = new();
    private readonly List<RuleSetManifest> _ruleSets = new();
    private readonly List<ConfigurationManifest> _configurations = new();
    private readonly List<SourceInfo> _sources = new();
    private readonly Dictionary<string, object?> _metadata = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the domain.</param>
    internal DomainBuilder(string name)
    {
        _name = name;
        _sources.Add(new SourceInfo
        {
            Type = "DSL",
            Location = "Fluent API",
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Sets the version of the domain.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number.</param>
    /// <param name="patch">The patch version number.</param>
    /// <returns>The domain builder for chaining.</returns>
    public DomainBuilder Version(int major, int minor, int patch)
    {
        _version = new Version(major, minor, patch);
        return this;
    }

    /// <summary>
    /// Adds an entity to the domain model.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="configure">The entity configuration action.</param>
    /// <returns>The domain builder for chaining.</returns>
    public DomainBuilder Entity<T>(Action<EntityBuilder<T>>? configure = null) where T : class
    {
        var builder = new EntityBuilder<T>();
        configure?.Invoke(builder);
        
        var manifest = builder.Build();
        _entities.Add(manifest);
        
        return this;
    }

    /// <summary>
    /// Adds a value object to the domain model.
    /// </summary>
    /// <typeparam name="T">The value object type.</typeparam>
    /// <param name="configure">The value object configuration action.</param>
    /// <returns>The domain builder for chaining.</returns>
    public DomainBuilder ValueObject<T>(Action<ValueObjectBuilder<T>>? configure = null) where T : class
    {
        var builder = new ValueObjectBuilder<T>();
        configure?.Invoke(builder);
        
        var manifest = builder.Build();
        _valueObjects.Add(manifest);
        
        return this;
    }

    /// <summary>
    /// Adds an enumeration to the domain model.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <returns>The domain builder for chaining.</returns>
    public DomainBuilder Enum<T>() where T : Enum
    {
        var builder = new EnumBuilder<T>();
        var manifest = builder.Build();
        _enums.Add(manifest);
        
        return this;
    }

    /// <summary>
    /// Adds metadata to the domain.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The domain builder for chaining.</returns>
    public DomainBuilder WithMetadata(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the domain manifest.
    /// </summary>
    /// <returns>The constructed domain manifest.</returns>
    public DomainManifest BuildManifest()
    {
        return new DomainManifest
        {
            Name = _name,
            Version = _version,
            Entities = _entities.AsReadOnly(),
            ValueObjects = _valueObjects.AsReadOnly(),
            Enums = _enums.AsReadOnly(),
            RuleSets = _ruleSets.AsReadOnly(),
            Configurations = _configurations.AsReadOnly(),
            Sources = _sources.AsReadOnly(),
            Metadata = _metadata.AsReadOnly(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Adds a rule set to the domain.
    /// </summary>
    /// <param name="ruleSet">The rule set manifest.</param>
    public void AddRuleSet(RuleSetManifest ruleSet)
    {
        ArgumentNullException.ThrowIfNull(ruleSet);
        _ruleSets.Add(ruleSet);
    }

    /// <summary>
    /// Adds a configuration to the domain.
    /// </summary>
    /// <param name="configuration">The configuration manifest.</param>
    public void AddConfiguration(ConfigurationManifest configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _configurations.Add(configuration);
    }
}
