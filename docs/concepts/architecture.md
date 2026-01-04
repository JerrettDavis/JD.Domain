# Architecture Overview

JD.Domain Suite architecture and package organization.

## System Design

JD.Domain follows a modular, layered architecture:

### Core Layer
- **Abstractions** - Contracts and primitives (Result<T>, DomainError, DomainManifest)
- **Modeling** - Fluent DSL for domain definitions
- **Configuration** - EF Core configuration DSL
- **Rules** - Business rules DSL
- **Runtime** - Rule evaluation engine

### Integration Layer
- **EFCore** - Entity Framework Core integration
- **AspNetCore** - ASP.NET Core middleware and filters
- **Validation** - Shared validation contracts

### Generation Layer
- **Generators.Core** - Base generator infrastructure
- **DomainModel.Generator** - Rich domain type generator
- **FluentValidation.Generator** - Validator generator

### Tooling Layer
- **Snapshot** - Domain snapshot serialization
- **Diff** - Change detection and comparison
- **Cli** - Command-line tools
- **T4.Shims** - T4 template integration

## Package Dependencies

```
Abstractions (netstandard2.0)
    ├─ Modeling → Configuration → EFCore
    ├─ Rules → Runtime → AspNetCore
    ├─ Generators.Core → DomainModel.Generator
    ├─ Generators.Core → FluentValidation.Generator
    └─ Snapshot → Diff → Cli
```

## Design Goals

1. **Opt-in** - No forced dependencies or base classes
2. **Modular** - Use only what you need
3. **Deterministic** - Stable, predictable outputs
4. **Extensible** - Clear extension points

## See Also
- [Design Principles](design-principles.md)
- [Package Matrix](../reference/package-matrix.md)
