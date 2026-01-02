using JD.Domain.Abstractions;

namespace JD.Domain.Runtime;

/// <summary>
/// Provides factory methods for creating domain runtime components.
/// </summary>
public static class DomainRuntime
{
    /// <summary>
    /// Creates a domain engine from the specified manifest.
    /// </summary>
    /// <param name="manifest">The domain manifest.</param>
    /// <returns>A configured domain engine.</returns>
    public static IDomainEngine CreateEngine(DomainManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return new DomainEngine(manifest);
    }

    /// <summary>
    /// Creates a domain engine with configuration.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>A configured domain engine.</returns>
    public static IDomainEngine Create(Action<DomainRuntimeOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        
        var options = new DomainRuntimeOptions();
        configure(options);

        if (options.Manifest == null)
        {
            throw new InvalidOperationException("Domain manifest must be configured.");
        }

        return new DomainEngine(options.Manifest);
    }
}

/// <summary>
/// Options for configuring the domain runtime.
/// </summary>
public sealed class DomainRuntimeOptions
{
    /// <summary>
    /// Gets or sets the domain manifest.
    /// </summary>
    public DomainManifest? Manifest { get; set; }

    /// <summary>
    /// Adds a manifest to the runtime.
    /// </summary>
    /// <param name="manifest">The domain manifest.</param>
    /// <returns>The options for chaining.</returns>
    public DomainRuntimeOptions AddManifest(DomainManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        Manifest = manifest;
        return this;
    }
}
