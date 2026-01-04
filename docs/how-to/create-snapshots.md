# Create Snapshots

Save domain state for version tracking.

## Goal
Create snapshots of domain manifests at specific points in time.

## Steps

```csharp
var writer = new SnapshotWriter();
var snapshot = writer.CreateSnapshot(manifest);
await storage.SaveAsync(snapshot, "v1.0.0.json");
```

## See Also
- [Version Management Tutorial](../tutorials/version-management.md)
- [Use CLI Tools](use-cli-tools.md)
