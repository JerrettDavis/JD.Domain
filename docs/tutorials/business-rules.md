# Business Rules

Learn how to define, compose, and evaluate business rules including invariants, validators, policies, and derivations.

**Time:** 60 minutes | **Level:** Intermediate

## What You'll Learn

- Invariants (always-true rules)
- Validators (context-dependent)
- Policies (authorization rules)
- Derivations (computed properties)
- Rule composition and reuse
- Conditional rules with `When`

## Topics Covered

### Invariants
Define always-true rules:
```csharp
new RuleSetBuilder<Customer>("Default")
    .Invariant("Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
    .WithMessage("Email is required")
```

### Validators
Context-dependent validation:
```csharp
.Validator("Email.Unique", async (c, ctx) =>
    await IsEmailUniqueAsync(c.Email))
.WithMessage("Email already exists")
```

### Policies
Authorization and business policies:
```csharp
.Policy("CanPlaceOrder", (c, ctx) =>
    c.IsActive && c.CreditLimit > 0)
.WithMessage("Customer cannot place orders")
```

### Derivations
Computed properties:
```csharp
.Derivation("FullName", c => $"{c.FirstName} {c.LastName}")
```

### Rule Composition
Reuse and combine rules:
```csharp
.Include(BaseCustomerRules.Default())
.When(c => c.IsPremium)
```

## Prerequisites

- Completion of [Domain Modeling](domain-modeling.md)
- Understanding of validation concepts

## API Reference

- [RuleSetBuilder](../../api/JD.Domain.Rules.RuleSetBuilder-1.yml)
- [CompiledRuleSet](../../api/JD.Domain.Rules.CompiledRuleSet-1.yml)

## Next Steps

- [EF Core Integration](ef-core-integration.md)
- [ASP.NET Core Integration](aspnet-core-integration.md)
