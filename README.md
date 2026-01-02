# JD.Domain Suite

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)

**JD.Domain** is a production-ready, opt-in domain modeling, rules, and configuration suite for .NET that enables seamless interoperability with Entity Framework Core while supporting two-way generation between EF Core models, domain rules, and rich domain types.

## Vision

The JD.Domain Suite aims to provide:

- **Opt-in domain rules** attachable to any anemic model (generated or handwritten) with zero required base interfaces
- **Two-way generation** between EF Core configurations and JD domain rulesets
- **Rich domain model generation** with runtime-safe construction and automatic invariant enforcement
- **FluentValidation integration** for request validation
- **ASP.NET Core middleware** for seamless API integration
- **Snapshot and migration tools** for domain evolution tracking

## Current Status

🚧 **This project is in early development (v0.1 alpha)**

### ✅ Completed (Milestone 1)

- **JD.Domain.Abstractions** - Core abstractions and primitives
  - `Result<T>` monad for functional error handling
  - `DomainError` with severity levels and metadata
  - `DomainManifest` as the central domain description model
  - Core interfaces (`IDomainEngine`, `IDomainFactory`)
  - Comprehensive unit tests

### 🔄 In Progress

- **JD.Domain.Modeling** - Fluent DSL for describing models
- **JD.Domain.Configuration** - Configuration DSL mirroring EF Core
- **JD.Domain.Rules** - Domain rules and invariants DSL

### 📋 Planned

See [ROADMAP.md](ROADMAP.md) for the complete implementation plan.

## Quick Example (Future API)

```csharp
// Define your domain model with rules
var manifest = Domain.Create("Blogging")
    .Version(1, 0, 0)
    .Entity<Blog>(e => e
        .Key(x => x.Id).Guid()
        .Property(x => x.Name).Required().MaxLength(200)
        .HasMany(x => x.Posts).WithOne(x => x.Blog).Required()
    )
    .Rules<Blog>(r => r
        .Invariant("NameRequired", x => !string.IsNullOrWhiteSpace(x.Name))
        .Invariant("NameMaxLength", x => x.Name.Length <= 200)
    )
    .BuildManifest();

// Generate EF configurations, validators, and rich domain types automatically
```

## Installation

```powershell
# Core abstractions (currently available)
dotnet add package JD.Domain.Abstractions

# Other packages coming soon
```

## Architecture

The suite is organized into modular packages:

```
JD.Domain.Abstractions         ✅ Core contracts and primitives
JD.Domain.Modeling             🔄 Fluent DSL for model description
JD.Domain.Configuration        📋 EF-compatible configuration DSL
JD.Domain.Rules                📋 Rules and invariants DSL
JD.Domain.Runtime              📋 Rule evaluation engine
JD.Domain.Validation           📋 Shared validation contracts
JD.Domain.AspNetCore           📋 ASP.NET Core middleware
JD.Domain.EFCore               📋 EF Core integration
JD.Domain.EFCore.Generators    📋 Source generators (EF ↔ JD)
JD.Domain.DomainModel.Generator 📋 Rich domain type generator
JD.Domain.FluentValidation.Generator 📋 FluentValidation generator
JD.Domain.Cli                  📋 Command-line tools
JD.Domain.T4.Shims             📋 T4 template integration
```

## Design Principles

- **Opt-in everything** - No required base classes or interfaces
- **Single source of truth** - Define once, generate everywhere
- **Deterministic outputs** - Stable, predictable code generation
- **Modular** - Use only what you need
- **Extensible** - Add custom primitives and hooks

## Contributing

Contributions are welcome! This project is in its early stages and there's much to build.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- [TinyBDD](https://github.com/JerrettDavis/TinyBDD) - BDD testing framework used for testing
- [JD.Efcpt.Build](https://github.com/JerrettDavis/JD.Efcpt.Build) - EF Core reverse engineering tools

## Acknowledgments

Created by [Jerrett Davis](https://github.com/JerrettDavis)
