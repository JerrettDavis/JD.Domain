# Generate Domain Types

Create construction-safe domain types with Result<T>.

## Goal
Generate rich domain types that enforce invariants.

## Steps

1. Install: `dotnet add package JD.Domain.DomainModel.Generator`
2. Build project
3. Use: `DomainCustomer.Create(...)` returns `Result<DomainCustomer>`

## See Also
- [Source Generators Tutorial](../tutorials/source-generators.md)
