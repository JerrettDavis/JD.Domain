# JD.Domain

A production-ready, opt-in domain modeling suite for .NET that brings rich domain models, business rules, and configuration to any codebase—whether database-first, code-first, or hybrid.

[![Build Status](https://github.com/JerrettDavis/JD.Domain/workflows/CI%2FCD/badge.svg)](https://github.com/JerrettDavis/JD.Domain/actions)
[![codecov](https://codecov.io/gh/JerrettDavis/JD.Domain/branch/main/graph/badge.svg)](https://codecov.io/gh/JerrettDavis/JD.Domain)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

---

## Why JD.Domain?

Traditional EF Core models often end up anemic—plain data bags with validation scattered across controllers, services, or separate validators. JD.Domain changes that by providing:

- **Rich Domain Models**: Embed invariants, validators, and policies directly in your domain
- **Opt-In Architecture**: Adopt incrementally without forced dependencies or framework lock-in
- **Database-First Friendly**: Works seamlessly with reverse-engineered EF Core entities
- **Code Generation**: Generate FluentValidation validators, rich domain types, and more
- **Version Management**: Track domain evolution with snapshots and detect breaking changes

## Quick Start

### Installation

Install the core packages:

```bash
# Automatic manifest generation
dotnet add package JD.Domain.ManifestGeneration
dotnet add package JD.Domain.ManifestGeneration.Generator

# Core runtime and rules
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime

# EF Core integration
dotnet add package JD.Domain.EFCore

# ASP.NET Core integration (optional)
dotnet add package JD.Domain.AspNetCore
```

### Define Your Domain

Add attributes to your entity classes for **automatic manifest generation**:

```csharp
using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;
using JD.Domain.Rules;

// Configure manifest generation
[assembly: GenerateManifest("ECommerce", Version = "1.0.0")]

// Define entities with attributes
[DomainEntity]
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
}

[DomainEntity]
public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public decimal Total { get; set; }
}

// Build generates ECommerceManifest.GeneratedManifest automatically
// NO manual string writing required!

// Define business rules
var customerRules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
        .WithMessage("Customer name is required")
    .Invariant("Email.Format", c => c.Email.Contains("@"))
        .WithMessage("Email must be valid")
    .Build();

// Use auto-generated manifest and evaluate rules at runtime
using JD.Domain.Generated;
var runtime = DomainRuntime.CreateEngine(ECommerceManifest.GeneratedManifest, customerRules);
var customer = new Customer { Name = "", Email = "invalid" };
var result = await runtime.EvaluateAsync(customer);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.PropertyName}: {error.Message}");
}
```

### Configure EF Core

```csharp
using JD.Domain.EFCore;
using JD.Domain.Generated;

public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply auto-generated domain configuration to EF Core
        modelBuilder.ApplyDomainManifest(ECommerceManifest.GeneratedManifest);

        base.OnModelCreating(modelBuilder);
    }
}
```

### ASP.NET Core Integration

```csharp
using JD.Domain.AspNetCore;
using JD.Domain.Generated;

var builder = WebApplication.CreateBuilder(args);

// Register domain validation with auto-generated manifest
builder.Services.AddDomainValidation(options =>
{
    options.AddManifest(ECommerceManifest.GeneratedManifest);
});

var app = builder.Build();

// Use domain validation middleware
app.UseDomainValidation();

// Validate in minimal API endpoints
app.MapPost("/customers", async (Customer customer, IDomainEngine engine) =>
{
    var result = await engine.EvaluateAsync(customer);
    if (!result.IsValid)
        return Results.ValidationProblem(result.ToValidationErrors());

    // Save customer...
    return Results.Created($"/customers/{customer.Id}", customer);
});
```

## Key Features

### 🎯 Three Workflows

**Code-First**: Define your domain with the fluent DSL and generate EF Core configurations.

**Database-First**: Reverse-engineer entities from an existing database and layer on business rules.

**Hybrid**: Mix code-first and database-first approaches while tracking evolution with snapshots.

[See workflow guide →](docs/getting-started/choose-workflow.md)

### 📐 Rich Business Rules

Define four types of rules:

- **Invariants**: Always-true constraints (e.g., "Email is required")
- **Validators**: Context-dependent validation (e.g., "Email format is valid")
- **Policies**: Authorization and business policies (e.g., "User can approve orders")
- **Derivations**: Computed properties (e.g., "Total = Quantity × Price")

[Learn about rules →](docs/tutorials/business-rules.md)

### 🔄 Source Generators

Generate code from your domain manifest:

- **FluentValidation Validators**: Convert JD rules to FluentValidation automatically
- **Rich Domain Types**: Construction-safe domain models with `Result<T>` and property validation

[Explore generators →](docs/tutorials/source-generators.md)

### 📸 Version Management

Track domain evolution with snapshots:

```bash
# Create a snapshot of your domain
jd-domain snapshot --manifest domain.json --output snapshots/

# Compare versions to detect changes
jd-domain diff snapshots/v1.json snapshots/v2.json --format md

# Generate migration plans
jd-domain migrate-plan snapshots/v1.json snapshots/v2.json
```

[Version management guide →](docs/tutorials/version-management.md)

## Packages

JD.Domain is organized into focused, composable packages:

| Package | Purpose |
|---------|---------|
| **JD.Domain.Abstractions** | Core contracts and the DomainManifest model |
| **JD.Domain.ManifestGeneration** ⭐ | Attributes for automatic manifest generation |
| **JD.Domain.ManifestGeneration.Generator** ⭐ | Roslyn source generator for manifests (NO manual strings!) |
| **JD.Domain.Modeling** | Fluent DSL for domain modeling (alternative approach) |
| **JD.Domain.Configuration** | EF Core-compatible configuration DSL |
| **JD.Domain.Rules** | Business rules (invariants, validators, policies) |
| **JD.Domain.Runtime** | Rule evaluation engine |
| **JD.Domain.EFCore** | Entity Framework Core integration |
| **JD.Domain.AspNetCore** | ASP.NET Core middleware and filters |
| **JD.Domain.Validation** | Validation contracts for web APIs |
| **JD.Domain.Generators.Core** | Base infrastructure for code generators |
| **JD.Domain.DomainModel.Generator** | Generate rich domain types |
| **JD.Domain.FluentValidation.Generator** | Generate FluentValidation validators |
| **JD.Domain.Snapshot** | Domain snapshot serialization |
| **JD.Domain.Diff** | Snapshot comparison and breaking change detection |
| **JD.Domain.Cli** | Command-line tools (`jd-domain`) |
| **JD.Domain.T4.Shims** | T4 template integration |

[See package matrix →](docs/reference/package-matrix.md)

## Sample Applications

Explore complete working examples:

- **[Manifest Generation](samples/ManifestGeneration.Sample)** ⭐: Automatic manifest generation from entity classes (NO manual strings!)
- **[CodeFirst](samples/JD.Domain.Samples.CodeFirst)**: Define domain with DSL, generate EF configs
- **[DbFirst](samples/JD.Domain.Samples.DbFirst)**: Add rules to reverse-engineered entities
- **[Hybrid](samples/JD.Domain.Samples.Hybrid)**: Mix approaches with snapshot versioning

## Documentation

- **[Getting Started](docs/getting-started/index.md)**: Installation, quick start, and workflow selection
- **[Tutorials](docs/tutorials/index.md)**: Step-by-step guides for major scenarios
- **[How-To Guides](docs/how-to/index.md)**: Task-oriented recipes for specific operations
- **[API Reference](https://jerrettdavis.github.io/JD.Domain/api/)**: Complete API documentation
- **[Changelog](CHANGELOG.md)**: Version history and release notes

## Requirements

- **.NET 8.0 or later** (packages target .NET Standard 2.0 for broad compatibility)
- **Entity Framework Core 8.0+** (for EF Core integration)
- **ASP.NET Core 8.0+** (for ASP.NET Core integration)

## Contributing

Contributions are welcome! Please see our [contributing guide](docs/contributing/index.md) for details on:

- Setting up your development environment
- Coding standards and conventions
- Submitting pull requests
- Reporting issues

## Design Principles

JD.Domain is built on three core principles:

1. **Opt-In**: Use what you need, ignore the rest. No forced dependencies.
2. **Modular**: Packages are focused and composable.
3. **Deterministic**: Snapshots and generation are stable and CI-friendly.

## License

JD.Domain is licensed under the [MIT License](LICENSE).

## Support

- **Documentation**: [https://jerrettdavis.github.io/JD.Domain/](https://jerrettdavis.github.io/JD.Domain/)
- **Issues**: [GitHub Issues](https://github.com/JerrettDavis/JD.Domain/issues)
- **Discussions**: [GitHub Discussions](https://github.com/JerrettDavis/JD.Domain/discussions)

---

**Built with ❤️ by [Jerrett Davis](https://github.com/JerrettDavis)**
