# Getting Started with JD.Domain

Welcome to JD.Domain Suite, a production-ready domain modeling and rules framework for .NET that helps you build rich domain models while maintaining compatibility with Entity Framework Core.

## What is JD.Domain?

JD.Domain is a modular suite of packages that enables you to:

- **Define business rules** that attach to any .NET class without requiring base interfaces
- **Enforce invariants** at construction time and during mutations
- **Generate code** from domain definitions (FluentValidation validators, rich domain types)
- **Track domain evolution** with snapshots, diffs, and migration plans
- **Integrate with EF Core** through two-way configuration generation
- **Validate in ASP.NET Core** with built-in middleware and endpoint filters

## Why Use JD.Domain?

### Traditional Approach Problems

In typical .NET applications, business rules are scattered across:
- Property setters with throw statements
- Separate validator classes (FluentValidation, DataAnnotations)
- Application service methods
- Controller action methods

This creates several issues:
- **Duplication** - Same rules repeated in multiple places
- **Inconsistency** - Different validation for API vs. domain vs. database
- **Fragility** - Easy to forget validation in one path
- **Poor discoverability** - Hard to find all rules for an entity

### JD.Domain Approach

JD.Domain provides a single source of truth for your domain:

```csharp
// Define domain model and rules once
var domain = Domain.Create("ECommerce")
    .Entity<Customer>(e => e
        .Property(c => c.Name)
        .Property(c => c.Email))
    .WithRules<Customer>(rules => rules
        .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
        .Invariant("Email.Valid", c => c.Email.Contains("@")))
    .Build();
```

Then generate or validate everywhere:
- ✅ Construction-safe domain types with `Result<T>`
- ✅ FluentValidation validators for API request validation
- ✅ EF Core configurations for database constraints
- ✅ Runtime validation in services
- ✅ ASP.NET Core middleware for automatic API validation

## Key Features

### 1. Opt-In Architecture

JD.Domain doesn't force your entities to inherit base classes or implement interfaces. You can add domain rules to any class, including EF Core scaffolded entities.

```csharp
// Works with any class - no base class required
public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }
}

// Add rules without modifying the class
var rules = new RuleSetBuilder<Blog>("Default")
    .Invariant("Url.Required", b => !string.IsNullOrWhiteSpace(b.Url))
    .Build();
```

### 2. Two-Way Generation

Define your domain in code or import from existing EF Core models, then generate:
- **Code-First** → Generate EF configurations, validators, rich types
- **Database-First** → Import EF scaffolded models, add rules, generate
- **Hybrid** → Mix both approaches with snapshot/diff tracking

### 3. Type-Safe Construction

Generate rich domain types that enforce invariants at construction time:

```csharp
// Generated domain type (safe construction)
var result = DomainCustomer.Create(name: "", email: "invalid");
if (!result.IsSuccess)
{
    // Returns Result<DomainCustomer> with validation errors
    Console.WriteLine(result.Error.Message);
}

// Or wrap existing entity
var customer = new Customer { Name = "John" };
var domainCustomer = DomainCustomer.FromEntity(customer);
```

### 4. Domain Evolution Tracking

As your domain evolves, track changes with snapshots and detect breaking changes:

```bash
# Create snapshot of current domain
jd-domain snapshot --manifest domain.json --output ./snapshots/v1.json

# Compare with previous version
jd-domain diff ./snapshots/v1.json ./snapshots/v2.json --format md

# Generate migration plan
jd-domain migrate-plan v1.json v2.json
```

## Supported Workflows

JD.Domain supports three main workflows:

### Code-First Workflow
Start with domain definitions using fluent DSL, then generate everything (EF configurations, validators, domain types).

**Best for:** New projects, greenfield development, domain-driven design

### Database-First Workflow
Start with existing database and EF Core scaffolded models, then add domain rules and generate validators/domain types.

**Best for:** Existing projects, legacy database modernization, retrofitting domain logic

### Hybrid Workflow
Mix code-first domain definitions with reverse-engineered database models, track evolution with snapshots.

**Best for:** Large projects with multiple teams, gradual migration, maintaining consistency

## Package Overview

JD.Domain is organized into focused packages you can adopt incrementally:

| Package | Purpose |
|---------|---------|
| **JD.Domain.Abstractions** | Core contracts and primitives (`Result<T>`, `DomainError`, `DomainManifest`) |
| **JD.Domain.Modeling** | Fluent DSL for defining domain models |
| **JD.Domain.Rules** | Fluent DSL for business rules (invariants, validators, policies) |
| **JD.Domain.Runtime** | Rule evaluation engine |
| **JD.Domain.EFCore** | Entity Framework Core integration |
| **JD.Domain.AspNetCore** | ASP.NET Core middleware and endpoint filters |
| **JD.Domain.DomainModel.Generator** | Source generator for rich domain types |
| **JD.Domain.FluentValidation.Generator** | FluentValidation generator |
| **JD.Domain.Snapshot** | Domain snapshot serialization |
| **JD.Domain.Diff** | Domain comparison and change detection |
| **JD.Domain.Cli** | Command-line tools for CI/CD |

## Next Steps

Ready to start? Choose your path:

1. **[Installation](installation.md)** - Install the packages you need
2. **[Quick Start](quick-start.md)** - Build your first domain model in 5 minutes
3. **[Choose Your Workflow](choose-workflow.md)** - Decide between Code-First, Database-First, or Hybrid

Or explore the samples:
- [Code-First Sample](../tutorials/code-first-walkthrough.md) - Complete walkthrough of code-first approach
- [Database-First Sample](../tutorials/db-first-walkthrough.md) - Retrofit rules onto existing database
- [Hybrid Sample](../tutorials/hybrid-workflow.md) - Mix both approaches with snapshots

## Requirements

- **.NET 10.0 or later** (for packages targeting `net10.0`)
- **.NET Standard 2.0 or later** (for core abstractions)
- **Entity Framework Core 10.0 or later** (optional, for EF integration)
- **ASP.NET Core 10.0 or later** (optional, for web integration)
- **FluentValidation 11.x** (optional, for validator generation)

## Support and Community

- **Documentation** - [Full Documentation](../index.md)
- **Issues** - [GitHub Issues](https://github.com/JerrettDavis/JD.Domain/issues)
- **Samples** - See the `samples/` folder in the repository
- **Contributing** - See [Contributing Guide](../contributing/index.md)

## License

JD.Domain is licensed under the [MIT License](https://github.com/JerrettDavis/JD.Domain/blob/main/LICENSE).
