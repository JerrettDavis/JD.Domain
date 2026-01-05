# Domain Modeling

Master the domain modeling DSL for defining entities, value objects, and enums with properties, keys, and relationships.

**Time:** 45 minutes | **Level:** Beginner-Intermediate

## What You'll Learn

- Entity definitions with properties
- Value object patterns
- Enumeration types
- Composite keys and indexes
- Relationships and navigation properties

## Topics Covered

### Defining Entities
Learn how to use the fluent DSL to define entities:
```csharp
Domain.Create("MyDomain")
    .Entity<Customer>(e => e
        .Property(c => c.Id)
        .Property(c => c.Name)
        .Property(c => c.Email))
```

### Value Objects
Model value objects that represent domain concepts:
```csharp
.ValueObject<Address>(v => v
    .Property(a => a.Street)
    .Property(a => a.City)
    .Property(a => a.ZipCode))
```

### Enumerations
Define strongly-typed enumerations:
```csharp
.Enum<OrderStatus>(e => e
    .Value("Pending", 0)
    .Value("Processing", 1)
    .Value("Completed", 2))
```

### Configuration
Add EF Core configuration:
```csharp
.ConfigureEntity<Customer>(config => config
    .HasKey(c => c.Id)
    .ToTable("Customers")
    .HasIndex(c => c.Email, idx => idx.IsUnique()))
```

## Prerequisites

- Basic C# knowledge
- Understanding of domain modeling concepts
- Completion of [Quick Start](../getting-started/quick-start.md)

## Sample Code

See `samples/JD.Domain.Samples.CodeFirst/` for complete examples.

## API Reference

- [Domain](../../api/JD.Domain.Modeling.Domain.yml)
- [DomainBuilder](../../api/JD.Domain.Modeling.DomainBuilder.yml)
- [EntityBuilder](../../api/JD.Domain.Modeling.EntityBuilder-1.yml)

## Next Steps

- [Business Rules](business-rules.md) - Add validation rules
- [EF Core Integration](ef-core-integration.md) - Apply to DbContext
