# JD.Domain Suite

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/JerrettDavis/JD.Domain/blob/main/LICENSE)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
![Test Status](https://img.shields.io/badge/tests-371%20passing-brightgreen)

**JD.Domain** is a production-ready, opt-in domain modeling, rules, and configuration suite for .NET that enables seamless interoperability with Entity Framework Core while supporting two-way generation between EF Core models, domain rules, and rich domain types.

---

## Get Started

Choose your workflow to get started in 5 minutes:

<div class="row">
  <div class="column">
    <h3>Code-First</h3>
    <p>Define your domain using the fluent DSL</p>
    <a href="docs/tutorials/code-first-walkthrough.html">Code-First Tutorial →</a>
  </div>
  <div class="column">
    <h3>Database-First</h3>
    <p>Add rules to existing EF Core entities</p>
    <a href="docs/tutorials/db-first-walkthrough.html">Database-First Tutorial →</a>
  </div>
  <div class="column">
    <h3>Hybrid</h3>
    <p>Mix and match with version management</p>
    <a href="docs/tutorials/hybrid-workflow.html">Hybrid Tutorial →</a>
  </div>
</div>

[Quick Start Guide](docs/getting-started/quick-start.md) | [Installation](docs/getting-started/installation.md) | [API Reference](api/index.md)

---

## Key Features

### Opt-In Domain Rules
Attach business rules to any anemic model (generated or handwritten) with zero required base interfaces. Your existing code stays clean.

### Two-Way Generation
Generate EF Core configurations from JD.Domain rules, or generate domain models from EF Core - true bidirectional support.

### Rich Domain Models
Generate runtime-safe domain types with automatic invariant enforcement and immutable construction patterns.

### Framework Integration
Seamless integration with:
- **Entity Framework Core** - ModelBuilder extensions and configuration
- **ASP.NET Core** - Middleware and endpoint filters
- **FluentValidation** - Automatic validator generation

### Version Management
Track domain evolution with snapshots, compare versions, detect breaking changes, and generate migration plans.

### Developer Tools
CLI tools for CI/CD integration, source generators for productivity, and T4 template support for legacy codebases.

---

## Example: Code-First Workflow

```csharp
using JD.Domain.Modeling;
using JD.Domain.Rules;
using JD.Domain.Runtime;

// Define domain model
var domain = Domain.Create("ECommerce")
    .Entity<Customer>()
    .Entity<Order>()
    .Build();

// Define business rules
var customerRules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Customer.Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
    .WithMessage("Customer name cannot be empty")
    .BuildCompiled();

// Validate at runtime
var result = customerRules.Evaluate(customer);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error.Message);
    }
}
```

[See full tutorial →](docs/tutorials/code-first-walkthrough.md)

---

## Architecture

The suite is organized into 15 modular packages that you can mix and match:

| Category | Packages |
|----------|----------|
| **Core** | [Abstractions](api/JD.Domain.Abstractions.html), [Modeling](api/JD.Domain.Modeling.html), [Configuration](api/JD.Domain.Configuration.html), [Rules](api/JD.Domain.Rules.html), [Runtime](api/JD.Domain.Runtime.html), [Validation](api/JD.Domain.Validation.html) |
| **Integration** | [AspNetCore](api/JD.Domain.AspNetCore.html), [EFCore](api/JD.Domain.EFCore.html) |
| **Generators** | [Generators.Core](api/JD.Domain.Generators.Core.html), [DomainModel.Generator](api/JD.Domain.DomainModel.Generator.html), [FluentValidation.Generator](api/JD.Domain.FluentValidation.Generator.html) |
| **Tooling** | [Snapshot](api/JD.Domain.Snapshot.html), [Diff](api/JD.Domain.Diff.html), [Cli](api/JD.Domain.Cli.html), [T4.Shims](api/JD.Domain.T4.Shims.html) |

[View package comparison →](docs/reference/package-matrix.md)

---

## Design Principles

- **Opt-in everything** - No required base classes or interfaces
- **Single source of truth** - Define once, generate everywhere
- **Deterministic outputs** - Stable, predictable code generation
- **Modular** - Use only what you need
- **Extensible** - Add custom primitives and hooks

[Learn more about our design philosophy →](docs/concepts/design-principles.md)

---

## Sample Applications

Explore working examples demonstrating different workflows:

- **[CodeFirst Sample](https://github.com/JerrettDavis/JD.Domain/tree/main/samples/JD.Domain.Samples.CodeFirst)** - Build domain models from scratch using the fluent DSL
- **[DbFirst Sample](https://github.com/JerrettDavis/JD.Domain/tree/main/samples/JD.Domain.Samples.DbFirst)** - Add rules to existing EF Core scaffolded entities
- **[Hybrid Sample](https://github.com/JerrettDavis/JD.Domain/tree/main/samples/JD.Domain.Samples.Hybrid)** - Version management with snapshot and diff tools

[Sample applications guide →](docs/reference/samples.md)

---

## Documentation

### Getting Started
- [Installation Guide](docs/getting-started/installation.md)
- [Quick Start (5 minutes)](docs/getting-started/quick-start.md)
- [Choose Your Workflow](docs/getting-started/choose-workflow.md)

### Tutorials
- [Code-First Walkthrough](docs/tutorials/code-first-walkthrough.md)
- [Database-First Walkthrough](docs/tutorials/db-first-walkthrough.md)
- [Domain Modeling](docs/tutorials/domain-modeling.md)
- [Business Rules](docs/tutorials/business-rules.md)
- [EF Core Integration](docs/tutorials/ef-core-integration.md)
- [ASP.NET Core Integration](docs/tutorials/aspnet-core-integration.md)

### Concepts
- [Architecture Overview](docs/concepts/architecture.md)
- [Rule System](docs/concepts/rule-system.md)
- [Domain Manifest](docs/concepts/domain-manifest.md)
- [Result Monad Pattern](docs/concepts/result-monad.md)

### Reference
- [API Documentation](api/index.md)
- [CLI Commands](docs/reference/cli-commands.md)
- [Package Matrix](docs/reference/package-matrix.md)

[View all documentation →](docs/index.md)

---

## Current Status

**v1.0.0 Release Candidate** - All core functionality complete

- ✅ 15 packages fully implemented
- ✅ 371 tests passing
- ✅ Complete API documentation
- ✅ Production-ready samples

[View changelog →](docs/changelog/index.md) | [View roadmap →](docs/changelog/roadmap.md)

---

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

# CLI tool (global install)
dotnet tool install -g JD.Domain.Cli
```

[Complete installation guide →](docs/getting-started/installation.md)

---

## Contributing

Contributions are welcome! See our [contributing guide](docs/contributing/index.md) for details.

- [Development Setup](docs/contributing/development-setup.md)
- [Coding Standards](docs/contributing/coding-standards.md)
- [Testing Guidelines](docs/contributing/testing-guidelines.md)

---

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/JerrettDavis/JD.Domain/blob/main/LICENSE) file for details.

---

## Related Projects

- [TinyBDD](https://github.com/JerrettDavis/TinyBDD) - BDD testing framework used for testing JD.Domain
- [JD.Efcpt.Build](https://github.com/JerrettDavis/JD.Efcpt.Build) - EF Core reverse engineering tools

---

**Created by [Jerrett Davis](https://github.com/JerrettDavis)**
