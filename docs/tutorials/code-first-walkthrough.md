# Code-First Walkthrough

In this tutorial, you'll build a complete e-commerce domain from scratch using JD.Domain's code-first approach. You'll learn how to define entities, configure properties, add business rules, integrate with EF Core, and generate rich domain types.

**Time:** 45-60 minutes | **Level:** Beginner

## What You'll Build

By the end of this tutorial, you'll have:

- ✅ A complete domain model with Customer and Order entities
- ✅ Business rules with invariants and validators
- ✅ EF Core integration with auto-generated configurations
- ✅ A runtime validation engine
- ✅ Rich domain types with construction safety using `Result<T>`

## Prerequisites

- .NET 10.0 SDK or later
- Basic understanding of C# and Entity Framework Core
- A code editor (Visual Studio, VS Code, or Rider)
- SQL Server LocalDB or another database (optional, can use in-memory)

## Step 1: Create the Project

Create a new console application for our e-commerce domain:

```bash
mkdir JD.Domain.Tutorial.CodeFirst
cd JD.Domain.Tutorial.CodeFirst
dotnet new console
```

## Step 2: Install Required Packages

Install the JD.Domain packages for code-first development:

```bash
# Core packages
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Configuration
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime

# EF Core integration
dotnet add package JD.Domain.EFCore
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design

# Source generator for rich domain types
dotnet add package JD.Domain.DomainModel.Generator
```

## Step 3: Define Domain Entities

Create simple POCO classes for your entities.

Create `Entities/Customer.cs`:

```csharp
namespace JD.Domain.Tutorial.CodeFirst.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
```

Create `Entities/Order.cs`:

```csharp
namespace JD.Domain.Tutorial.CodeFirst.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }

    // Navigation property
    public Customer? Customer { get; set; }
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
```

### Explanation

Notice that our entities are simple POCOs with no base classes or interfaces. This is JD.Domain's opt-in design - you're not forced to inherit from anything or implement specific interfaces.

## Step 4: Define the Domain Model

Now let's describe our domain using JD.Domain's fluent DSL.

Create `Domain/ECommerceDomain.cs`:

```csharp
using JD.Domain.Modeling;
using JD.Domain.Configuration;
using JD.Domain.Tutorial.CodeFirst.Entities;

namespace JD.Domain.Tutorial.CodeFirst.Domain;

public static class ECommerceDomain
{
    public static DomainManifest Create()
    {
        return Domain.Create("ECommerce")
            // Define Customer entity
            .Entity<Customer>(entity => entity
                .Property(c => c.Id)
                .Property(c => c.Name)
                .Property(c => c.Email)
                .Property(c => c.Phone)
                .Property(c => c.CreatedAt)
                .Property(c => c.IsActive))

            // Configure Customer
            .ConfigureEntity<Customer>(config => config
                .HasKey(c => c.Id)
                .ToTable("Customers", "dbo")
                .HasIndex(c => c.Email, idx => idx.IsUnique())
                .Property(c => c.Name, p => p.IsRequired().HasMaxLength(100))
                .Property(c => c.Email, p => p.IsRequired().HasMaxLength(255))
                .Property(c => c.Phone, p => p.HasMaxLength(20)))

            // Define Order entity
            .Entity<Order>(entity => entity
                .Property(o => o.Id)
                .Property(o => o.CustomerId)
                .Property(o => o.OrderNumber)
                .Property(o => o.TotalAmount)
                .Property(o => o.OrderDate)
                .Property(o => o.Status))

            // Configure Order
            .ConfigureEntity<Order>(config => config
                .HasKey(o => o.Id)
                .ToTable("Orders", "dbo")
                .HasIndex(o => o.OrderNumber, idx => idx.IsUnique())
                .HasIndex(o => o.CustomerId)
                .Property(o => o.OrderNumber, p => p.IsRequired().HasMaxLength(50))
                .Property(o => o.TotalAmount, p => p.IsRequired().HasPrecision(18, 2)))

            .Build();
    }
}
```

### Explanation

- **`Domain.Create("ECommerce")`** - Creates a new domain manifest named "ECommerce"
- **`.Entity<T>()`** - Defines an entity type and its properties
- **`.ConfigureEntity<T>()`** - Adds EF Core configuration (keys, indexes, constraints)
- **`.Build()`** - Finalizes and returns the domain manifest

