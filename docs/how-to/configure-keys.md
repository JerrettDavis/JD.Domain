# Configure Keys

Configure primary and composite keys.

## Goal
Set up entity keys using JD.Domain configuration DSL.

## Steps
1. Primary key: `.HasKey(c => c.Id)`
2. Composite key: `.HasKey(oi => new { oi.OrderId, oi.ProductId })`
3. Alternate key: `.HasAlternateKey(c => c.Email)`

See [EF Core Integration Tutorial](../tutorials/ef-core-integration.md)
