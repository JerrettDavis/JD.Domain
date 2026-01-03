using JD.Domain.Rules;

namespace JD.Domain.Samples.CodeFirst;

/// <summary>
/// Demonstrates code-first workflow: define domain using JD DSL, then generate EF configs.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== JD.Domain Code-First Sample ===\n");

        // Step 1: Define domain model using fluent DSL
        Console.WriteLine("1. Defining domain model...");
        var domain = Modeling.Domain.Create("ECommerce")
            .Entity<Customer>()
            .Entity<Order>()
            .Entity<Product>()
            .Build();

        Console.WriteLine($"   Created domain: {domain.Name} v{domain.Version}");
        Console.WriteLine($"   Entities: {domain.Entities.Count}");

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
    }
}

// Domain entities
public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public List<Order> Orders { get; set; } = new();
}

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

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

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
