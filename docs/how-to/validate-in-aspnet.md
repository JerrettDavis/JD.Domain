# Validate in ASP.NET

Add automatic validation to ASP.NET Core APIs.

## Goal
Integrate JD.Domain validation with ASP.NET Core middleware.

## Steps

1. Add services: `builder.Services.AddDomainValidation()`
2. Add middleware: `app.UseDomainValidation()`
3. Use `[DomainValidation]` attribute or `.WithDomainValidation<T>()`

## See Also
- [ASP.NET Core Integration Tutorial](../tutorials/aspnet-core-integration.md)
