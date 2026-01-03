# API Reference

Welcome to the JD.Domain API reference documentation. This section contains auto-generated API documentation from XML comments for all 15 packages.

## Core Packages

### [JD.Domain.Abstractions](JD.Domain.Abstractions.yml)
Core contracts and primitives including `DomainManifest`, `Result<T>`, `DomainError`, and entity/property/rule manifest types. This is the foundation package that all other packages depend on.

**Key Types:**
- `DomainManifest` - The central model representing your domain
- `Result<T>` - Functional result type for error handling
- `DomainError` - Error representation with severity levels
- `EntityManifest`, `PropertyManifest`, `RuleManifest` - Metadata types

### [JD.Domain.Modeling](JD.Domain.Modeling.yml)
Fluent DSL for domain definition. Start with `Domain.Create()` to build your domain model programmatically.

**Key Types:**
- `Domain` - Entry point for domain modeling
- `DomainBuilder` - Fluent builder for defining domains
- `EntityBuilder<T>` - Builder for entity configuration
- `PropertyBuilder<T>` - Builder for property configuration

### [JD.Domain.Configuration](JD.Domain.Configuration.yml)
Fluent DSL for EF Core-compatible configuration including keys, indexes, and relationships.

**Key Types:**
- `EntityConfigurationBuilder<T>` - Configure tables, indexes, relationships
- `IndexBuilder<T>` - Configure indexes with filtering
- `DomainBuilderConfigurationExtensions` - Extension methods for configuration

### [JD.Domain.Rules](JD.Domain.Rules.yml)
Business rule DSL. Use `RuleSetBuilder<T>` to define invariants, validators, policies, and derivations.

**Key Types:**
- `RuleSetBuilder<T>` - Build rule sets with fluent API
- `CompiledRuleSet<T>` - Compiled, high-performance rule evaluation
- `RuleBuilder<T>` - Individual rule configuration

### [JD.Domain.Runtime](JD.Domain.Runtime.yml)
Rule evaluation engine. Use `DomainRuntime.CreateEngine()` to evaluate rules against entities.

**Key Types:**
- `DomainEngine` - Main rule evaluation engine
- `DomainRuntime` - Factory for creating engines
- `RuleEvaluationResult` - Result of rule evaluation

### [JD.Domain.Validation](JD.Domain.Validation.yml)
Validation problem details and RFC 9457 compliance for standardized error responses.

**Key Types:**
- `ValidationProblemDetails` - RFC 9457-compliant problem details
- `DomainValidationException` - Exception with domain errors

## Integration Packages

### [JD.Domain.EFCore](JD.Domain.EFCore.yml)
Entity Framework Core integration with `ModelBuilder.ApplyDomainManifest()` extension method.

**Key Types:**
- `ModelBuilderExtensions` - Apply domain manifests to EF Core

### [JD.Domain.AspNetCore](JD.Domain.AspNetCore.yml)
ASP.NET Core middleware, endpoint filters, and dependency injection extensions.

**Key Types:**
- `DomainValidationServiceExtensions` - DI registration
- `DomainValidationEndpointFilterExtensions` - Endpoint filter registration
- `DomainValidationMiddleware` - Validation middleware

## Generator Packages

### [JD.Domain.Generators.Core](JD.Domain.Generators.Core.yml)
Source generator infrastructure providing base classes and utilities for code generation.

**Key Types:**
- `GeneratorPipeline` - Pipeline for code generation
- `ICodeGenerator` - Interface for generators
- `GeneratorContext` - Context for generation

### [JD.Domain.DomainModel.Generator](JD.Domain.DomainModel.Generator.yml)
Generates rich domain types from manifests with runtime-safe construction.

**Generates:**
- Immutable entity types
- Value objects
- Enum types
- Builder patterns

### [JD.Domain.FluentValidation.Generator](JD.Domain.FluentValidation.Generator.yml)
Generates FluentValidation validators from JD.Domain rules automatically.

**Generates:**
- FluentValidation `AbstractValidator<T>` classes
- Rule translations

## Tooling Packages

### [JD.Domain.Snapshot](JD.Domain.Snapshot.yml)
Domain manifest versioning and snapshot serialization for tracking domain evolution.

**Key Types:**
- `DomainSnapshot` - Versioned snapshot with hash
- `SnapshotWriter` - Create and serialize snapshots
- `SnapshotReader` - Deserialize snapshots
- `SnapshotStorage` - File system storage

### [JD.Domain.Diff](JD.Domain.Diff.yml)
Snapshot comparison, breaking change detection, and migration plan generation.

**Key Types:**
- `DiffEngine` - Compare snapshots
- `DomainDiff` - Diff result with changes
- `DiffFormatter` - Format as Markdown or JSON
- `MigrationPlanGenerator` - Generate migration recommendations

### [JD.Domain.Cli](JD.Domain.Cli.yml)
CLI tool for snapshot/diff/emit commands.

**Commands:**
- `jd-domain snapshot` - Create snapshots
- `jd-domain diff` - Compare snapshots
- `jd-domain migrate-plan` - Generate migration plans

### [JD.Domain.T4.Shims](JD.Domain.T4.Shims.yml)
T4 template compatibility layer for legacy codebases using T4 templates.

**Key Types:**
- `T4ManifestLoader` - Load manifests in T4
- `T4EntityGenerator` - Generate entities in T4
- `T4TypeMapper` - Map types for T4

## Common Workflows

### Code-First Development
1. Use `Domain.Create()` from [JD.Domain.Modeling](JD.Domain.Modeling.yml)
2. Define rules with `RuleSetBuilder<T>` from [JD.Domain.Rules](JD.Domain.Rules.yml)
3. Evaluate with `DomainEngine` from [JD.Domain.Runtime](JD.Domain.Runtime.yml)

[See full tutorial →](../docs/tutorials/code-first-walkthrough.md)

### Database-First Development
1. Create `DomainManifest` from [JD.Domain.Abstractions](JD.Domain.Abstractions.yml)
2. Apply with `ModelBuilder.ApplyDomainManifest()` from [JD.Domain.EFCore](JD.Domain.EFCore.yml)
3. Add validation rules

[See full tutorial →](../docs/tutorials/db-first-walkthrough.md)

### ASP.NET Core Integration
1. Register services with `AddDomainValidation()` from [JD.Domain.AspNetCore](JD.Domain.AspNetCore.yml)
2. Use middleware or endpoint filters
3. Return RFC 9457 problem details

[See full tutorial →](../docs/tutorials/aspnet-core-integration.md)

## Package Dependencies

```
JD.Domain.Abstractions (foundation for all)
    ├── JD.Domain.Modeling
    ├── JD.Domain.Configuration
    ├── JD.Domain.Rules
    ├── JD.Domain.Runtime
    ├── JD.Domain.Validation
    ├── JD.Domain.Snapshot
    └── JD.Domain.Diff
        └── JD.Domain.Cli

JD.Domain.EFCore
    └── JD.Domain.Abstractions

JD.Domain.AspNetCore
    └── JD.Domain.Validation
        └── JD.Domain.Abstractions
```

[View detailed architecture →](../docs/concepts/architecture.md)
