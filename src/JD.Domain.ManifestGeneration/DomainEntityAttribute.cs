namespace JD.Domain.ManifestGeneration;

/// <summary>
/// Marks a class as a domain entity for automatic manifest generation.
/// The generator will analyze the class structure, properties, data annotations,
/// and relationships to create an entity manifest.
/// </summary>
/// <example>
/// <code>
/// [DomainEntity(TableName = "Customers", Schema = "dbo")]
/// public class Customer
/// {
///     [Key]
///     public int Id { get; set; }
///
///     [Required]
///     [MaxLength(200)]
///     public string Name { get; set; }
///
///     [EmailAddress]
///     public string Email { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DomainEntityAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the database table name for this entity.
    /// If not specified, the class name will be used.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the database schema name for this entity.
    /// If not specified, defaults to the database's default schema.
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets a description of this entity for documentation purposes.
    /// </summary>
    public string? Description { get; set; }
}
