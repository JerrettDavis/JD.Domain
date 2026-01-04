# Define Entities

Learn how to define entity types using JD.Domain's fluent DSL.

## Goal

Create an entity definition with properties that can be used for EF Core configuration and code generation.

## Prerequisites

- JD.Domain.Modeling package installed
- Basic understanding of domain modeling

## Steps

### 1. Create the Entity Class

Define a simple POCO class:

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### 2. Define the Entity in DSL

Use the fluent DSL to describe the entity:

```csharp
using JD.Domain.Modeling;

var domain = Domain.Create("MyDomain")
    .Entity<Customer>(entity => entity
        .Property(c => c.Id)
        .Property(c => c.Name)
        .Property(c => c.Email)
        .Property(c => c.CreatedAt))
    .Build();
```

### 3. Add Multiple Entities

Define multiple entities in one domain:

```csharp
var domain = Domain.Create("ECommerce")
    .Entity<Customer>(e => e
        .Property(c => c.Id)
        .Property(c => c.Name)
        .Property(c => c.Email))
    .Entity<Order>(e => e
        .Property(o => o.Id)
        .Property(o => o.CustomerId)
        .Property(o => o.TotalAmount))
    .Build();
```

## Result

You now have a domain manifest describing your entities. This manifest can be used for:
- Generating EF Core configurations
- Creating business rules
- Generating domain types
- Creating snapshots

## Next Steps

- **[Define Value Objects](define-value-objects.md)** - Add value object types
- **[Configure Keys](configure-keys.md)** - Add primary keys
- **[Create Invariants](create-invariants.md)** - Add business rules

## See Also

- [Domain Modeling Tutorial](../tutorials/domain-modeling.md)
- [API: EntityBuilder](../../api/JD.Domain.Modeling.EntityBuilder-1.yml)
