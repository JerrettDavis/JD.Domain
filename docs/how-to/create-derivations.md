# Create Derivations

Define computed properties and derived values.

## Goal

Create derivation rules that compute values from other properties.

## Prerequisites

- JD.Domain.Rules package installed

## Steps

### 1. Simple Derivation

```csharp
var rules = new RuleSetBuilder<Customer>("Default")
    .Derivation("FullName", c => $"{c.FirstName} {c.LastName}")
    .Build();
```

### 2. Complex Derivation

```csharp
.Derivation("TotalOrderValue", c => 
    c.Orders.Sum(o => o.TotalAmount))

.Derivation("MembershipLevel", c =>
{
    var total = c.Orders.Sum(o => o.TotalAmount);
    if (total > 10000) return "Gold";
    if (total > 5000) return "Silver";
    return "Bronze";
})
```

### 3. Use Derived Values

Generated domain types expose derived values as properties:

```csharp
var customer = DomainCustomer.Create(...);
Console.WriteLine(customer.FullName);
Console.WriteLine(customer.MembershipLevel);
```

## Result

Derivations encapsulate computed logic within the domain model, making it reusable and testable.

## Next Steps

- [Compose Rules](compose-rules.md) - Combine with other rules

## See Also

- [Business Rules Tutorial](../tutorials/business-rules.md)
