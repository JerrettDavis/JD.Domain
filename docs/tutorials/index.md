# Tutorials

Comprehensive step-by-step tutorials to help you master JD.Domain. Each tutorial includes working code examples, explanations, and best practices.

## Getting Started Tutorials

Start here if you're new to JD.Domain:

### [Code-First Walkthrough](code-first-walkthrough.md)
**Time:** 45-60 minutes | **Level:** Beginner

Build a complete e-commerce domain from scratch using JD.Domain's fluent DSL. Learn how to define entities, configure properties, add business rules, and generate EF Core configurations.

**What you'll build:**
- Customer and Order entities with rules
- EF Core DbContext with generated configurations
- Runtime validation engine
- Rich domain types with construction safety

**Prerequisites:** Basic C# and EF Core knowledge

---

### [Database-First Walkthrough](db-first-walkthrough.md)
**Time:** 30-45 minutes | **Level:** Beginner

Retrofit business rules onto existing EF Core scaffolded entities from a database. Learn how to add validation without modifying generated code.

**What you'll build:**
- Domain manifest from scaffolded entities
- Business rules for existing models
- FluentValidation validators (generated)
- ASP.NET Core API with automatic validation

**Prerequisites:** Existing EF Core project or database

---

### [Hybrid Workflow](hybrid-workflow.md)
**Time:** 60-90 minutes | **Level:** Intermediate

Combine code-first and database-first approaches while tracking domain evolution with snapshots and diffs.

**What you'll build:**
- Mixed code-first and database-first domain
- Snapshot versioning system
- Breaking change detection
- Migration planning workflow

**Prerequisites:** Understanding of both code-first and database-first workflows

---

## Feature-Specific Tutorials

Deep dives into specific JD.Domain features:

### [Domain Modeling](domain-modeling.md)
**Time:** 45 minutes | **Level:** Beginner-Intermediate

Master the domain modeling DSL for defining entities, value objects, and enums with properties, keys, and relationships.

**Topics covered:**
- Entity definitions with properties
- Value object patterns
- Enumeration types
- Composite keys and indexes
- Relationships and navigation properties

---

### [Business Rules](business-rules.md)
**Time:** 60 minutes | **Level:** Intermediate

Learn how to define, compose, and evaluate business rules including invariants, validators, policies, and derivations.

**Topics covered:**
- Invariants (always-true rules)
- Validators (context-dependent)
- Policies (authorization rules)
- Derivations (computed properties)
- Rule composition and reuse
- Conditional rules with `When`

---

### [EF Core Integration](ef-core-integration.md)
**Time:** 30 minutes | **Level:** Beginner-Intermediate

Integrate JD.Domain with Entity Framework Core to apply domain configurations to your DbContext.

**Topics covered:**
- ApplyDomainManifest extension
- Property configuration (required, max length)
- Index configuration (unique, filtered)
- Key configuration (primary, composite)
- Table mapping (name, schema)

---

### [ASP.NET Core Integration](aspnet-core-integration.md)
**Time:** 45 minutes | **Level:** Intermediate

Add automatic request validation to your ASP.NET Core APIs using JD.Domain middleware and endpoint filters.

**Topics covered:**
- Domain validation middleware
- Endpoint filters for Minimal APIs
- MVC action filters
- ProblemDetails responses (RFC 9457)
- Custom error handling
- DomainContext for user/tenant context

---

### [Source Generators](source-generators.md)
**Time:** 60 minutes | **Level:** Intermediate-Advanced

Use JD.Domain source generators to create rich domain types and FluentValidation validators automatically.

**Topics covered:**
- Domain model generator (construction-safe types)
- FluentValidation generator
- Generated code structure
- Customizing generator options
- Partial class extension points
- Troubleshooting generator issues

---

### [Version Management](version-management.md)
**Time:** 45 minutes | **Level:** Intermediate

Track domain evolution over time using snapshots, compare versions with diffs, and generate migration plans.

**Topics covered:**
- Creating domain snapshots
- Comparing snapshots with DiffEngine
- Breaking vs. non-breaking changes
- Generating migration plans
- CLI tools for CI/CD integration
- Canonical JSON serialization

---

