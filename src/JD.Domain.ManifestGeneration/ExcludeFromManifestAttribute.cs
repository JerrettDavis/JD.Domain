namespace JD.Domain.ManifestGeneration;

/// <summary>
/// Excludes a class or property from automatic manifest generation.
/// Use this to opt-out specific types or properties that should not be included
/// in the generated domain manifest.
/// </summary>
/// <example>
/// <code>
/// [DomainEntity]
/// public class Customer
/// {
///     [Key]
///     public int Id { get; set; }
///
///     [Required]
///     public string Name { get; set; }
///
///     // This property will be excluded from the manifest
///     [ExcludeFromManifest]
///     public DateTime InternalTimestamp { get; set; }
///
///     [ExcludeFromManifest]
///     public byte[] InternalData { get; set; }
/// }
///
/// // This entire class will be excluded from manifest generation
/// [ExcludeFromManifest]
/// public class InternalAuditLog
/// {
///     public int Id { get; set; }
///     public string Details { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ExcludeFromManifestAttribute : Attribute
{
}
