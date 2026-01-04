# Create Validators

Define context-dependent validation rules.

## Goal

Create validator rules that depend on external context or perform async operations.

## Prerequisites

- JD.Domain.Rules package installed
- Understanding of invariants

## Steps

### 1. Create Synchronous Validator

```csharp
var rules = new RuleSetBuilder<Customer>("Default")
    .Validator("Email.Unique", c => CheckEmailUnique(c.Email))
    .WithMessage("Email already exists")
    .Build();

bool CheckEmailUnique(string email)
{
    // Check database, external service, etc.
    return !existingEmails.Contains(email);
}
```

### 2. Create Async Validator

```csharp
var rules = new RuleSetBuilder<Customer>("Default")
    .Validator("Email.Unique", async (c, ctx) => 
        await _repository.IsEmailUniqueAsync(c.Email))
    .WithMessage("Email already exists")
    .Build();
```

### 3. Use Context Parameter

```csharp
.Validator("CanModify", (c, ctx) => 
    c.OwnerId == ctx.User.Id)
.WithMessage("You can only modify your own customer record")
```

## Result

Validators enable complex validation that depends on external state or requires I/O operations.

## Next Steps

- [Create Policies](create-policies.md) - Authorization rules
- [Compose Rules](compose-rules.md) - Combine validators

## See Also

- [Business Rules Tutorial](../tutorials/business-rules.md)