This single definition will be used to generate EF configurations, validators, and more.

## Step 5: Define Business Rules

Create rule sets for validating entities.

Create `Domain/CustomerRules.cs`:

```csharp
using JD.Domain.Rules;
using JD.Domain.Tutorial.CodeFirst.Entities;

namespace JD.Domain.Tutorial.CodeFirst.Domain;

public static class CustomerRules
{
    public static RuleSetManifest Default()
    {
        return new RuleSetBuilder<Customer>("Default")
            // Name is required and has minimum length
            .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
            .WithMessage("Customer name is required")

            .Invariant("Name.MinLength", c => c.Name.Length >= 2)
            .WithMessage("Customer name must be at least 2 characters")

            // Email is required and valid format
            .Invariant("Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("Customer email is required")

            .Invariant("Email.Format", c => c.Email.Contains("@") && c.Email.Contains("."))
            .WithMessage("Customer email must be a valid email address")

            // Phone format (if provided)
            .Invariant("Phone.Format", c => string.IsNullOrEmpty(c.Phone) || c.Phone.Length >= 10)
            .WithMessage("Phone number must be at least 10 digits")

            // Active customers must have been created
            .Invariant("Active.CreatedAt", c => !c.IsActive || c.CreatedAt != default)
            .WithMessage("Active customers must have a creation date")

            .Build();
    }
}
```

Create `Domain/OrderRules.cs`:

```csharp
using JD.Domain.Rules;
using JD.Domain.Tutorial.CodeFirst.Entities;

namespace JD.Domain.Tutorial.CodeFirst.Domain;

public static class OrderRules
{
    public static RuleSetManifest Default()
    {
        return new RuleSetBuilder<Order>("Default")
            // Order number is required and properly formatted
            .Invariant("OrderNumber.Required", o => !string.IsNullOrWhiteSpace(o.OrderNumber))
            .WithMessage("Order number is required")

            .Invariant("OrderNumber.Format", o => o.OrderNumber.StartsWith("ORD-"))
            .WithMessage("Order number must start with 'ORD-'")

            // Customer ID must be positive
            .Invariant("CustomerId.Positive", o => o.CustomerId > 0)
            .WithMessage("Order must be associated with a valid customer")

            // Total amount must be positive
            .Invariant("TotalAmount.Positive", o => o.TotalAmount > 0)
            .WithMessage("Order total must be greater than zero")

            // Order date validations
            .Invariant("OrderDate.NotFuture", o => o.OrderDate <= DateTime.UtcNow)
            .WithMessage("Order date cannot be in the future")

            .Invariant("OrderDate.NotTooOld", o => o.OrderDate >= DateTime.UtcNow.AddYears(-10))
            .WithMessage("Order date cannot be more than 10 years in the past")

            // Status-specific rules
            .Invariant("Status.ValidTransition", o =>
                o.Status == OrderStatus.Pending ||
                o.Status == OrderStatus.Processing ||
                o.Status == OrderStatus.Shipped ||
                o.Status == OrderStatus.Delivered ||
                o.Status == OrderStatus.Cancelled)
            .WithMessage("Order status is invalid")

            .Build();
    }
}
```

### Explanation

- **Invariants** are always-true rules that define valid entity state
- **`.WithMessage()`** provides user-friendly error messages
- Rules are declarative - you describe what should be true, not how to validate
- Rules are reusable across different contexts (API, domain layer, etc.)

## Step 6: Create the DbContext

Create an EF Core DbContext that applies the domain manifest.

Create `Data/ECommerceDbContext.cs`:

```csharp
using JD.Domain.EFCore;
using JD.Domain.Tutorial.CodeFirst.Domain;
using JD.Domain.Tutorial.CodeFirst.Entities;
using Microsoft.EntityFrameworkCore;

namespace JD.Domain.Tutorial.CodeFirst.Data;

public class ECommerceDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply domain manifest - this generates all EF Core configurations
        var manifest = ECommerceDomain.Create();
        modelBuilder.ApplyDomainManifest(manifest);

        // Optional: Add additional EF-specific configurations not in manifest
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### Explanation

The **`ApplyDomainManifest()`** extension method reads your domain manifest and generates:
- Table names and schemas
- Primary keys
- Indexes (unique and non-unique)
- Property constraints (required, max length, precision)

You can still add additional EF-specific configurations manually (like relationships).

## Step 7: Create the Validation Service

Create a service that validates entities using the domain engine.

Create `Services/DomainValidationService.cs`:

```csharp
using JD.Domain.Abstractions;
using JD.Domain.Runtime;
using JD.Domain.Tutorial.CodeFirst.Domain;

