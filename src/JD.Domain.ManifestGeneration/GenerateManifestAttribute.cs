namespace JD.Domain.ManifestGeneration;

/// <summary>
/// Marks a DbContext class or assembly for automatic manifest generation.
/// When applied to a DbContext, the generator will analyze the EF Core model configuration.
/// When applied at assembly level, the generator will scan for all marked entity types.
/// </summary>
/// <example>
/// <code>
/// // Apply to DbContext
/// [GenerateManifest("Blogging", Version = "1.0.0")]
/// public class BloggingContext : DbContext
/// {
///     public DbSet&lt;Blog&gt; Blogs { get; set; }
///     protected override void OnModelCreating(ModelBuilder modelBuilder) { ... }
/// }
///
/// // Apply at assembly level
/// [assembly: GenerateManifest("ECommerce", Version = "1.0.0")]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class GenerateManifestAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the domain manifest.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the version of the domain manifest (e.g., "1.0.0").
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the output path for the generated manifest JSON file (optional).
    /// If specified, the generator will emit both C# code and a JSON snapshot file.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the namespace for the generated manifest class.
    /// If not specified, defaults to "JD.Domain.Generated".
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateManifestAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the domain manifest.</param>
    public GenerateManifestAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Manifest name cannot be null or whitespace.", nameof(name));
        }

        Name = name;
    }
}
