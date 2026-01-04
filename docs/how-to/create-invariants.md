# Create Invariants

Define always-true rules that validate entity state.

## Goal

Create invariant rules that must always be true for an entity to be in a valid state.

## Prerequisites

- JD.Domain.Rules package installed
- Entity definitions created

## Steps

### 1. Create Rule Set Builder

```csharp
using JD.Domain.Rules;

var rules = new RuleSetBuilder<Customer>("Default")
```

### 2. Add Invariant Rules

```csharp
var rules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
    .WithMessage("Customer name is required")
    
    .Invariant("Email.Format", c => c.Email.Contains("@"))
    .WithMessage("Email must be valid")
    
    .Build();
```

### 3. Evaluate Rules

```csharp
using JD.Domain.Runtime;

var engine = DomainRuntime.CreateEngine(manifest);
var result = engine.Evaluate(customer, rules);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error.Message);
    }
}
```

## Result

Invariant rules enforce entity validity at runtime and during construction of generated domain types.

## Next Steps

- [Create Validators](create-validators.md) - Context-dependent rules
- [Compose Rules](compose-rules.md) - Combine rule sets

## See Also

- [Business Rules Tutorial](../tutorials/business-rules.md)
