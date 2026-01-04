# Next Steps

Congratulations on getting started with JD.Domain! This guide helps you plan your learning journey based on your workflow and goals.

## Learning Paths

Choose your learning path based on the workflow you selected:

### Path 1: Code-First Development

For developers building new applications with domain-driven design:

#### Foundation (1-2 hours)
1. ✅ **[Quick Start](quick-start.md)** - You've completed this!
2. 📖 **[Domain Modeling Tutorial](../tutorials/domain-modeling.md)** - Learn the modeling DSL
3. 📖 **[Business Rules Tutorial](../tutorials/business-rules.md)** - Define invariants, validators, and policies

#### Integration (2-3 hours)
4. 📖 **[EF Core Integration](../tutorials/ef-core-integration.md)** - Apply configurations to DbContext
5. 📖 **[ASP.NET Core Integration](../tutorials/aspnet-core-integration.md)** - Add middleware and endpoint filters

#### Advanced (3-4 hours)
6. 📖 **[Source Generators](../tutorials/source-generators.md)** - Generate rich domain types
7. 📖 **[Version Management](../tutorials/version-management.md)** - Track domain evolution with snapshots

**Total Time:** 6-9 hours

**Next:** [Code-First Walkthrough](../tutorials/code-first-walkthrough.md)

### Path 2: Database-First Development

For developers working with existing databases:

#### Foundation (1-2 hours)
1. ✅ **[Quick Start](quick-start.md)** - You've completed this!
2. 📖 **[Business Rules Tutorial](../tutorials/business-rules.md)** - Add rules to existing entities
3. 📖 **[Database-First Walkthrough](../tutorials/db-first-walkthrough.md)** - Complete example with scaffolded entities

#### Integration (2-3 hours)
4. 📖 **[ASP.NET Core Integration](../tutorials/aspnet-core-integration.md)** - Validate API requests
5. 📖 **[Source Generators](../tutorials/source-generators.md)** - Generate rich wrappers for EF entities

#### Advanced (2-3 hours)
6. 📖 **[Hybrid Workflow](../tutorials/hybrid-workflow.md)** - Mix code-first and database-first
7. 📖 **[Version Management](../tutorials/version-management.md)** - Track schema evolution

**Total Time:** 5-8 hours

**Next:** [Database-First Walkthrough](../tutorials/db-first-walkthrough.md)

### Path 3: Hybrid/Migration

For teams migrating from anemic models or legacy systems:

#### Foundation (2-3 hours)
1. ✅ **[Quick Start](quick-start.md)** - You've completed this!
2. 📖 **[Database-First Walkthrough](../tutorials/db-first-walkthrough.md)** - Start with existing database
3. 📖 **[Business Rules Tutorial](../tutorials/business-rules.md)** - Add rules to legacy code

#### Version Management (3-4 hours)
4. 📖 **[Snapshot and Diff](../tutorials/version-management.md)** - Create snapshots of current state
5. 📖 **[Hybrid Workflow](../tutorials/hybrid-workflow.md)** - Mix old and new approaches
6. 📖 **[Migration from Anemic Models](../migration/from-anemic-models.md)** - Gradual migration strategy

#### Modernization (3-4 hours)
7. 📖 **[Domain Modeling Tutorial](../tutorials/domain-modeling.md)** - Rebuild critical domains in DSL
8. 📖 **[Source Generators](../tutorials/source-generators.md)** - Generate construction-safe types

**Total Time:** 8-11 hours

**Next:** [Hybrid Workflow Tutorial](../tutorials/hybrid-workflow.md)

## Topic-Based Learning

Prefer to learn specific topics? Browse by feature:

### Domain Modeling

Learn how to define entities, value objects, and enums:

- **[Define Entities](../how-to/define-entities.md)** - Create entity definitions
- **[Define Value Objects](../how-to/define-value-objects.md)** - Model value objects
- **[Define Enums](../how-to/define-enums.md)** - Create enumeration types
- **[Domain Modeling Concepts](../concepts/dsl-overview.md)** - Deep dive into DSL design

**Time:** 2-3 hours

### Business Rules

