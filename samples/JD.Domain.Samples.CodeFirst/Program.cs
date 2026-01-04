using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;
using JD.Domain.Rules;

namespace JD.Domain.Samples.CodeFirst;

/// <summary>
/// Demonstrates code-first workflow with automatic manifest generation.
/// NO manual string writing required - all metadata extracted from entity classes!
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== JD.Domain Code-First Sample (Automatic Manifest Generation) ===\n");

        // Step 1: Access auto-generated manifest
        Console.WriteLine("1. Using auto-generated domain manifest...");
        var manifest = ECommerceManifest.GeneratedManifest;

        Console.WriteLine($"   Domain: {manifest.Name} v{manifest.Version}");
        Console.WriteLine($"   Entities: {manifest.Entities.Count}");
        Console.WriteLine($"   Source: {manifest.Sources[0].Type}");
        Console.WriteLine($"   NO MANUAL STRING WRITING REQUIRED!");

        // Step 2: Define business rules
        Console.WriteLine("\n2. Defining business rules...");
        var customerRules = new RuleSetBuilder<Customer>("Default")
            .Invariant("Customer.Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
            .WithMessage("Customer name cannot be empty")
            .Invariant("Customer.Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("Customer email cannot be empty")
            .BuildCompiled();

        var orderRules = new RuleSetBuilder<Order>("Default")
            .Invariant("Order.Items.Required", o => o.Items.Count > 0)
            .WithMessage("An order must contain at least one item")
            .BuildCompiled();

        Console.WriteLine($"   Customer rules: {customerRules.Rules.Count}");
        Console.WriteLine($"   Order rules: {orderRules.Rules.Count}");

        // Step 3: Validate sample data using compiled rules
        Console.WriteLine("\n3. Validating sample data...");

        // Valid customer
        var validCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com"
        };

        var validResult = customerRules.Evaluate(validCustomer);
        Console.WriteLine($"   Valid customer: {(validResult.IsValid ? "PASSED" : "FAILED")}");

        // Invalid customer (empty name triggers rule)
        var invalidCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "",
            Email = "invalid-email"
        };

        var invalidResult = customerRules.Evaluate(invalidCustomer);
        Console.WriteLine($"   Invalid customer: {(invalidResult.IsValid ? "PASSED" : "FAILED")}");
        foreach (var error in invalidResult.Errors)
        {
            Console.WriteLine($"      - {error.Message}");
        }

        Console.WriteLine("\n=== Sample Complete ===");
        Console.WriteLine("Manifest was generated automatically from entity attributes!");
    }
}

// Domain entities with automatic manifest generation
[DomainEntity]
public class Customer
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(500)]
    public string Email { get; set; } = "";

    [ExcludeFromManifest]
    public List<Order> Orders { get; set; } = new();
}

[DomainEntity]
public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [ExcludeFromManifest]
    public Customer Customer { get; set; } = null!;

    [ExcludeFromManifest]
    public List<OrderItem> Items { get; set; } = new();
}

[DomainEntity]
public class Product
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
}

// OrderItem is not included in manifest (no [DomainEntity] attribute)
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
