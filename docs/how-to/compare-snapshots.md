# Compare Snapshots

Detect changes between domain versions.

## Goal
Compare two snapshots to identify what changed.

## Steps

```csharp
var diffEngine = new DiffEngine();
var diff = diffEngine.Compare(snapshotV1, snapshotV2);
Console.WriteLine($"Breaking changes: {diff.HasBreakingChanges}");
```

## See Also
- [Version Management Tutorial](../tutorials/version-management.md)
