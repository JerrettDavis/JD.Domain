# Define Value Objects

Learn how to define value object types that represent domain concepts without identity.

## Goal

Create value object definitions for complex types like Address, Money, or Email.

## Prerequisites

- JD.Domain.Modeling package installed
- Understanding of value object pattern

## Steps

### 1. Create the Value Object Class

Define a POCO with value semantics:

```csharp
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}
```

### 2. Define as Value Object

Use `.ValueObject<T>()` instead of `.Entity<T>()`:

```csharp
using JD.Domain.Modeling;

var domain = Domain.Create("MyDomain")
    .ValueObject<Address>(vo => vo
        .Property(a => a.Street)
        .Property(a => a.City)
        .Property(a => a.State)
        .Property(a => a.ZipCode))
    .Build();
```

### 3. Use in Entity

Reference value objects from entities:

```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address ShippingAddress { get; set; } = new();
    public Address BillingAddress { get; set; } = new();
}

var domain = Domain.Create("ECommerce")
    .ValueObject<Address>(vo => vo
        .Property(a => a.Street)
        .Property(a => a.City)
        .Property(a => a.State)
        .Property(a => a.ZipCode))
    .Entity<Customer>(e => e
        .Property(c => c.Id)
        .Property(c => c.Name)
        .Property(c => c.ShippingAddress)
        .Property(c => c.BillingAddress))
    .Build();
```

## Result

Value objects are treated differently than entities:
- No primary key
- Compared by value, not identity
- Can be embedded in entities
- EF Core configurations use owned types

## Next Steps

- **[Define Enums](define-enums.md)** - Add enumeration types
- **[Create Invariants](create-invariants.md)** - Add validation to value objects

## See Also

- [Domain Modeling Tutorial](../tutorials/domain-modeling.md)
- [API: ValueObjectBuilder](../../api/JD.Domain.Modeling.ValueObjectBuilder-1.yml)
