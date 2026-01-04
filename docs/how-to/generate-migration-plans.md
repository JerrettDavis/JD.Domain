# Generate Migration Plans

Create step-by-step migration guides.

## Goal
Generate detailed migration plans between versions.

## Steps

```csharp
var planGenerator = new MigrationPlanGenerator();
var plan = planGenerator.Generate(diff);
var markdown = plan.ToMarkdown();
```

## See Also
- [Version Management Tutorial](../tutorials/version-management.md)