Learn how to create and compose rules:

- **[Create Invariants](../how-to/create-invariants.md)** - Always-true rules
- **[Create Validators](../how-to/create-validators.md)** - Context-dependent validation
- **[Create Policies](../how-to/create-policies.md)** - Authorization and business policies
- **[Compose Rules](../how-to/compose-rules.md)** - Combine multiple rules
- **[Rule System Concepts](../concepts/rule-system.md)** - Deep dive into rule engine

**Time:** 3-4 hours

### EF Core Integration

Learn how to integrate with Entity Framework Core:

- **[Apply to ModelBuilder](../how-to/apply-to-modelbuilder.md)** - EF Core integration
- **[Configure Keys](../how-to/configure-keys.md)** - Primary and composite keys
- **[Configure Indexes](../how-to/configure-indexes.md)** - Index creation
- **[Configure Relationships](../how-to/configure-relationships.md)** - Foreign keys and navigation
- **[EF Core Integration Tutorial](../tutorials/ef-core-integration.md)** - Complete walkthrough

**Time:** 2-3 hours

### ASP.NET Core Integration

Learn how to validate requests in web APIs:

- **[Validate in ASP.NET](../how-to/validate-in-aspnet.md)** - Middleware and filters
- **[ASP.NET Core Tutorial](../tutorials/aspnet-core-integration.md)** - Complete walkthrough
- **[Validation Errors Concept](../concepts/validation-errors.md)** - Error model deep dive

**Time:** 2-3 hours

### Source Generators

Learn how to generate code from domain definitions:

- **[Generate FluentValidation](../how-to/generate-fluentvalidation.md)** - Validator generation
- **[Generate Domain Types](../how-to/generate-domain-types.md)** - Rich type generation
- **[Source Generators Tutorial](../tutorials/source-generators.md)** - Complete walkthrough
- **[Generator Architecture](../concepts/source-generators.md)** - Deep dive

**Time:** 2-3 hours

### Version Management

Learn how to track domain evolution:

- **[Create Snapshots](../how-to/create-snapshots.md)** - Snapshot creation
- **[Compare Snapshots](../how-to/compare-snapshots.md)** - Diff comparison
- **[Detect Breaking Changes](../how-to/detect-breaking-changes.md)** - Change classification
- **[Use CLI Tools](../how-to/use-cli-tools.md)** - Command-line tools
- **[Snapshot Format Concept](../concepts/snapshot-format.md)** - Deep dive

**Time:** 2-3 hours

## Hands-On Practice

### Sample Applications

Explore working examples in the repository:

1. **[Code-First Sample](../../samples/JD.Domain.Samples.CodeFirst/)**
   - Complete code-first workflow
   - Domain modeling with DSL
   - EF Core integration
   - ASP.NET Core validation

2. **[Database-First Sample](../../samples/JD.Domain.Samples.DbFirst/)**
   - Scaffolded EF entities
   - Added business rules
   - Rich domain type generation
   - Runtime validation

3. **[Hybrid Sample](../../samples/JD.Domain.Samples.Hybrid/)**
   - Mixed code-first and database-first
   - Snapshot creation and comparison
   - Migration planning
   - Version tracking

### Build Your Own Project

Apply what you've learned to your own project:

1. **Start Small** - Pick one entity or aggregate
2. **Add Rules** - Define basic invariants
3. **Validate** - Test runtime validation
4. **Expand** - Add more entities and rules
5. **Generate** - Try source generators
6. **Integrate** - Add ASP.NET Core middleware

## Reference Materials

Keep these handy as you develop:

### Essential Documentation

- **[API Reference](../../api/index.md)** - Complete API documentation
- **[Package Matrix](../reference/package-matrix.md)** - Package comparison table
- **[CLI Commands Reference](../reference/cli-commands.md)** - Command-line tool usage
- **[Error Codes](../reference/error-codes.md)** - Error catalog

### Conceptual Documentation

- **[Architecture Overview](../concepts/architecture.md)** - System design
- **[Design Principles](../concepts/design-principles.md)** - Core philosophy
- **[Domain Manifest](../concepts/domain-manifest.md)** - Central model
- **[Result Monad](../concepts/result-monad.md)** - Result<T> pattern

