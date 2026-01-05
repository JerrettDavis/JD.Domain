# Hybrid Workflow

This tutorial covers combining code-first and database-first approaches while tracking domain evolution with snapshots and diffs.

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

The sample uses the manifest source generator so the manifest stays DRY and aligned with the code-first types, then derives versioned manifests for snapshot comparisons.

## Key Concepts

### Snapshot Versioning
Track domain state at points in time using canonical JSON serialization.

### Breaking Change Detection
Automatically classify changes as breaking or non-breaking.

### Migration Planning
Generate step-by-step plans for migrating between versions.

## Sample Code

1) Enable the manifest generator in your project:
```xml
<ProjectReference Include="../../src/JD.Domain.ManifestGeneration/JD.Domain.ManifestGeneration.csproj" />
<ProjectReference Include="../../src/JD.Domain.ManifestGeneration.Generator/JD.Domain.ManifestGeneration.Generator.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

2) Add an assembly-level manifest attribute and mark entities:
```csharp
using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;

[assembly: GenerateManifest("UserManagement", Version = "1.1.0", Namespace = "JD.Domain.Samples.Hybrid")]

[DomainEntity]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = "";
}
```

3) Use the generated manifest to create versioned snapshots:
```csharp
var generated = UserManagementManifest.GeneratedManifest;
var v1 = CreateVersionedManifest(generated, new Version(1, 0, 0), entities =>
    entities.Where(e => e.Name != nameof(UserProfile)).ToList());

var v1_1 = CreateVersionedManifest(generated, new Version(1, 1, 0));
```

See `samples/JD.Domain.Samples.Hybrid/` for a complete working example.

## Workflow Steps

4) Create and store snapshots for each version:
```csharp
using JD.Domain.Snapshot;

var storage = new SnapshotStorage(new SnapshotOptions
{
    OutputDirectory = "DomainSnapshots"
});

storage.Save(v1);
storage.Save(v1_1);
```

5) Compare versions and classify breaking changes:
```csharp
using JD.Domain.Diff;

var diffEngine = new DiffEngine();
var diff = diffEngine.Compare(v1, v1_1);

Console.WriteLine($"Breaking changes: {diff.BreakingChanges.Count}");
```

6) Generate a migration plan:
```csharp
using JD.Domain.Diff;

var planner = new MigrationPlanGenerator();
var plan = planner.Generate(diff);

foreach (var step in plan.Steps)
{
    Console.WriteLine(step.Description);
}
```

If you prefer the CLI, see [Use CLI Tools](../how-to/use-cli-tools.md) for snapshot and diff commands.