namespace JD.Domain.Tutorial.CodeFirst.Services;

public class DomainValidationService
{
    private readonly IDomainEngine _engine;

    public DomainValidationService()
    {
        var manifest = ECommerceDomain.Create();
        _engine = DomainRuntime.CreateEngine(manifest);
    }

    public Result<T> Validate<T>(T entity, RuleSetManifest ruleSet) where T : class
    {
        var result = _engine.Evaluate(entity, ruleSet);

        if (result.IsValid)
        {
            return Result<T>.Success(entity);
        }

        var errors = string.Join("; ", result.Errors.Select(e => e.Message));
        return Result<T>.Failure(new DomainError(
            "ValidationFailed",
            errors,
            RuleSeverity.Error));
    }
}
```

### Explanation

- **`DomainRuntime.CreateEngine()`** creates a rule evaluation engine
- **`engine.Evaluate()`** runs rules against an entity
- **`Result<T>`** is a functional programming pattern that represents success or failure

## Step 8: Test the Domain

Update `Program.cs` to test your domain:

```csharp
using JD.Domain.Tutorial.CodeFirst.Domain;
using JD.Domain.Tutorial.CodeFirst.Entities;
using JD.Domain.Tutorial.CodeFirst.Services;

var validationService = new DomainValidationService();

Console.WriteLine("=== Testing Customer Validation ===\n");

