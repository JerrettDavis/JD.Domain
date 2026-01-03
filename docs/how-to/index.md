# How-To Guides

Task-oriented guides for accomplishing specific goals with JD.Domain. Each guide focuses on a single task with clear steps and working code examples.

## Domain Modeling

Learn how to define your domain model:

- **[Define Entities](define-entities.md)** - Create entity definitions with properties
- **[Define Value Objects](define-value-objects.md)** - Model value objects and complex types
- **[Define Enums](define-enums.md)** - Create enumeration types

## Business Rules

Learn how to create and use business rules:

- **[Create Invariants](create-invariants.md)** - Define always-true rules
- **[Create Validators](create-validators.md)** - Build context-dependent validation rules
- **[Create Policies](create-policies.md)** - Implement authorization and business policies
- **[Create Derivations](create-derivations.md)** - Define computed properties
- **[Compose Rules](compose-rules.md)** - Combine and reuse multiple rules

## Configuration

Learn how to configure EF Core integration:

- **[Configure Keys](configure-keys.md)** - Set up primary and composite keys
- **[Configure Indexes](configure-indexes.md)** - Create unique and filtered indexes
- **[Configure Relationships](configure-relationships.md)** - Define foreign keys and navigation

## Integration

Learn how to integrate with frameworks:

- **[Apply to ModelBuilder](apply-to-modelbuilder.md)** - Integrate with EF Core DbContext
- **[Validate in ASP.NET](validate-in-aspnet.md)** - Add automatic API validation

## Generators

Learn how to use source generators:

- **[Generate FluentValidation](generate-fluentvalidation.md)** - Auto-generate FluentValidation validators
- **[Generate Domain Types](generate-domain-types.md)** - Create construction-safe domain types

## Version Management

Learn how to track domain evolution:

- **[Create Snapshots](create-snapshots.md)** - Save domain state at points in time
- **[Compare Snapshots](compare-snapshots.md)** - Detect changes between versions
- **[Detect Breaking Changes](detect-breaking-changes.md)** - Identify breaking changes automatically
- **[Generate Migration Plans](generate-migration-plans.md)** - Create step-by-step migration guides

## Tooling

Learn how to use JD.Domain tools:

- **[Use CLI Tools](use-cli-tools.md)** - Command-line tools for snapshots and diffs
- **[Use T4 Templates](use-t4-templates.md)** - Integrate with T4 for code generation

## Guide Format

Each how-to guide follows this structure:

1. **Goal** - What you'll accomplish
2. **Prerequisites** - What you need before starting
3. **Steps** - Numbered steps with code examples
4. **Result** - What you should see
5. **Next Steps** - Related guides

## Finding the Right Guide

### By Task

- **I want to define my domain** → Domain Modeling guides
- **I want to add validation** → Business Rules guides
- **I want to use EF Core** → Configuration + Integration guides
- **I want to generate code** → Generator guides
- **I want to track changes** → Version Management guides
- **I want to use command-line tools** → Tooling guides

### By Experience Level

**Beginner:**
- Define Entities
- Create Invariants
- Apply to ModelBuilder
- Use CLI Tools

**Intermediate:**
- Define Value Objects
- Create Validators
- Configure Indexes
- Generate FluentValidation
- Create Snapshots

**Advanced:**
- Define Enums
- Create Policies
- Configure Relationships
- Generate Domain Types
- Generate Migration Plans

## See Also

- **[Tutorials](../tutorials/index.md)** - Step-by-step walkthroughs
- **[Concepts](../concepts/index.md)** - Deep dives into architecture
- **[API Reference](../../api/index.md)** - Complete API documentation
