# Hybrid Workflow

**Status:** Coming Soon

This tutorial will cover combining code-first and database-first approaches while tracking domain evolution with snapshots and diffs.

## What You'll Learn

- Mixed code-first and database-first domain modeling
- Snapshot creation and versioning
- Breaking change detection with DiffEngine
- Migration planning workflow
- CI/CD integration with JD.Domain.Cli

## Prerequisites

- Completion of [Code-First Walkthrough](code-first-walkthrough.md)
- Completion of [Database-First Walkthrough](db-first-walkthrough.md)
- Understanding of both workflows

## Overview

The hybrid workflow allows you to:
- Keep some entities database-first (legacy tables)
- Define new entities code-first
- Track evolution with snapshots
- Detect breaking changes automatically
- Generate migration plans

## Key Concepts

### Snapshot Versioning
Track domain state at points in time using canonical JSON serialization.

### Breaking Change Detection
Automatically classify changes as breaking or non-breaking.

### Migration Planning
Generate step-by-step plans for migrating between versions.

## Sample Code

See `samples/JD.Domain.Samples.Hybrid/` for a complete working example.

## Coming Soon

This tutorial is under development. For now, refer to:
- [Version Management Tutorial](version-management.md) - Snapshots and diffs
- [Code-First Walkthrough](code-first-walkthrough.md) - Code-first approach
- [Database-First Walkthrough](db-first-walkthrough.md) - Database-first approach
