namespace JD.Domain.ManifestGeneration;

/// <summary>
/// Marks a class as a domain value object for automatic manifest generation.
/// Value objects are immutable objects defined by their attributes rather than identity.
/// The generator will analyze the class structure and properties to create a value object manifest.
/// </summary>
/// <example>
/// <code>
/// [DomainValueObject]
/// public class Address
/// {
///     [Required]
///     public string Street { get; set; }
///
///     [Required]
///     [MaxLength(100)]
///     public string City { get; set; }
///
///     [Required]
///     [MaxLength(2)]
///     public string State { get; set; }
///
///     [Required]
///     [RegularExpression(@"^\d{5}(-\d{4})?$")]
///     public string ZipCode { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DomainValueObjectAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a description of this value object for documentation purposes.
    /// </summary>
    public string? Description { get; set; }
}
