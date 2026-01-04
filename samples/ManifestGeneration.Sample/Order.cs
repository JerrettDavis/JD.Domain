using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;

namespace ManifestGeneration.Sample;

[DomainEntity(TableName = "Orders", Schema = "sales")]
public class Order
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    public decimal TotalAmount { get; set; }
}
