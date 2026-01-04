# Create Policies

Implement authorization and business policy rules.

## Goal

Create policy rules that enforce authorization and business constraints.

## Prerequisites

- JD.Domain.Rules package installed
- Understanding of validators

## Steps

### 1. Authorization Policy

```csharp
var rules = new RuleSetBuilder<Order>("Default")
    .Policy("CanCancel", (o, ctx) => 
        o.Status == OrderStatus.Pending && 
        o.CustomerId == ctx.User.Id)
    .WithMessage("You can only cancel pending orders you own")
    .Build();
```

### 2. Business Constraint Policy

```csharp
.Policy("CanPlaceOrder", (c, ctx) =>
    c.IsActive && 
    c.CreditLimit > 0 &&
    !c.HasOverduePayments)
.WithMessage("Customer cannot place orders due to account status")
```

### 3. Time-Based Policy

```csharp
.Policy("WithinBusinessHours", (o, ctx) =>
{
    var now = DateTime.Now;
    return now.Hour >= 9 && now.Hour < 17;
})
.WithMessage("Orders can only be placed during business hours (9 AM - 5 PM)")
```

## Result

Policies enable fine-grained authorization and business constraints that go beyond simple validation.

## Next Steps

- [Create Derivations](create-derivations.md) - Computed properties
- [Compose Rules](compose-rules.md) - Combine policies

## See Also

- [Business Rules Tutorial](../tutorials/business-rules.md)
