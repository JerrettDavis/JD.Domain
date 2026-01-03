# JD.Domain Suite

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)

**JD.Domain** is a production-ready, opt-in domain modeling, rules, and configuration suite for .NET that enables seamless interoperability with Entity Framework Core while supporting two-way generation between EF Core models, domain rules, and rich domain types.

## Features

- **Opt-in domain rules** attachable to any anemic model (generated or handwritten) with zero required base interfaces
- **Two-way generation** between EF Core configurations and JD domain rulesets
- **Rich domain model generation** with runtime-safe construction and automatic invariant enforcement
- **FluentValidation integration** for request validation
- **ASP.NET Core middleware** for seamless API integration
- **Snapshot and diff tools** for domain evolution tracking
- **CLI tools** for CI/CD integration

## Current Status

**v1.0.0 Release Candidate** - All core functionality complete (95% of v1 scope)

| Package | Status |
|---------|--------|
| JD.Domain.Abstractions | ✅ Complete |
| JD.Domain.Modeling | ✅ Complete |
| JD.Domain.Configuration | ✅ Complete |
| JD.Domain.Rules | ✅ Complete |
| JD.Domain.Runtime | ✅ Complete |
| JD.Domain.Validation | ✅ Complete |
| JD.Domain.AspNetCore | ✅ Complete |
| JD.Domain.EFCore | ✅ Complete |
| JD.Domain.Generators.Core | ✅ Complete |
| JD.Domain.DomainModel.Generator | ✅ Complete |
| JD.Domain.FluentValidation.Generator | ✅ Complete |
| JD.Domain.Snapshot | ✅ Complete |
| JD.Domain.Diff | ✅ Complete |
| JD.Domain.Cli | ✅ Complete |
| JD.Domain.T4.Shims | ✅ Complete |

**Test Status**: 187 tests passing

See [ROADMAP.md](ROADMAP.md) for the complete implementation plan.

## Quick Start

### Code-First Workflow

Define your domain model using the fluent DSL:

```csharp
using JD.Domain.Modeling;
using JD.Domain.Rules;
using JD.Domain.Runtime;

// Define domain model
var domain = Domain.Create("ECommerce")
    .Entity<Customer>()
    .Entity<Order>()
    .Entity<Product>()
    .Build();

// Define business rules
var customerRules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Customer.Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
    .WithMessage("Customer name cannot be empty")
    .Invariant("Customer.Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
    .WithMessage("Customer email cannot be empty")
    .Build();

// Validate at runtime
var engine = DomainRuntime.CreateEngine(domain);
var result = engine.Evaluate(customer, customerRules);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

### Database-First Workflow

Add rules to existing EF Core scaffolded entities:

```csharp
using JD.Domain.Abstractions;
using JD.Domain.Rules;
using JD.Domain.Runtime;

// Create manifest from existing EF entities
var manifest = new DomainManifest
{
    Name = "BloggingDb",
    Version = new Version(1, 0, 0),
    Entities = [
        new EntityManifest
        {
            Name = "Blog",
            TypeName = "MyApp.Blog",
            TableName = "Blogs",
            Properties = [
                new PropertyManifest { Name = "BlogId", TypeName = "System.Int32", IsRequired = true },
                new PropertyManifest { Name = "Url", TypeName = "System.String", IsRequired = true, MaxLength = 500 }
            ],
            KeyProperties = ["BlogId"]
        }
    ]
};

// Define rules for existing entities
var blogRules = new RuleSetBuilder<Blog>("Default")
    .Invariant("Blog.Url.Required", b => !string.IsNullOrWhiteSpace(b.Url))
    .WithMessage("Blog must have a valid URL")
    .Invariant("Blog.Url.Protocol", b => b.Url.StartsWith("http"))
    .WithMessage("Blog URL must start with http:// or https://")
    .Build();

// Validate
var engine = DomainRuntime.CreateEngine(manifest);
var result = engine.Evaluate(blog, blogRules);
```

### Snapshot and Diff

Track domain evolution with snapshots and diffs:

```csharp
using JD.Domain.Snapshot;
using JD.Domain.Diff;

