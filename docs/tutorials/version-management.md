# Version Management

**Status:** Coming Soon

Track domain evolution over time using snapshots, compare versions with diffs, and generate migration plans.

**Time:** 45 minutes | **Level:** Intermediate

## What You'll Learn

- Creating domain snapshots
- Comparing snapshots with DiffEngine
- Breaking vs. non-breaking changes
- Generating migration plans
- CLI tools for CI/CD integration
- Canonical JSON serialization

## Topics Covered

### Creating Snapshots
```csharp
var writer = new SnapshotWriter();
var snapshot = writer.CreateSnapshot(manifest);
await snapshot.SaveAsync("snapshots/v1.json");
```

### Comparing Snapshots
```csharp
var diff = diffEngine.Compare(snapshotV1, snapshotV2);
Console.WriteLine($"Breaking changes: {diff.HasBreakingChanges}");
```

### CLI Tools
```bash
# Create snapshot
jd-domain snapshot --manifest domain.json --output ./snapshots

# Compare versions
jd-domain diff v1.json v2.json --format md

# Generate migration plan
jd-domain migrate-plan v1.json v2.json --output plan.md
```

### Breaking Changes
Automatically detected:
- Removed entities/properties
- Changed types
- Removed required fields
- Changed keys

### Migration Plans
Generated step-by-step:
1. Add new properties
2. Migrate data
3. Remove old properties

## Prerequisites

- Understanding of versioning concepts
- Completion of [Domain Modeling](domain-modeling.md)

## API Reference

- [SnapshotWriter](../../api/JD.Domain.Snapshot.SnapshotWriter.yml)
- [DiffEngine](../../api/JD.Domain.Diff.DiffEngine.yml)
- [MigrationPlanGenerator](../../api/JD.Domain.Diff.MigrationPlanGenerator.yml)

## Next Steps

- [Hybrid Workflow](hybrid-workflow.md)
