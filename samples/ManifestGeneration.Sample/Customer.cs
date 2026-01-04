using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;

namespace ManifestGeneration.Sample;

[DomainEntity(TableName = "Customers", Schema = "dbo")]
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Email { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    [ExcludeFromManifest]
    public DateTime InternalTimestamp { get; set; }
}
