# Detect Breaking Changes

Identify breaking vs. non-breaking changes.

## Goal
Classify changes to plan safe migrations.

## Steps

```csharp
var classifier = new BreakingChangeClassifier();
var breaking = diff.Changes.Where(c => classifier.IsBreaking(c));
```

## See Also
- [Version Management Tutorial](../tutorials/version-management.md)