// Test 1: Valid customer
var validCustomer = new Customer
{
    Id = 1,
    Name = "John Doe",
    Email = "john.doe@example.com",
    Phone = "555-123-4567",
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

var result1 = validationService.Validate(validCustomer, CustomerRules.Default());
Console.WriteLine($"Valid Customer: {(result1.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
if (!result1.IsSuccess)
{
    Console.WriteLine($"  Errors: {result1.Error.Message}");
}

// Test 2: Invalid customer (empty name, bad email)
var invalidCustomer = new Customer
{
    Id = 2,
    Name = "",
    Email = "invalid-email",
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

var result2 = validationService.Validate(invalidCustomer, CustomerRules.Default());
Console.WriteLine($"\nInvalid Customer: {(result2.IsSuccess ? "✓ PASSED" : "✗ FAILED (Expected)")}");
if (!result2.IsSuccess)
{
    Console.WriteLine($"  Errors: {result2.Error.Message}");
}

Console.WriteLine("\n=== Testing Order Validation ===\n");

// Test 3: Valid order
var validOrder = new Order
{
    Id = 1,
    CustomerId = 1,
    OrderNumber = "ORD-2025-001",
    TotalAmount = 99.99m,
    OrderDate = DateTime.UtcNow,
    Status = OrderStatus.Pending
};

var result3 = validationService.Validate(validOrder, OrderRules.Default());
Console.WriteLine($"Valid Order: {(result3.IsSuccess ? "✓ PASSED" : "✗ FAILED")}");
if (!result3.IsSuccess)
{
    Console.WriteLine($"  Errors: {result3.Error.Message}");
}

// Test 4: Invalid order (bad order number, negative amount, future date)
var invalidOrder = new Order
{
    Id = 2,
    CustomerId = 0,
    OrderNumber = "INVALID",
    TotalAmount = -50.00m,
    OrderDate = DateTime.UtcNow.AddDays(1),
    Status = OrderStatus.Pending
};

var result4 = validationService.Validate(invalidOrder, OrderRules.Default());
Console.WriteLine($"\nInvalid Order: {(result4.IsSuccess ? "✓ PASSED" : "✗ FAILED (Expected)")}");
if (!result4.IsSuccess)
{
    Console.WriteLine($"  Errors: {result4.Error.Message}");
}
```

Run the application:

```bash
dotnet run
```

### Expected Output

```
=== Testing Customer Validation ===

Valid Customer: ✓ PASSED

Invalid Customer: ✗ FAILED (Expected)
  Errors: Customer name is required; Customer email must be a valid email address

=== Testing Order Validation ===

Valid Order: ✓ PASSED

Invalid Order: ✗ FAILED (Expected)
  Errors: Order number must start with 'ORD-'; Order must be associated with a valid customer; Order total must be greater than zero; Order date cannot be in the future
```

## Step 9: Generate Database Schema (Optional)

If you want to create the database, add migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This will create tables with all the configurations from your domain manifest:
- `Customers` table with unique email index
- `Orders` table with unique order number index
- Proper constraints (required fields, max lengths, precision)

## Step 10: Explore Generated Domain Types

The `JD.Domain.DomainModel.Generator` package automatically generates construction-safe domain types.

Check your `obj/` folder for generated files:

```bash
find obj -name "*DomainModel.g.cs" # Linux/Mac
dir obj /s /b | findstr DomainModel.g.cs # Windows
```

You'll find generated types like `DomainCustomer` and `DomainOrder` with:
- Static `Create()` methods returning `Result<T>`
- `FromEntity()` methods to wrap existing entities
- `With*()` mutation methods
- Automatic rule enforcement

### Using Generated Types

```csharp
// Construction-safe creation
var result = DomainCustomer.Create(
    name: "Jane Doe",
    email: "jane@example.com",
    phone: "555-987-6543");

if (result.IsSuccess)
{
    var customer = result.Value;
    Console.WriteLine($"Created customer: {customer.Name}");
}
else
{
    Console.WriteLine($"Failed: {result.Error.Message}");
}

// Wrap existing entity
var existingCustomer = new Customer { Name = "John", Email = "john@test.com" };
var domainCustomer = DomainCustomer.FromEntity(existingCustomer);
```

## What You've Learned

In this tutorial, you:

✅ Defined entities as simple POCOs without inheritance
✅ Used the fluent DSL to describe your domain model
✅ Added EF Core configurations declaratively
✅ Defined business rules as invariants
✅ Created a runtime validation engine
✅ Validated entities and handled `Result<T>` patterns
✅ Applied domain configurations to EF Core DbContext
✅ Explored auto-generated construction-safe domain types

## Key Concepts

### 1. Single Source of Truth

Your domain manifest is the single source of truth. From it, JD.Domain generates:
- EF Core configurations
- Rich domain types
- FluentValidation validators (if you add that generator)

### 2. Opt-In Architecture

Your entities remain POCOs. No forced inheritance, no marker interfaces. This makes JD.Domain easy to adopt incrementally.

### 3. Declarative Rules

Rules describe *what* should be true, not *how* to validate. This makes them:
- Easy to read and understand
- Reusable across contexts
- Testable in isolation

### 4. Result Pattern

`Result<T>` eliminates exceptions for expected failures (validation errors) while maintaining type safety.

## Next Steps

### Extend Your Domain

- Add more entities (Product, Category, etc.)
- Add value objects (Address, Money, Email)
- Define relationships and navigation properties

### Add More Rules

- Create Validator rules (context-dependent validation)
- Create Policy rules (authorization)
- Add Derivation rules (computed properties)
- Compose rules with `.Include()` and `.When()`

### Integrate with ASP.NET Core

Follow the [ASP.NET Core Integration Tutorial](aspnet-core-integration.md) to add automatic API validation.

### Generate FluentValidation Validators

```bash
dotnet add package JD.Domain.FluentValidation.Generator
```

See [Source Generators Tutorial](source-generators.md) for details.

### Track Domain Evolution

Use snapshots to track changes over time:

```bash
dotnet tool install -g JD.Domain.Cli
jd-domain snapshot --manifest domain.json --output ./snapshots
```

See [Version Management Tutorial](version-management.md) for details.

## Additional Resources

- **[Domain Modeling Tutorial](domain-modeling.md)** - Deep dive into modeling DSL
- **[Business Rules Tutorial](business-rules.md)** - Advanced rule patterns
- **[EF Core Integration](ef-core-integration.md)** - More EF Core features
- **[API Reference](../../api/JD.Domain.Modeling.yml)** - Complete API documentation

## Get Help

- **Questions?** Open a [GitHub Issue](https://github.com/JerrettDavis/JD.Domain/issues)
- **Sample Code** See `samples/JD.Domain.Samples.CodeFirst/` for a complete working example

Congratulations on completing the Code-First walkthrough! You now have a solid foundation for building rich domain models with JD.Domain.
