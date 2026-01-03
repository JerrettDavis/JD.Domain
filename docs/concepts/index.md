# Concepts

Deep dives into JD.Domain's architecture, design principles, and core concepts.

## Architecture

- **[Architecture Overview](architecture.md)** - System design and package structure
- **[Design Principles](design-principles.md)** - Core philosophy: opt-in, modular, deterministic

## Core Concepts

- **[Domain Manifest](domain-manifest.md)** - The central domain description model
- **[DSL Overview](dsl-overview.md)** - Fluent DSL design philosophy
- **[Result Monad](result-monad.md)** - Result<T> pattern for error handling

## Rules & Validation

- **[Rule System](rule-system.md)** - Rule types and evaluation engine
- **[Runtime Engine](runtime-engine.md)** - How rules are evaluated
- **[Validation Errors](validation-errors.md)** - Error model and RFC 9457 ProblemDetails

## Code Generation

- **[Source Generators](source-generators.md)** - Generator architecture and extensibility
- **[Extensibility](extensibility.md)** - Extension points and customization

## Version Management

- **[Snapshot Format](snapshot-format.md)** - Canonical JSON serialization
- **[Diff Algorithm](diff-algorithm.md)** - How changes are detected
- **[Breaking Changes](breaking-changes.md)** - Change classification rules

## Understanding the Concepts

These documents explain the "why" and "how" behind JD.Domain's design. They're ideal for:

- **Architects** evaluating JD.Domain for their projects
- **Advanced users** wanting deep understanding
- **Contributors** looking to extend or improve JD.Domain

## See Also

- **[Getting Started](../getting-started/index.md)** - Quick introduction
- **[Tutorials](../tutorials/index.md)** - Step-by-step guides
- **[How-To Guides](../how-to/index.md)** - Task-oriented documentation
