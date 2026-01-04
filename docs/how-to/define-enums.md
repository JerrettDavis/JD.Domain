# Define Enums

Learn how to define enumeration types for domain concepts.

## Goal

Create enum definitions that can be used in entities and generate EF Core configurations.

## Prerequisites

- JD.Domain.Modeling package installed

## Steps

### 1. Create the Enum

Define a C# enum:

```csharp
public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
```

### 2. Define in DSL

Use `.Enum<T>()` to describe the enum:

```csharp
using JD.Domain.Modeling;

var domain = Domain.Create("MyDomain")
    .Enum<OrderStatus>(e => e
        .Value("Pending", 0)
        .Value("Processing", 1)
        .Value("Shipped", 2)
        .Value("Delivered", 3)
        .Value("Cancelled", 4))
    .Build();
```

### 3. Use in Entity

Reference enums from entities:

```csharp
public class Order
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
}

var domain = Domain.Create("ECommerce")
    .Enum<OrderStatus>(e => e
        .Value("Pending", 0)
        .Value("Processing", 1)
        .Value("Shipped", 2)
        .Value("Delivered", 3)
        .Value("Cancelled", 4))
    .Entity<Order>(e => e
        .Property(o => o.Id)
        .Property(o => o.Status))
    .Build();
```

## Result

Enum definitions in the manifest enable:
- EF Core enum to string/int conversion configuration
- Strong typing in domain types
- Validation of enum values

## Next Steps

- **[Define Entities](define-entities.md)** - Use enums in entities
- **[Create Invariants](create-invariants.md)** - Validate enum values

## See Also

- [Domain Modeling Tutorial](../tutorials/domain-modeling.md)
- [API: EnumBuilder](../../api/JD.Domain.Modeling.EnumBuilder-1.yml)
