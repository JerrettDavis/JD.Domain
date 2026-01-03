# Quick Start

This guide walks you through creating your first domain model with JD.Domain in 5 minutes. By the end, you'll have a working domain model with business rules and runtime validation.

## What You'll Build

A simple e-commerce domain with:
- A `Customer` entity with validation rules
- An `Order` entity with business rules
- Runtime validation that enforces invariants
- Type-safe construction with `Result<T>`

## Step 1: Create a New Project

Create a new console application:

```bash
dotnet new console -n JD.Domain.QuickStart
cd JD.Domain.QuickStart
```

## Step 2: Install Required Packages

Install the core JD.Domain packages:

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
```

## Step 3: Define Your Domain Entities

Create simple POCO classes for your entities. Create `Entities/Customer.cs`:

```csharp
namespace JD.Domain.QuickStart.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

Create `Entities/Order.cs`:

```csharp
namespace JD.Domain.QuickStart.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
}
```

## Step 4: Define Domain Model

Create a domain model that describes your entities. Create `DomainModel.cs`:

```csharp
using JD.Domain.Modeling;
using JD.Domain.QuickStart.Entities;

namespace JD.Domain.QuickStart;

public static class DomainModel
{
    public static DomainManifest Create()
    {
        return Domain.Create("ECommerce")
            .Entity<Customer>(e => e
                .Property(c => c.Id)
                .Property(c => c.Name)
                .Property(c => c.Email)
                .Property(c => c.CreatedAt))
            .Entity<Order>(e => e
                .Property(o => o.Id)
                .Property(o => o.CustomerId)
                .Property(o => o.TotalAmount)
                .Property(o => o.OrderDate))
            .Build();
    }
}
```

## Step 5: Define Business Rules

Create rule sets for your entities. Create `Rules/CustomerRules.cs`:

```csharp
using JD.Domain.Rules;
using JD.Domain.QuickStart.Entities;

namespace JD.Domain.QuickStart.Rules;

public static class CustomerRules
{
    public static RuleSetManifest Default()
    {
        return new RuleSetBuilder<Customer>("Default")
            // Name is required
            .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
            .WithMessage("Customer name cannot be empty")

            // Name length constraint
            .Invariant("Name.Length", c => c.Name.Length <= 100)
            .WithMessage("Customer name cannot exceed 100 characters")

            // Email is required
            .Invariant("Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("Customer email cannot be empty")

            // Email format validation
            .Invariant("Email.Format", c => c.Email.Contains("@") && c.Email.Contains("."))
            .WithMessage("Customer email must be valid")

            .Build();
    }
}
```

Create `Rules/OrderRules.cs`:

```csharp
using JD.Domain.Rules;
using JD.Domain.QuickStart.Entities;

namespace JD.Domain.QuickStart.Rules;

public static class OrderRules
{
    public static RuleSetManifest Default()
    {
        return new RuleSetBuilder<Order>("Default")
            // Customer ID must be positive
            .Invariant("CustomerId.Positive", o => o.CustomerId > 0)
            .WithMessage("Order must be associated with a valid customer")

            // Total amount must be positive
            .Invariant("TotalAmount.Positive", o => o.TotalAmount > 0)
            .WithMessage("Order total must be greater than zero")

            // Order date cannot be in the future
            .Invariant("OrderDate.NotFuture", o => o.OrderDate <= DateTime.UtcNow)
            .WithMessage("Order date cannot be in the future")

            .Build();
    }
}
```

## Step 6: Validate Entities at Runtime

Update `Program.cs` to create and validate entities:

```csharp
using JD.Domain.Runtime;
using JD.Domain.QuickStart;
using JD.Domain.QuickStart.Entities;
using JD.Domain.QuickStart.Rules;

// Create domain manifest
var domain = DomainModel.Create();

// Create domain engine
var engine = DomainRuntime.CreateEngine(domain);

// Create a valid customer
var validCustomer = new Customer
{
    Id = 1,
    Name = "John Doe",
    Email = "john.doe@example.com",
    CreatedAt = DateTime.UtcNow
};

// Validate the customer
var customerResult = engine.Evaluate(validCustomer, CustomerRules.Default());

if (customerResult.IsValid)
{
    Console.WriteLine("✓ Customer is valid");
}
else
{
    Console.WriteLine("✗ Customer validation failed:");
    foreach (var error in customerResult.Errors)
    {
        Console.WriteLine($"  - {error.Message}");
    }
}

// Create an invalid customer (empty name and invalid email)
var invalidCustomer = new Customer
{
    Id = 2,
    Name = "",
    Email = "invalid-email",
    CreatedAt = DateTime.UtcNow
};

// Validate the invalid customer
var invalidResult = engine.Evaluate(invalidCustomer, CustomerRules.Default());

if (!invalidResult.IsValid)
{
    Console.WriteLine("\n✗ Invalid customer detected:");
    foreach (var error in invalidResult.Errors)
    {
        Console.WriteLine($"  - {error.Message}");
    }
}

// Create a valid order
var validOrder = new Order
{
    Id = 1,
    CustomerId = 1,
    TotalAmount = 99.99m,
    OrderDate = DateTime.UtcNow
};

// Validate the order
var orderResult = engine.Evaluate(validOrder, OrderRules.Default());

if (orderResult.IsValid)
{
    Console.WriteLine("\n✓ Order is valid");
}

// Create an invalid order (negative amount, future date)
var invalidOrder = new Order
{
    Id = 2,
    CustomerId = 0,
    TotalAmount = -50.00m,
    OrderDate = DateTime.UtcNow.AddDays(1)
};

// Validate the invalid order
var invalidOrderResult = engine.Evaluate(invalidOrder, OrderRules.Default());

if (!invalidOrderResult.IsValid)
{
    Console.WriteLine("\n✗ Invalid order detected:");
    foreach (var error in invalidOrderResult.Errors)
    {
        Console.WriteLine($"  - {error.Message}");
    }
}
```

