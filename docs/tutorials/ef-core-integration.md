# EF Core Integration

**Status:** Coming Soon

Integrate JD.Domain with Entity Framework Core to apply domain configurations to your DbContext.

**Time:** 30 minutes | **Level:** Beginner-Intermediate

## What You'll Learn

- ApplyDomainManifest extension
- Property configuration (required, max length)
- Index configuration (unique, filtered)
- Key configuration (primary, composite)
- Table mapping (name, schema)

## Topics Covered

### Basic Integration
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var manifest = MyDomain.Create();
    modelBuilder.ApplyDomainManifest(manifest);
}
```

### Property Configuration
- Required fields
- Max length constraints
- Precision for decimals
- Default values

### Index Configuration
- Unique indexes
- Composite indexes
- Filtered indexes
- Included columns

### Key Configuration
- Primary keys
- Composite keys
- Alternate keys

### Table Mapping
- Table names
- Schema names
- View mapping

## Prerequisites

- Basic EF Core knowledge
- Completion of [Domain Modeling](domain-modeling.md)

## API Reference

- [ModelBuilderExtensions](../../api/JD.Domain.EFCore.ModelBuilderExtensions.yml)

## Next Steps

- [ASP.NET Core Integration](aspnet-core-integration.md)
- [Version Management](version-management.md)
