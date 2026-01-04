# Changelog

All notable changes to JD.Domain Suite will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0-alpha] - 2025-01-03

Initial alpha release of JD.Domain Suite.

### Added

#### Core Packages
- **JD.Domain.Abstractions** - Core contracts and primitives
  - `Result<T>` monad for functional error handling
  - `DomainError` with severity levels and metadata
  - `DomainManifest` as the central domain description model
  - Core interfaces (`IDomainEngine`, `IDomainFactory`)
  - Rule evaluation types and options

- **JD.Domain.Modeling** - Fluent DSL for model description
  - `Domain.Create()` entry point
  - `DomainBuilder` with `Entity<T>`, `ValueObject<T>`, `Enum<T>`
  - Reflection-based model discovery
  - Type metadata extraction

- **JD.Domain.Configuration** - EF-compatible configuration DSL
  - Keys (primary, alternate)
  - Properties (required, length, precision)
  - Indexes (unique, filtered, included properties)
  - Table mapping (name, schema)

- **JD.Domain.Rules** - Rules and invariants DSL
  - Invariants, Validators, Policies, Derivations
  - RuleSetBuilder with fluent chaining
  - Rule composition (Include, When)
  - Severity levels and custom messages

- **JD.Domain.Runtime** - Rule evaluation engine
  - `DomainRuntime.CreateEngine()` factory
  - Synchronous and asynchronous rule evaluation
  - Rule set filtering by name
  - Error/warning/info collection

- **JD.Domain.ManifestGeneration** ⭐ NEW - Opt-in attributes for automatic manifest generation
  - `[GenerateManifest]` - Assembly or DbContext-level manifest configuration
  - `[DomainEntity]` - Marks entity classes for manifest inclusion
  - `[DomainValueObject]` - Marks value object classes for manifest inclusion
  - `[ExcludeFromManifest]` - Opt-out for specific properties or classes
  - NO manual string writing required - metadata extracted automatically from code

#### Integration Packages
- **JD.Domain.EFCore** - Entity Framework Core integration
  - `ModelBuilder.ApplyDomainManifest()` extension
  - Entity configurations from manifests
  - Property, index, key, and table configuration

- **JD.Domain.Validation** - Shared validation contracts
  - `DomainValidationError` record
  - `ValidationProblemDetails` extending ProblemDetails
  - `ProblemDetailsBuilder` fluent builder
  - `ValidationProblemDetailsFactory`

- **JD.Domain.AspNetCore** - ASP.NET Core middleware
  - `UseDomainValidation()` middleware
  - `DomainExceptionHandler` (IExceptionHandler)
  - `AddDomainValidation()` service registration
  - Minimal API extensions (`.WithDomainValidation<T>()`)
  - MVC action filter (`[DomainValidation]` attribute)

#### Generator Packages
- **JD.Domain.Generators.Core** - Base generator infrastructure
  - `BaseCodeGenerator` abstract class
  - `GeneratorPipeline` for chaining generators
  - `CodeBuilder` fluent API with auto-generated headers
  - Deterministic generation infrastructure

- **JD.Domain.ManifestGeneration.Generator** ⭐ NEW - Roslyn source generator for automatic manifest creation
  - Analyzes entity classes at compile-time using Roslyn incremental generator
  - Automatically extracts property metadata from data annotations ([Key], [Required], [MaxLength], etc.)
  - Generates `DomainManifest` code from `[DomainEntity]` and `[DomainValueObject]` attributes
  - Supports assembly-level `[GenerateManifest]` configuration
  - Respects `[ExcludeFromManifest]` opt-out attribute
  - Eliminates manual string writing - all metadata from code
  - Generates at build time, no runtime reflection required

- **JD.Domain.DomainModel.Generator** - Rich domain type generator
  - Domain proxy types (e.g., `DomainBlog`)
  - Construction-safe API with `Result<T>`
  - `FromEntity()` for wrapping tracked entities
  - Property-level rule enforcement
  - `With*()` mutation methods

- **JD.Domain.FluentValidation.Generator** - FluentValidation generator
  - Map JD rules to FluentValidation
  - Generate `AbstractValidator<T>` classes
  - Custom error messages with escaping
  - Severity mapping

#### Tooling Packages
- **JD.Domain.Snapshot** - Domain snapshot serialization
  - `DomainSnapshot` model with metadata and hash
  - Canonical JSON serialization (xxHash64)
  - `SnapshotStorage` for file operations

- **JD.Domain.Diff** - Domain diff and migration planning
  - `DiffEngine` for snapshot comparison
  - Breaking vs non-breaking change classification
  - `DiffFormatter` with Markdown and JSON output
  - `MigrationPlanGenerator`

- **JD.Domain.Cli** - Command-line tools
  - `jd-domain snapshot` command
  - `jd-domain diff` command
  - `jd-domain migrate-plan` command
  - Global tool installation support

- **JD.Domain.T4.Shims** - T4 template integration
  - `T4ManifestLoader` for loading manifests
  - `T4TypeMapper` for type mapping
  - `T4CodeBuilder` for T4-friendly code generation
  - `T4EntityGenerator` for entity code generation

#### Samples
- **JD.Domain.Samples.CodeFirst** - Code-first workflow demonstration
- **JD.Domain.Samples.DbFirst** - Database-first workflow demonstration
- **JD.Domain.Samples.Hybrid** - Mixed sources with snapshot/diff

### Infrastructure
- Directory.Build.props with centralized NuGet metadata
- Source Link integration for debugging
- Deterministic builds
- Symbol packages (snupkg)
- 187 unit tests passing

### v1 Acceptance Criteria Met
1. Database-first workflow: Generate JD partials from existing EF models/configs
2. Code-first workflow: Author JD DSL and generate EF configs
3. Round-trip equivalence: EF to JD to EF produces equivalent model
4. Domain types enforce invariants without external validation calls
5. Snapshot/diff/migration is deterministic and CI-friendly
6. Everything is opt-in; no forced dependencies
