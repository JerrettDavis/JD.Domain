# ASP.NET Core Integration

**Status:** Coming Soon

Add automatic request validation to your ASP.NET Core APIs using JD.Domain middleware and endpoint filters.

**Time:** 45 minutes | **Level:** Intermediate

## What You'll Learn

- Domain validation middleware
- Endpoint filters for Minimal APIs
- MVC action filters
- ProblemDetails responses (RFC 9457)
- Custom error handling
- DomainContext for user/tenant context

## Topics Covered

### Middleware Setup
```csharp
builder.Services.AddDomainValidation(options =>
{
    options.AddManifest(manifest);
});

app.UseDomainValidation();
```

### Minimal API Integration
```csharp
app.MapPost("/api/customers", (Customer customer) => ...)
    .WithDomainValidation<Customer>();
```

### MVC Integration
```csharp
[HttpPost]
[DomainValidation]
public IActionResult Create(Customer customer)
{
    // Validation happens automatically
}
```

### Error Responses
RFC 9457 ProblemDetails format:
```json
{
  "type": "https://tools.ietf.org/html/rfc9457",
  "title": "Validation Failed",
  "status": 400,
  "errors": [...]
}
```

### DomainContext
Access user/tenant context in rules:
```csharp
.Policy("CanEdit", (entity, ctx) =>
    ctx.User.Id == entity.OwnerId)
```

## Prerequisites

- ASP.NET Core knowledge
- Completion of [Business Rules](business-rules.md)

## API Reference

- [DomainValidationMiddleware](../../api/JD.Domain.AspNetCore.DomainValidationMiddleware.yml)
- [DomainValidationAttribute](../../api/JD.Domain.AspNetCore.DomainValidationAttribute.yml)

## Next Steps

- [Source Generators](source-generators.md)
