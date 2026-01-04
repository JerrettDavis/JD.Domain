# Apply to ModelBuilder

Apply domain configurations to EF Core DbContext.

## Goal
Integrate JD.Domain manifest with Entity Framework Core.

## Steps

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var manifest = MyDomain.Create();
    modelBuilder.ApplyDomainManifest(manifest);
}
```

## See Also
- [EF Core Integration Tutorial](../tutorials/ef-core-integration.md)
- [API: ModelBuilderExtensions](../../api/JD.Domain.EFCore.ModelBuilderExtensions.yml)
