# Configure Indexes

Create database indexes.

## Goal
Configure unique and filtered indexes.

## Steps
1. Simple: `.HasIndex(c => c.Email)`
2. Unique: `.HasIndex(c => c.Email, idx => idx.IsUnique())`
3. Filtered: `.HasIndex(c => c.Email, idx => idx.HasFilter("IsActive = 1"))`

See [EF Core Integration Tutorial](../tutorials/ef-core-integration.md)
