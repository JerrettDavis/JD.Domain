using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;

namespace ManifestGeneration.Sample;

[DomainValueObject]
public class Address
{
    [Required]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string State { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{5}(-\d{4})?$")]
    [MaxLength(10)]
    public string ZipCode { get; set; } = string.Empty;
}
