# Sample Applications

Explore working examples demonstrating different workflows and features of JD.Domain.

## Available Samples

### Code-First Sample

**Location:** `samples/JD.Domain.Samples.CodeFirst`

Demonstrates building a domain model from scratch using the fluent DSL.

**Features:**
- Entity and value object definitions
- Business rules (invariants, validators, policies)
- EF Core integration with `ApplyDomainManifest()`
- ASP.NET Core validation middleware
- Runtime rule evaluation

**Key Files:**
- `BloggingDomain.cs` - Domain model definition using fluent API
- `BlogRules.cs` - Business rules for Blog and Post entities
- `Program.cs` - ASP.NET Core setup with domain validation

**Run:**
```bash
cd samples/JD.Domain.Samples.CodeFirst
dotnet run
```

### Database-First Sample

**Location:** `samples/JD.Domain.Samples.DbFirst`

Demonstrates adding JD.Domain rules to existing EF Core scaffolded entities.

**Features:**
- Working with pre-existing entity classes
- Adding business rules without modifying entities
- Manual manifest creation for existing models
- Validation in an existing ASP.NET Core application

**Key Files:**
- `Data/` - Scaffolded EF Core entities (unmodified)
- `BloggingManifest.cs` - Manual manifest for existing entities
- `BlogRules.cs` - Business rules attached to existing types

**Run:**
```bash
cd samples/JD.Domain.Samples.DbFirst
dotnet run
```

### Hybrid Sample

**Location:** `samples/JD.Domain.Samples.Hybrid`

Demonstrates version management with snapshots and diff tools.

**Features:**
- Domain model snapshots (canonical JSON)
- Comparing snapshots to detect changes
- Breaking change detection
- Migration plan generation
- CLI tool integration

**Key Files:**
- `v1/BloggingDomain.cs` - Initial domain version
- `v2/BloggingDomain.cs` - Updated domain version
- `Program.cs` - Snapshot comparison and diff demo

**Run:**
```bash
cd samples/JD.Domain.Samples.Hybrid
dotnet run
```

### Manifest Generation Sample ⭐ NEW

**Location:** `samples/ManifestGeneration.Sample`

Demonstrates automatic manifest generation from entity classes using source generators.

**Features:**
- Opt-in attributes (`[DomainEntity]`, `[DomainValueObject]`)
- Automatic property metadata extraction from data annotations
- Assembly-level manifest configuration
- Property exclusion with `[ExcludeFromManifest]`
- NO manual string writing required

**Key Files:**
- `Customer.cs` - Entity with `[DomainEntity]` attribute
- `Order.cs` - Entity with table/schema configuration
- `Address.cs` - Value object with `[DomainValueObject]` attribute
- `AssemblyInfo.cs` - Assembly-level `[GenerateManifest]` configuration
- `Program.cs` - Demonstrates using auto-generated manifest

**Run:**
```bash
cd samples/ManifestGeneration.Sample
dotnet run
```

**Expected Output:**
```
=== JD.Domain Manifest Generation Sample ===

Manifest Name: ECommerce
Version: 1.0.0
Sources: Generator

Entities: 2
  - Customer (Table: dbo.Customers)
    Properties: 4
    Keys: Id
  - Order (Table: sales.Orders)
    Properties: 5
    Keys: OrderId

Value Objects: 1
  - Address
    Properties: 4

NO MANUAL STRING WRITING REQUIRED!
```

## Sample Workflow Comparison

| Feature | Code-First | DB-First | Hybrid | Manifest Generation |
|---------|-----------|----------|---------|---------------------|
| **Starting Point** | Fresh codebase | Existing database | Existing domain | Existing entities |
| **Manifest Creation** | Fluent DSL | Manual construction | Fluent DSL | Automatic (source generator) |
| **EF Core Entities** | Generated from manifest | Pre-existing | Pre-existing | Pre-existing |
| **Business Rules** | Defined with manifest | Added separately | Defined with manifest | Added separately |
| **Version Management** | Optional | Not shown | Primary focus | Compatible |
| **Best For** | New projects | Legacy databases | Evolving domains | Quick adoption |

## Running All Samples

Run all samples in sequence:

```bash
# Code-First
dotnet run --project samples/JD.Domain.Samples.CodeFirst

# DB-First
dotnet run --project samples/JD.Domain.Samples.DbFirst

# Hybrid
dotnet run --project samples/JD.Domain.Samples.Hybrid

# Manifest Generation
dotnet run --project samples/ManifestGeneration.Sample
```

## Building Samples

Build all samples:

```bash
dotnet build JD.Domain.sln --filter="samples/**"
```

## Next Steps

After exploring the samples:

1. **Choose Your Workflow**
   - [Code-First Tutorial](~/docs/tutorials/code-first-walkthrough.md)
   - [Database-First Tutorial](~/docs/tutorials/db-first-walkthrough.md)
   - [Hybrid Workflow Guide](~/docs/tutorials/hybrid-workflow.md)

2. **Learn Key Concepts**
   - [Domain Modeling](~/docs/tutorials/domain-modeling.md)
   - [Business Rules](~/docs/tutorials/business-rules.md)
   - [Source Generators](~/docs/tutorials/source-generators.md)

3. **Integration Guides**
   - [EF Core Integration](~/docs/how-to/apply-to-modelbuilder.md)
   - [ASP.NET Core Integration](~/docs/how-to/validate-in-aspnet.md)
   - [Generate Automatic Manifests](~/docs/how-to/generate-automatic-manifests.md)

## See Also

- [Getting Started Guide](~/docs/getting-started/quick-start.md)
- [Package Matrix](~/docs/reference/package-matrix.md)
- [Architecture Overview](~/docs/concepts/architecture.md)