### Advanced Topics

- **[Performance Optimization](../advanced/performance.md)** - Tuning and optimization
- **[Telemetry Integration](../advanced/telemetry.md)** - OpenTelemetry support
- **[Custom Generators](../advanced/custom-generators.md)** - Build your own
- **[Integration Patterns](../advanced/integration-patterns.md)** - Framework integration

## Common Next Steps

Based on where you are in your journey:

### If You Just Finished Quick Start

**Recommended:**
1. Choose your workflow: [Choose Your Workflow](choose-workflow.md)
2. Follow the appropriate tutorial
3. Explore sample applications

### If You're Evaluating JD.Domain

**Recommended:**
1. Review [Architecture Overview](../concepts/architecture.md)
2. Check [Design Principles](../concepts/design-principles.md)
3. Compare with [Migration from FluentValidation](../migration/from-fluentvalidation.md)

### If You're Ready to Build

**Recommended:**
1. Follow workflow-specific tutorial
2. Reference [How-To Guides](../how-to/index.md)
3. Use [API Reference](../../api/index.md)

### If You're Migrating

**Recommended:**
1. Read [Migration from Anemic Models](../migration/from-anemic-models.md)
2. Follow [Hybrid Workflow Tutorial](../tutorials/hybrid-workflow.md)
3. Use [Version Management Tools](../how-to/create-snapshots.md)

## Skill Level Roadmap

### Beginner (0-10 hours)

**Goals:**
- Understand opt-in architecture
- Define simple business rules
- Validate entities at runtime
- Integrate with EF Core or ASP.NET Core

**Resources:**
- Getting Started guides (this section)
- Basic tutorials
- Simple how-to guides

### Intermediate (10-30 hours)

**Goals:**
- Model complex domains with DSL
- Use source generators effectively
- Track domain evolution with snapshots
- Compose and reuse rule sets

**Resources:**
- Advanced tutorials
- Conceptual documentation
- Sample applications
- Reference documentation

### Advanced (30+ hours)

**Goals:**
- Build custom generators
- Optimize for high-performance scenarios
- Integrate with custom frameworks
- Contribute to JD.Domain

**Resources:**
- Advanced topics
- Source code exploration
- Contributing guidelines
- Architecture deep dives

## Getting Help

### Documentation

- **Search** - Use the search feature in the documentation site
- **API Reference** - Browse complete API documentation
- **Samples** - Check the working examples

### Community

- **GitHub Issues** - Ask questions or report bugs
- **Discussions** - Share ideas and get feedback

### Best Practices

- **Start simple** - Don't try to use every feature at once
- **Follow examples** - Use sample applications as templates
- **Ask questions** - Don't hesitate to open a GitHub issue
- **Share feedback** - Help improve JD.Domain

## Summary

You've completed the Getting Started section! Here's what you've learned:

✅ What JD.Domain is and why you should use it
✅ How to install required packages
✅ How to build your first domain model
✅ How to choose the right workflow for your project

**Your next steps depend on your workflow:**

- **Code-First:** [Code-First Walkthrough](../tutorials/code-first-walkthrough.md)
- **Database-First:** [Database-First Walkthrough](../tutorials/db-first-walkthrough.md)
- **Hybrid:** [Hybrid Workflow Tutorial](../tutorials/hybrid-workflow.md)

## Additional Resources

### Keep Learning

- 📚 **[Tutorials](../tutorials/index.md)** - Step-by-step guides
- 🎯 **[How-To Guides](../how-to/index.md)** - Task-oriented documentation
- 🧠 **[Concepts](../concepts/index.md)** - Deep dives into architecture
- 📖 **[Reference](../reference/index.md)** - Complete reference material

### Stay Updated

- 📋 **[Changelog](../changelog/index.md)** - Version history
- 🗺️ **[Roadmap](../changelog/roadmap.md)** - Future plans
- 🤝 **[Contributing](../contributing/index.md)** - How to contribute

Happy coding with JD.Domain!