## Step 7: Run the Application

Build and run your application:

```bash
dotnet run
```

You should see output like:

```
✓ Customer is valid

✗ Invalid customer detected:
  - Customer name cannot be empty
  - Customer email must be valid

✓ Order is valid

✗ Invalid order detected:
  - Order must be associated with a valid customer
  - Order total must be greater than zero
  - Order date cannot be in the future
```

## What You Just Built

Congratulations! You've created:

1. **Domain entities** - Simple POCO classes with no special requirements
2. **Domain manifest** - Metadata describing your entities
3. **Business rules** - Declarative rules that validate entity state
4. **Runtime validation** - Automatic validation using the domain engine

## Key Concepts Demonstrated

### 1. Opt-In Design

Notice how your `Customer` and `Order` classes don't inherit from any base class or implement any interface. JD.Domain is completely opt-in.

### 2. Declarative Rules

Rules are defined declaratively using lambda expressions:

```csharp
.Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
.WithMessage("Customer name cannot be empty")
```

This is more maintainable than scattering validation logic throughout your codebase.

### 3. Separation of Concerns

Your entities remain pure POCOs, while rules are defined separately. This makes it easy to:
- Test entities without validation logic
- Reuse entities across different contexts
- Version rules independently from entities

### 4. Explicit Validation

Validation is explicit - you call `engine.Evaluate()` when you want to validate. This gives you full control over when validation happens.

## Next Steps

Now that you've built your first domain model, explore more features:

### Add EF Core Integration

Apply domain configurations to Entity Framework Core:

```bash
dotnet add package JD.Domain.EFCore
```

See the [EF Core Integration Tutorial](../tutorials/ef-core-integration.md) for details.

### Generate Rich Domain Types

Create construction-safe types that enforce invariants:

```bash
dotnet add package JD.Domain.DomainModel.Generator
```

See the [Domain Model Generator Tutorial](../tutorials/source-generators.md) for details.

### Add ASP.NET Core Validation

Automatically validate API requests:

```bash
dotnet add package JD.Domain.AspNetCore
```

See the [ASP.NET Core Integration Tutorial](../tutorials/aspnet-core-integration.md) for details.

### Explore More Examples

- **[Code-First Walkthrough](../tutorials/code-first-walkthrough.md)** - Complete code-first workflow
- **[Database-First Walkthrough](../tutorials/db-first-walkthrough.md)** - Add rules to existing EF entities
- **[Hybrid Workflow](../tutorials/hybrid-workflow.md)** - Mix both approaches with snapshots

## Common Questions

### How do I validate nested entities?

Use the `Validator` rule type for context-dependent validation:

```csharp
.Validator("Address.Valid", c => ValidateAddress(c.Address))
.WithMessage("Customer address is invalid")
```

### Can I use async rules?

Yes! The runtime engine supports async evaluation:

```csharp
var result = await engine.EvaluateAsync(customer, rules);
```

### How do I conditionally apply rules?

Use the `When` condition:

```csharp
.Invariant("PremiumEmail.Required", c => !string.IsNullOrWhiteSpace(c.Email))
.When(c => c.IsPremiumCustomer)
.WithMessage("Premium customers must have an email")
```

### Can I compose multiple rule sets?

Yes! Use the `Include` method:

```csharp
.Include(BaseCustomerRules.Default())
.Invariant("Additional.Rule", c => /* ... */)
```

## Troubleshooting

### Rules Not Firing

Ensure you're calling `engine.Evaluate()` with the correct rule set:

```csharp
var result = engine.Evaluate(customer, CustomerRules.Default());
```

### Build Errors

Make sure you've installed all required packages:

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
```

### Performance Concerns

For high-performance scenarios, consider:
- Caching the domain manifest and engine
- Using specific rule sets instead of evaluating all rules
- Implementing async rules for I/O-bound validation

See [Performance Optimization](../advanced/performance.md) for details.

## Summary

In this quick start, you learned:
- How to define domain entities as POCOs
- How to create a domain manifest
- How to define business rules declaratively
- How to validate entities at runtime
- Key concepts like opt-in design and separation of concerns

Continue your journey with the [Choose Your Workflow](choose-workflow.md) guide to decide which approach fits your project best.
