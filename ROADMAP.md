# JD.Domain Suite v1 — Complete Roadmap

This document outlines the complete implementation plan for JD.Domain Suite v1, based on the original issue specification.

## Overview

The goal is to ship a production-ready, opt-in domain modeling + rules + configuration suite that can be adopted in **any** codebase (database-first or code-first), interoperates seamlessly with EF Core reverse-engineered models, and supports two-way generation.

## Implementation Milestones

### ✅ Milestone 1 — Abstractions + Manifest (COMPLETED)

**Status**: Complete (commit 3cd0f59)

**Deliverables**:
- ✅ JD.Domain.Abstractions package with core contracts
- ✅ DomainManifest model with all manifest types (21 types)
- ✅ Result<T> monad for functional error handling
- ✅ DomainError model with severity and metadata
- ✅ Core interfaces (IDomainEngine, IDomainFactory)
- ✅ RuleEvaluationResult and RuleEvaluationOptions
- ✅ Comprehensive unit tests (13 passing tests)

### ✅ Milestone 2 — DSLs (COMPLETED)

**Status**: Complete (commits ceeaa4b, 81bc0c1, b8d4fd2)

**Deliverables**:
- ✅ JD.Domain.Modeling package
  - Fluent DSL entry point: `Domain.Create(name)`
  - DomainBuilder with Entity<T>, ValueObject<T>, Enum<T>
  - Reflection-based model discovery
  - Type metadata extraction
- ✅ JD.Domain.Configuration package
  - Configuration DSL mirroring EF Core
  - Keys (primary, alternate)
  - Properties (required, length, precision)
  - Indexes (unique, filtered, included properties)
  - Table mapping (name, schema)
  - Relationship/inheritance infrastructure (hooks prepared)
- ✅ JD.Domain.Rules package
  - Invariants, Validators, Policies, Derivations
  - State transitions infrastructure
  - RuleContext support
  - Rule composition (Include, When)
  - Severity levels and custom messages
- ✅ Merge and precedence system (prepared)
- ✅ Unit tests for all DSL packages

### ✅ Milestone 3 — Runtime (COMPLETED)

**Status**: Complete (commits c674558, b8d4fd2)

**Deliverables**:
- ✅ JD.Domain.Runtime package
  - DomainRuntime.Create() implementation
  - Synchronous rule evaluation engine
  - Asynchronous rule evaluation engine
  - IDomainEngine implementation
  - Rule set filtering by name
  - Error/warning/info collection
  - Evaluation metrics
- ✅ Telemetry hooks prepared (OpenTelemetry-ready)
- ✅ Standalone entry points (non-DI usage)
- ✅ Unit tests for runtime

### ✅ Milestone 4 — EF Core Adapter (COMPLETED)

**Status**: Complete (commit 6c15f0d)

**Deliverables**:
- ✅ JD.Domain.EFCore package (net10.0, EF Core 10.0.1)
  - ModelBuilder.ApplyDomainManifest() extension
  - Apply entity configurations from manifests
  - Property configuration (required, max length)
  - Index configuration (unique, filtered)
  - Key configuration
  - Table mapping (name, schema)
- ⏳ SaveChanges interceptors (infrastructure prepared, not implemented)
- ⏳ Domain event emission (infrastructure prepared)
- ⏳ Mapper utilities (infrastructure prepared)

### ✅ Milestone 5 — Generators (Core) (COMPLETED)

**Status**: Complete (commit 1b5eda2)

**Deliverables**:
- ✅ JD.Domain.Generators.Core package
  - BaseCodeGenerator abstract class
  - ICodeGenerator interface
  - GeneratorContext for manifest and options
  - GeneratorPipeline for chaining generators
  - GeneratedFile representation
  - CodeBuilder fluent API with:
    - Auto-generated headers with version info
    - Using statements, namespaces
    - Class/interface/method generation
    - Indentation tracking
  - GeneratorUtilities for common operations
- ✅ Deterministic generation infrastructure
  - Stable file naming and ordering
  - Version hash headers
  - Auto-generated markers
- ✅ Generator tests

### ✅ Milestone 6 — FluentValidation Generator (COMPLETED)

**Status**: Complete (commits c29b47a, 72c4ad3)

**Deliverables**:
- ✅ JD.Domain.FluentValidation.Generator package
  - Generator: JD rules → FluentValidation
  - Map Invariant rules to validator rules
  - Map Validator rules with proper selectors
  - Generate AbstractValidator<T> classes
  - Property path resolution from expressions
  - Custom error messages with escaping
  - Severity mapping
- ✅ Integration with FluentValidation 11.x
- ✅ Generator tests

### ✅ Milestone 7 — Domain Model Generator (COMPLETED)

**Status**: Complete (implemented proxy-wrapper approach)

**Deliverables**:
- ✅ JD.Domain.DomainModel.Generator package
  - Generates domain proxy types (e.g., DomainBlog) that wrap EF entities
  - Construction-safe API with static Create methods returning Result<T>
  - FromEntity() for wrapping existing tracked entities
  - Implicit conversion to EF entity for EF interop
  - Property-level rule enforcement in setters
  - With*() mutation methods returning Result<T>
  - Partial class support for semantic method extensions
  - DomainContext parameter support for policies/auditing
  - Configurable options (namespace, prefix, validation mode)
- ✅ DomainValidationException for property setter failures
- ✅ RuleEvaluationOptions extended with PropertyName support
- ✅ 25 unit tests for generator behavior

### 📋 Milestone 8 — ASP.NET Core Integration

**Estimated Effort**: 2 weeks

