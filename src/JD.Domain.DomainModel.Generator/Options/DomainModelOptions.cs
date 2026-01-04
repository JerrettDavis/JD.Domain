using System.Collections.Generic;

namespace JD.Domain.DomainModel.Generator.Options;

/// <summary>
/// Configuration options for the domain model generator.
/// </summary>
public sealed class DomainModelOptions
{
    /// <summary>
    /// Gets or sets the namespace for generated domain types.
    /// If not specified, uses the manifest name with ".Domain" suffix.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the prefix for generated domain type names.
    /// Default is "Domain" (e.g., Blog -> DomainBlog).
    /// </summary>
    public string DomainTypePrefix { get; set; } = "Domain";

    /// <summary>
    /// Gets or sets whether to generate With* mutation methods.
    /// Default is true.
    /// </summary>
    public bool GenerateWithMethods { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate partial class declarations
    /// for user customization (e.g., semantic methods).
    /// Default is true.
    /// </summary>
    public bool GeneratePartialClasses { get; set; } = true;

    /// <summary>
    /// Gets or sets when property validation occurs.
    /// </summary>
    public PropertyValidationMode ValidationMode { get; set; } = PropertyValidationMode.OnSet;

    /// <summary>
    /// Gets or sets the rule set name used for Create() factory method validation.
    /// Default is "Create".
    /// </summary>
    public string CreateRuleSet { get; set; } = "Create";

    /// <summary>
    /// Gets or sets the default rule set name used when no specific rule set is requested.
    /// Default is "Default".
    /// </summary>
    public string DefaultRuleSet { get; set; } = "Default";

    /// <summary>
    /// Gets or sets whether to generate implicit conversion operators to the EF entity type.
    /// Default is true.
    /// </summary>
    public bool GenerateImplicitConversion { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include DomainContext parameter in factory and mutation methods.
    /// Default is true.
    /// </summary>
    public bool IncludeDomainContext { get; set; } = true;

    /// <summary>
    /// Gets or sets additional using directives to include in generated files.
    /// </summary>
    public IList<string> AdditionalUsings { get; set; } = new List<string>();
}
