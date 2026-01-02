# JD.Domain Suite v1 — Complete Roadmap

This document outlines the complete implementation plan for JD.Domain Suite v1, based on the original issue specification.

## Overview

The goal is to ship a production-ready, opt-in domain modeling + rules + configuration suite that can be adopted in **any** codebase (database-first or code-first), interoperates seamlessly with EF Core reverse-engineered models, and supports two-way generation.

## Implementation Milestones

### ✅ Milestone 1 — Abstractions + Manifest (COMPLETED)

**Status**: Complete

**Deliverables**:
- ✅ JD.Domain.Abstractions package with core contracts
- ✅ DomainManifest model with all manifest types
- ✅ Result<T> monad for functional error handling
- ✅ DomainError model with severity and metadata
- ✅ Core interfaces (IDomainEngine, IDomainFactory)
- ✅ RuleEvaluationResult and RuleEvaluationOptions
- ✅ Comprehensive unit tests (13 passing tests)

### 🔄 Milestone 2 — DSLs (IN PROGRESS)

**Estimated Effort**: 2-3 weeks

**Deliverables**:
- [ ] JD.Domain.Modeling package
  - Fluent DSL entry point: `Domain.Create(name)`
  - DomainBuilder with Entity<T>, ValueObject<T>, Enum<T>
  - Reflection-based model discovery
  - Type metadata extraction
- [ ] JD.Domain.Configuration package
  - Configuration DSL mirroring EF Core
  - Keys (primary, alternate)
  - Properties (required, length, precision, conversions)
  - Indexes (unique, filtered, included properties)
  - Relationships (one-to-many, one-to-one, many-to-many)
  - Inheritance (TPH/TPT/TPC)
  - Provider annotations
- [ ] JD.Domain.Rules package
  - Invariants, Validators, Policies, Derivations
  - State transitions
  - RuleContext support
  - Rule composition (Include, When)
  - Cross-entity rules
- [ ] Merge and precedence system
- [ ] Unit tests for all DSL packages

### 📋 Milestone 3 — Runtime

**Estimated Effort**: 2 weeks

**Deliverables**:
- [ ] JD.Domain.Runtime package
  - DomainRuntime.Create() implementation
  - Synchronous rule evaluation engine
  - Asynchronous rule evaluation engine
  - IDomainEngine implementation
  - IDomainFactory implementation
  - Construction pipeline with validation
- [ ] Telemetry integration
  - OpenTelemetry spans and events
  - Rule evaluation tracing
  - Performance metrics
  - Configurable enrichment
- [ ] Standalone entry points (non-DI usage)
- [ ] Unit tests and integration tests

### 📋 Milestone 4 — EF Core Adapter

**Estimated Effort**: 2-3 weeks

**Deliverables**:
- [ ] JD.Domain.EFCore package
  - ModelBuilder.ApplyDomainManifest() extension
  - Apply generated configurations
  - SaveChanges interceptors for invariant enforcement
  - Domain event emission (opt-in)
  - Conventions for domain mapping
  - Domain substitution mode support
- [ ] Mapper utilities (Domain ↔ EF entities)
- [ ] Round-trip model equivalence utilities
- [ ] Integration tests with SQLite, SQL Server, PostgreSQL

### 📋 Milestone 5 — Generators (Core)

**Estimated Effort**: 3-4 weeks

**Deliverables**:
- [ ] JD.Domain.EFCore.Generators package
  - Source generator: EF → JD extraction
    - Scan IEntityTypeConfiguration<T>
    - Scan DbContext.OnModelCreating
    - Generate configuration DSL partials
    - Generate rules DSL partials
  - Source generator: JD → EF emission
    - Generate IEntityTypeConfiguration<T>
    - Generate ModelBuilder extensions
- [ ] JD.Domain.Rules.Generators package
  - Reverse generator for rules from EF metadata
- [ ] Deterministic generation infrastructure
  - Stable file naming and ordering
  - Consistent formatting (Roslyn)
  - Version hash headers
  - Auto-generated markers
- [ ] Diagnostics catalog (JDxxxx error codes)
- [ ] Generator tests

### 📋 Milestone 6 — FluentValidation Generator

**Estimated Effort**: 1-2 weeks

**Deliverables**:
- [ ] JD.Domain.FluentValidation.Generator package
  - Source generator: JD → FluentValidation
  - Map Invariant rules to validator rules
  - Map Validator rules with proper selectors
  - Generate AbstractValidator<T> classes
  - Property path resolution
  - Custom error messages
- [ ] Integration with FluentValidation 11.x
- [ ] Generator tests

### 📋 Milestone 7 — Domain Model Generator

**Estimated Effort**: 3-4 weeks

**Deliverables**:
- [ ] JD.Domain.DomainModel.Generator package
  - Generate rich domain types (e.g., DomainBlog)
  - Construction-safe API with static Create methods
  - Immutable types (records or readonly properties)
  - Controlled-mutation variants (optional)
  - Integrated invariant enforcement
  - EF substitution mode support
    - Generate EF configurations for domain types
    - Private setters / backing fields
    - ValueConverter generation
  - Generate mappers (ToDomain/ToEf extensions)
  - Generate safe projections for queries
- [ ] Diagnostics for unsupported EF mapping scenarios
- [ ] Generator tests

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

**Milestone 1**: ✅ Complete (100%)  
**Overall Progress**: ~8% of total v1 scope

## Next Steps

1. Begin Milestone 2 by implementing JD.Domain.Modeling with the fluent DSL
2. Add reflection-based model discovery
3. Implement JD.Domain.Configuration with EF-compatible DSL
4. Implement JD.Domain.Rules with full rule categories

## Contributing

Given the scope, contributions are highly welcome! Areas where help is needed:

- DSL design and implementation
- Source generator expertise
- EF Core integration patterns
- Documentation and samples
- Testing and feedback

## Notes

This is an ambitious project with a clear vision. The modular architecture allows for incremental delivery and adoption. Each milestone can be released independently as preview packages.