// Create snapshots
var writer = new SnapshotWriter();
var snapshotV1 = writer.CreateSnapshot(manifestV1);
var snapshotV2 = writer.CreateSnapshot(manifestV2);

// Compare versions
var diffEngine = new DiffEngine();
var diff = diffEngine.Compare(snapshotV1, snapshotV2);

Console.WriteLine($"Has changes: {diff.HasChanges}");
Console.WriteLine($"Breaking changes: {diff.HasBreakingChanges}");

// Generate diff report
var formatter = new DiffFormatter();
var markdown = formatter.FormatAsMarkdown(diff);

// Generate migration plan
var planGenerator = new MigrationPlanGenerator();
var plan = planGenerator.Generate(diff);
```

### CLI Tools

```bash
# Install the CLI tool
dotnet tool install -g JD.Domain.Cli

# Create a snapshot
jd-domain snapshot --manifest domain.json --output ./snapshots

# Compare versions
jd-domain diff v1.json v2.json --format md

# Generate migration plan
jd-domain migrate-plan v1.json v2.json --output migration-plan.md
```

### EF Core Integration

Apply domain configurations to EF Core:

```csharp
using JD.Domain.EFCore;

public class MyDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyDomainManifest(manifest);
    }
}
```

### ASP.NET Core Integration

Add domain validation to your API:

```csharp
using JD.Domain.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add domain validation services
builder.Services.AddDomainValidation(options =>
{
    options.AddManifest(manifest);
});

var app = builder.Build();

// Use domain validation middleware
app.UseDomainValidation();

// Or use endpoint filters
app.MapPost("/api/customers", (Customer customer) => ...)
    .WithDomainValidation<Customer>();
```

## Architecture

The suite is organized into modular packages:

```
JD.Domain.Abstractions         Core contracts and primitives
JD.Domain.Modeling             Fluent DSL for model description
JD.Domain.Configuration        EF-compatible configuration DSL
JD.Domain.Rules                Rules and invariants DSL
JD.Domain.Runtime              Rule evaluation engine
JD.Domain.Validation           Shared validation contracts
JD.Domain.AspNetCore           ASP.NET Core middleware
JD.Domain.EFCore               EF Core integration
JD.Domain.Generators.Core      Base generator infrastructure
JD.Domain.DomainModel.Generator   Rich domain type generator
JD.Domain.FluentValidation.Generator   FluentValidation generator
JD.Domain.Snapshot             Domain snapshot serialization
JD.Domain.Diff                 Domain diff and migration planning
JD.Domain.Cli                  Command-line tools
JD.Domain.T4.Shims             T4 template integration
```

## Design Principles

- **Opt-in everything** - No required base classes or interfaces
- **Single source of truth** - Define once, generate everywhere
- **Deterministic outputs** - Stable, predictable code generation
- **Modular** - Use only what you need
- **Extensible** - Add custom primitives and hooks

## Sample Applications

The repository includes sample applications demonstrating different workflows:

- **JD.Domain.Samples.CodeFirst** - Code-first workflow with domain DSL
- **JD.Domain.Samples.DbFirst** - Database-first workflow with EF entities
- **JD.Domain.Samples.Hybrid** - Mixed sources with snapshot/diff

## Installation

```powershell
# Core packages
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime

# EF Core integration
dotnet add package JD.Domain.EFCore

# ASP.NET Core integration
dotnet add package JD.Domain.AspNetCore

# Generators
dotnet add package JD.Domain.DomainModel.Generator
dotnet add package JD.Domain.FluentValidation.Generator

# Snapshot and diff
dotnet add package JD.Domain.Snapshot
dotnet add package JD.Domain.Diff

# CLI tool
dotnet tool install -g JD.Domain.Cli
```

## Contributing

Contributions are welcome! See [ROADMAP.md](ROADMAP.md) for areas where help is needed.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- [TinyBDD](https://github.com/JerrettDavis/TinyBDD) - BDD testing framework used for testing
- [JD.Efcpt.Build](https://github.com/JerrettDavis/JD.Efcpt.Build) - EF Core reverse engineering tools

## Acknowledgments

Created by [Jerrett Davis](https://github.com/JerrettDavis)
