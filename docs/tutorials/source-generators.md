# Source Generators

**Status:** Coming Soon

Use JD.Domain source generators to create rich domain types and FluentValidation validators automatically.

**Time:** 60 minutes | **Level:** Intermediate-Advanced

## What You'll Learn

- Domain model generator (construction-safe types)
- FluentValidation generator
- Generated code structure
- Customizing generator options
- Partial class extension points
- Troubleshooting generator issues

## Topics Covered

### Domain Model Generator
Generates construction-safe domain types:
```csharp
var result = DomainCustomer.Create(
    name: "John Doe",
    email: "john@example.com");

if (result.IsSuccess)
{
    var customer = result.Value;
}
```

### FluentValidation Generator
Auto-generates validators:
```csharp
public class CustomerValidator : AbstractValidator<Customer>
{
    // Generated from JD.Domain rules
}
```

### Generated Code Structure
- Static Create() methods
- FromEntity() for wrapping
- With*() mutation methods
- Implicit conversions

### Generator Options
```xml
<PropertyGroup>
    <JDDomainNamespace>MyApp.Domain</JDDomainNamespace>
    <JDDomainPrefix>Domain</JDDomainPrefix>
</PropertyGroup>
```

### Partial Class Extensions
Extend generated types:
```csharp
public partial class DomainCustomer
{
    public string GetFullName() => $"{FirstName} {LastName}";
}
```

## Prerequisites

- Understanding of source generators
- Completion of [Domain Modeling](domain-modeling.md)

## API Reference

- [DomainModelGenerator](../../api/JD.Domain.DomainModel.Generator.DomainModelGenerator.yml)
- [FluentValidationGenerator](../../api/JD.Domain.FluentValidation.Generator.FluentValidationGenerator.yml)

## Next Steps

- [Version Management](version-management.md)
