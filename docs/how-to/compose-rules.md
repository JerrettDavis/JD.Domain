# Compose Rules

Combine and reuse multiple rule sets.

## Goal

Learn how to compose rule sets using Include and When for reusability.

## Prerequisites

- JD.Domain.Rules package installed
- Understanding of rule types

## Steps

### 1. Include Base Rules

```csharp
// Base rules
var baseRules = new RuleSetBuilder<Customer>("Base")
    .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
    .Build();

// Extended rules
var extendedRules = new RuleSetBuilder<Customer>("Extended")
    .Include(baseRules)
    .Invariant("Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
    .Build();
```

### 2. Conditional Rules with When

```csharp
var rules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
    .When(c => c.IsPremiumCustomer)
    .WithMessage("Premium customers must have an email")
    
    .Invariant("Phone.Required", c => !string.IsNullOrWhiteSpace(c.Phone))
    .When(c => c.RequiresPhoneVerification)
    .WithMessage("Phone number is required for verification")
    
    .Build();
```

### 3. Rule Set Families

```csharp
public static class CustomerRules
{
    public static RuleSetManifest Minimal() => 
        new RuleSetBuilder<Customer>("Minimal")
            .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
            .Build();
    
    public static RuleSetManifest Standard() =>
        new RuleSetBuilder<Customer>("Standard")
            .Include(Minimal())
            .Invariant("Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
            .Build();
    
    public static RuleSetManifest Premium() =>
        new RuleSetBuilder<Customer>("Premium")
            .Include(Standard())
            .Invariant("Phone.Required", c => !string.IsNullOrWhiteSpace(c.Phone))
            .Build();
}
```

## Result

Rule composition enables:
- DRY (Don't Repeat Yourself) principle
- Progressive validation levels
- Context-specific rule application

## Next Steps

- [Validate in ASP.NET](validate-in-aspnet.md) - Use composed rules in APIs

## See Also

- [Business Rules Tutorial](../tutorials/business-rules.md)