## Tutorial Series

Follow these series for comprehensive learning:

### Beginner Series (3-4 hours total)

Perfect for developers new to JD.Domain:

1. **[Code-First Walkthrough](code-first-walkthrough.md)** - Foundations
2. **[Domain Modeling](domain-modeling.md)** - Deep dive into DSL
3. **[Business Rules](business-rules.md)** - Rule system mastery
4. **[EF Core Integration](ef-core-integration.md)** - Database integration

**Outcome:** Build production-ready domain models with validation

---

### Database Modernization Series (2-3 hours total)

For teams working with existing databases:

1. **[Database-First Walkthrough](db-first-walkthrough.md)** - Add rules to legacy code
2. **[ASP.NET Core Integration](aspnet-core-integration.md)** - API validation
3. **[Source Generators](source-generators.md)** - Generate rich wrappers
4. **[Hybrid Workflow](hybrid-workflow.md)** - Gradual migration path

**Outcome:** Modernize legacy applications incrementally

---

### Advanced Series (3-4 hours total)

For teams managing complex domains:

1. **[Hybrid Workflow](hybrid-workflow.md)** - Mixed approaches
2. **[Version Management](version-management.md)** - Track evolution
3. **[Source Generators](source-generators.md)** - Code generation
4. **[ASP.NET Core Integration](aspnet-core-integration.md)** - Full stack integration

**Outcome:** Enterprise-grade domain management

---

## Sample Applications

Each tutorial references these working sample applications:

### JD.Domain.Samples.CodeFirst
Complete code-first workflow demonstration with:
- Customer and Order entities
- Business rules and validation
- EF Core integration
- Generated domain types

**Location:** `samples/JD.Domain.Samples.CodeFirst/`

---

### JD.Domain.Samples.DbFirst
Database-first workflow demonstration with:
- Scaffolded Blog and Post entities
- Added business rules
- FluentValidation generation
- ASP.NET Core API

**Location:** `samples/JD.Domain.Samples.DbFirst/`

---

### JD.Domain.Samples.Hybrid
Hybrid workflow demonstration with:
- Mixed code-first and database-first entities
- Snapshot versioning
- Diff comparison
- Migration planning

**Location:** `samples/JD.Domain.Samples.Hybrid/`

---

## Tutorial Format

Each tutorial follows a consistent structure:

1. **Overview** - What you'll learn and build
2. **Prerequisites** - Required knowledge and tools
3. **Setup** - Project creation and package installation
4. **Step-by-Step Instructions** - Detailed walkthrough with code
5. **Explanation** - Concepts and design decisions
6. **Testing** - Verify it works
7. **Summary** - Key takeaways
8. **Next Steps** - Where to go from here

## How to Use These Tutorials

### If You're New to JD.Domain

Start with the **[Code-First Walkthrough](code-first-walkthrough.md)** to understand core concepts, then explore feature-specific tutorials based on your needs.

### If You Have an Existing Project

Start with the **[Database-First Walkthrough](db-first-walkthrough.md)** to learn how to add JD.Domain to existing code without major refactoring.

### If You're Evaluating JD.Domain

Follow the **[Quick Start](../getting-started/quick-start.md)** (5 minutes) to get a taste, then try the **[Code-First Walkthrough](code-first-walkthrough.md)** (60 minutes) for a complete picture.

### If You're Building Production Systems

Complete the **Beginner Series**, then follow the **Advanced Series** to learn best practices for enterprise scenarios.

## Additional Learning Resources

- **[Getting Started](../getting-started/index.md)** - Installation and quick start
- **[How-To Guides](../how-to/index.md)** - Task-oriented guides
- **[Concepts](../concepts/index.md)** - Deep dives into architecture
- **[API Reference](../../api/index.md)** - Complete API documentation

## Get Help

- **Questions?** Open a [GitHub Issue](https://github.com/JerrettDavis/JD.Domain/issues)
- **Found a bug?** Report it on [GitHub](https://github.com/JerrettDavis/JD.Domain/issues)
- **Want to contribute?** See our [Contributing Guide](../contributing/index.md)

---

Let's get started! Choose your first tutorial above and begin building with JD.Domain.