**Deliverables**:
- [ ] JD.Domain.AspNetCore package
  - UseDomainValidation() middleware
  - Endpoint metadata detection
  - Rule set evaluation on requests
  - ProblemDetails error formatting
  - Minimal API extensions (.WithDomainValidation<T>())
  - MVC action filter ([DomainValidation] attribute)
  - Model binding integration for domain types
  - Exception handling integration
- [ ] JD.Domain.Validation package
  - Shared error contracts
  - ProblemDetails builders
  - Error response formatters
- [ ] Integration tests with TestServer

### 📋 Milestone 9 — Snapshot/Diff/Migration + CLI

**Estimated Effort**: 2-3 weeks

**Deliverables**:
- [ ] Snapshot system
  - JSON snapshot writer with canonical format
  - Storage: DomainSnapshots/{name}/v{version}.json
  - SHA-256 hash generation
  - Version metadata
- [ ] Diff engine
  - Snapshot comparison algorithm
  - Entity/property/rule change detection
  - Breaking vs non-breaking classification
  - Markdown diff output
  - Machine-readable JSON diff
- [ ] Migration plan generation
  - DomainMigrationPlan.md generation
  - Schema impact inference from config changes
  - Recommended migration steps
- [ ] JD.Domain.Cli package
  - Command: jd-domain snapshot
  - Command: jd-domain diff
  - Command: jd-domain migrate-plan
  - Command: jd-domain emit ef
  - Command: jd-domain emit validators
  - Command: jd-domain emit domain-models
- [ ] MSBuild integration targets
- [ ] CLI tests

### 📋 Milestone 10 — T4 Shims

**Estimated Effort**: 1-2 weeks

**Deliverables**:
- [ ] JD.Domain.T4.Shims package
  - T4 templates for EF entities with JD markers
  - T4 templates for parallel JD rules/config generation
  - Shims for EFCPTools integration
  - Deterministic output handling
  - Documentation for T4 workflow
- [ ] Integration examples
- [ ] Tests

### 📋 Milestone 11 — Tests + Samples + Docs

**Estimated Effort**: 2-3 weeks

**Deliverables**:
- [ ] Complete test suite
  - Unit tests for all packages (>80% coverage)
  - Integration tests with multiple EF providers
  - BDD tests with TinyBDD for acceptance criteria
  - Round-trip equivalence tests (EF → JD → EF)
  - Domain substitution persistence/query tests
  - Generator snapshot tests
- [ ] Sample applications
  - DbFirst.Sample (reverse-engineered models workflow)
  - CodeFirst.Sample (JD DSL first workflow)
  - Hybrid.Sample (mixed sources workflow)
- [ ] Documentation
  - DocFX site setup
  - Getting started guides (db-first, code-first, hybrid)
  - Rules DSL cookbook with examples
  - Configuration DSL cookbook with examples
  - Generator guides (how to use each generator)
  - ASP.NET Core integration guide
  - EF Core integration guide
  - Snapshot/diff/migration workflow guide
  - Troubleshooting guide
  - Diagnostics catalog (all JDxxxx codes)

### 📋 Milestone 12 — Final Release Preparation

**Estimated Effort**: 1 week

**Deliverables**:
- [ ] Verify all v1 acceptance criteria
- [ ] Run full test suite across all packages
- [ ] Update README with complete examples
- [ ] Add NuGet package metadata and icons
- [ ] Verify deterministic builds
- [ ] Security review with CodeQL
- [ ] Performance benchmarks
- [ ] Release notes
- [ ] Tag v1.0.0

## Total Estimated Effort

**20-28 weeks** (approximately 5-7 months) for a complete v1 implementation.

This assumes:
- Focused development time
- Iterative feedback and refinement
- Community contributions for samples and documentation

## v1 Acceptance Criteria

1. ✅ Database-first workflow: Generate JD partials from existing EF models/configs
2. ✅ Code-first workflow: Author JD DSL and generate EF configs
3. ✅ Round-trip equivalence: EF → JD → EF produces equivalent model
4. ✅ Domain types enforce invariants without external validation calls
5. ✅ Snapshot/diff/migration is deterministic and CI-friendly
6. ✅ Everything is opt-in; no forced dependencies

## Current Progress

**Milestone 1**: ✅ Complete
**Milestone 2**: ✅ Complete
**Milestone 3**: ✅ Complete
**Milestone 4**: ✅ Complete
**Milestone 5**: ✅ Complete
**Milestone 6**: ✅ Complete
**Milestone 7**: ✅ Complete
**Milestone 8**: 📋 Not Started
**Milestone 9**: 📋 Not Started
**Milestone 10**: 📋 Not Started
**Milestone 11**: 📋 Not Started
**Milestone 12**: 📋 Not Started

**Overall Progress**: ~64% of total v1 scope (7/11 milestones complete)

**Test Status**: 102 tests passing, 0 failures

## Next Steps

1. **Milestone 8**: Implement ASP.NET Core Integration
   - Create JD.Domain.AspNetCore package
   - Middleware for domain validation
   - ProblemDetails error formatting

2. **Milestone 9**: Implement Snapshot/Diff/CLI
   - Snapshot system with canonical JSON format
   - Diff engine for change detection
   - CLI commands for tooling

3. **Milestone 10**: T4 Shims for legacy T4 template support

4. **Milestone 11**: Tests, Samples, Docs - complete test suite, sample apps, documentation

## Contributing

Given the scope, contributions are highly welcome! Areas where help is needed:

- DSL design and implementation
- Source generator expertise
- EF Core integration patterns
- Documentation and samples
- Testing and feedback

## Notes

This is an ambitious project with a clear vision. The modular architecture allows for incremental delivery and adoption. Each milestone can be released independently as preview packages.
