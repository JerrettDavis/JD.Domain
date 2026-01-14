# Choose Your Workflow

JD.Domain supports three main workflows for different project scenarios. This guide helps you choose the right approach for your needs.

## Decision Tree

Use this decision tree to quickly identify your workflow:

```
Do you have an existing database?
│
├─ No → Code-First Workflow
│   └─ Start with domain definitions, generate everything
│
└─ Yes
    │
    ├─ Do you want to keep the database as source of truth?
    │   │
    │   ├─ Yes → Database-First Workflow
    │   │   └─ Scaffold EF entities, add domain rules, generate rich types
    │   │
    │   └─ No → Hybrid Workflow
    │       └─ Mix database and code-first, track with snapshots
```

## Workflow Comparison

| Feature | Code-First | Database-First | Hybrid |
|---------|-----------|----------------|--------|
| **Source of Truth** | Domain code | Database schema | Mixed |
| **Best For** | New projects, greenfield | Existing databases | Large teams, gradual migration |
| **Domain Definition** | Fluent DSL | EF Scaffolding + Rules | Both |
| **EF Configuration** | Generated | Manual + Generated | Mixed |
| **Version Tracking** | Optional | Optional | Required (snapshots) |
| **Learning Curve** | Moderate | Low | High |
| **Flexibility** | High | Medium | Very High |

## Code-First Workflow

### Overview

Start with domain definitions in code using JD.Domain's fluent DSL, then generate EF Core configurations, validators, and rich domain types.

### When to Use

- ✅ Greenfield projects with no existing database
- ✅ Domain-Driven Design (DDD) projects
- ✅ Projects where domain logic is complex and central
- ✅ Teams familiar with domain modeling concepts
- ✅ Projects requiring strong type safety and compile-time validation

### Process Flow

```
1. Define Domain Model (DSL)
   ↓
2. Define Business Rules (DSL)
   ↓
3. Generate EF Core Configurations
   ↓
4. Generate Rich Domain Types
   ↓
5. Generate FluentValidation Validators
   ↓
6. Apply to DbContext
```

### Example

```csharp
// 1. Define domain model
var domain = Domain.Create("ECommerce")
    .Entity<Customer>(e => e
        .Property(c => c.Id)
        .Property(c => c.Name)
        .Property(c => c.Email))
    .Build();

// 2. Define rules
var rules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
    .Build();

// 3. Apply to EF Core
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyDomainManifest(domain);
}
```

### Required Packages

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Configuration
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
dotnet add package JD.Domain.EFCore
dotnet add package JD.Domain.DomainModel.Generator
```

### Pros

- ✅ Single source of truth in code
- ✅ Compile-time type safety
- ✅ Full control over domain design
- ✅ Easy to refactor and version
- ✅ Domain-centric approach

### Cons

- ❌ Steeper learning curve
- ❌ Requires domain modeling knowledge
- ❌ More upfront design work
- ❌ Not ideal for simple CRUD apps

### Next Steps

👉 **[Code-First Tutorial](../tutorials/code-first-walkthrough.md)** - Complete walkthrough

## Database-First Workflow

### Overview

Start with an existing database, scaffold EF Core entities, then add domain rules and generate rich types or validators.

### When to Use

- ✅ Existing databases that must remain authoritative
- ✅ Legacy application modernization
- ✅ Database-driven projects (reports, analytics)
- ✅ Teams more comfortable with databases than code
- ✅ Projects with strict database requirements (compliance, regulations)

### Process Flow

```
1. Scaffold EF Entities from Database
   ↓
2. Create Domain Manifest from Entities
   ↓
3. Define Business Rules for Entities
   ↓
4. Generate Rich Domain Types (Optional)
   ↓
5. Generate FluentValidation Validators (Optional)
   ↓
6. Validate at Runtime
```

### Example

```csharp
// 1. Entities scaffolded from database with [DomainEntity] attributes added
[assembly: GenerateManifest("ECommerce", Version = "1.0.0")]

[DomainEntity(TableName = "Customers")]
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(500)]
    public string Email { get; set; } = "";
}

// 2. Manifest is automatically generated - NO manual creation needed!
var manifest = ECommerceManifest.GeneratedManifest;

// 3. Add rules to scaffolded entities
var rules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
    .Invariant("Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
    .Build();
```

### Required Packages

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.ManifestGeneration
dotnet add package JD.Domain.ManifestGeneration.Generator
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
dotnet add package JD.Domain.DomainModel.Generator  # Optional
dotnet add package JD.Domain.FluentValidation.Generator  # Optional
```

### Pros

- ✅ Works with existing databases
- ✅ Lower learning curve
- ✅ Minimal database changes
- ✅ Gradual adoption possible
- ✅ Familiar to database developers

### Cons

- ❌ Database remains source of truth (potential drift)
- ❌ Less control over domain design
- ❌ EF scaffolding can produce suboptimal models
- ❌ Requires adding attributes to scaffolded entities

### Next Steps

👉 **[Database-First Tutorial](../tutorials/db-first-walkthrough.md)** - Complete walkthrough

## Hybrid Workflow

### Overview

Mix code-first domain definitions with database-first scaffolded entities, using snapshots to track evolution and detect conflicts.

### When to Use

- ✅ Large projects with multiple teams
- ✅ Gradual migration from database-first to code-first
- ✅ Projects with both legacy and new components
- ✅ Teams wanting flexibility to use both approaches
- ✅ Projects requiring strict version tracking

### Process Flow

```
1. Define Some Entities in Code (DSL)
   ↓
2. Scaffold Other Entities from Database
   ↓
3. Merge into Single Domain Manifest
   ↓
4. Create Snapshot (Version 1)
   ↓
5. Make Changes (Code or Database)
   ↓
6. Create Snapshot (Version 2)
   ↓
7. Compare Snapshots (Detect Changes)
   ↓
8. Generate Migration Plan
```

### Example

```csharp
// All entities use source generation - NO manual manifest creation!
[assembly: GenerateManifest("ECommerce", Version = "1.1.0")]

// Code-first entity
[DomainEntity]
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";
}

// Database-first entity (scaffolded with attributes added)
[DomainEntity(TableName = "Orders")]
public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public decimal Total { get; set; }
}

// Access auto-generated manifest
var manifest = ECommerceManifest.GeneratedManifest;

// Create snapshot
var writer = new SnapshotWriter();
var snapshot = writer.CreateSnapshot(manifest);

// Later: compare versions
var diff = diffEngine.Compare(snapshotV1, snapshotV2);
if (diff.HasBreakingChanges)
{
    Console.WriteLine("Warning: Breaking changes detected!");
}
```

### Required Packages

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Configuration
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
dotnet add package JD.Domain.EFCore
dotnet add package JD.Domain.Snapshot
dotnet add package JD.Domain.Diff
dotnet tool install -g JD.Domain.Cli
```

### Pros

- ✅ Maximum flexibility
- ✅ Gradual migration path
- ✅ Works with existing and new code
- ✅ Version tracking and change detection
- ✅ Supports team autonomy

### Cons

- ❌ Most complex approach
- ❌ Requires discipline to avoid conflicts
- ❌ More tooling and infrastructure
- ❌ Steeper learning curve

### Next Steps

👉 **[Hybrid Workflow Tutorial](../tutorials/hybrid-workflow.md)** - Complete walkthrough

## Special Scenarios

### Microservices with Shared Domain

**Recommendation:** Code-First or Hybrid

Use code-first for each service's bounded context. Use snapshots to track evolution and ensure compatibility between services.

```bash
# Service A creates snapshot
jd-domain snapshot --manifest service-a.json --output ./snapshots/service-a-v1.json

# Service B compares with Service A
jd-domain diff ./snapshots/service-a-v1.json ./service-b-manifest.json
```

### Legacy Modernization

**Recommendation:** Database-First → Hybrid → Code-First

Start with database-first to retrofit rules, then gradually move to hybrid as you refactor, and eventually to code-first for new features.

**Migration Path:**
1. **Phase 1:** Scaffold entities, add rules (Database-First)
2. **Phase 2:** Create snapshots, track changes (Hybrid)
3. **Phase 3:** Rewrite critical domains in DSL (Code-First + Hybrid)
4. **Phase 4:** Fully migrate to code-first (Code-First)

### API-Only Projects (No Database)

**Recommendation:** Code-First (Simplified)

Use only modeling and rules packages without EF integration:

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
dotnet add package JD.Domain.AspNetCore
```

### Read-Only Reporting

**Recommendation:** Database-First (Minimal)

For read-only scenarios, you may not need rules at all. Just use EF Core scaffolding.

If you need validation for report parameters, use JD.Domain.Rules on request DTOs:

```csharp
public class ReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

var rules = new RuleSetBuilder<ReportRequest>("Default")
    .Invariant("DateRange.Valid", r => r.EndDate >= r.StartDate)
    .Build();
```

## Choosing Packages by Workflow

### Minimal Code-First

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
```

### Full Code-First

```bash
# Minimal + EF + Generators + ASP.NET Core
dotnet add package JD.Domain.Configuration
dotnet add package JD.Domain.EFCore
dotnet add package JD.Domain.DomainModel.Generator
dotnet add package JD.Domain.FluentValidation.Generator
dotnet add package JD.Domain.AspNetCore
```

### Minimal Database-First

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.ManifestGeneration
dotnet add package JD.Domain.ManifestGeneration.Generator
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
```

### Full Database-First

```bash
# Minimal + Generators + ASP.NET Core
dotnet add package JD.Domain.DomainModel.Generator
dotnet add package JD.Domain.FluentValidation.Generator
dotnet add package JD.Domain.AspNetCore
```

### Hybrid

```bash
# Full Code-First + Snapshot/Diff + CLI
dotnet add package JD.Domain.Snapshot
dotnet add package JD.Domain.Diff
dotnet tool install -g JD.Domain.Cli
```

## Summary Decision Matrix

| Project Type | Workflow | Key Packages | Tutorial |
|-------------|----------|--------------|----------|
| **New project, no DB** | Code-First | Modeling, Rules, EFCore | [Code-First](../tutorials/code-first-walkthrough.md) |
| **Existing DB, keep as-is** | Database-First | Rules, Runtime | [Database-First](../tutorials/db-first-walkthrough.md) |
| **Large team, mixed sources** | Hybrid | All + Snapshot/Diff | [Hybrid](../tutorials/hybrid-workflow.md) |
| **API-only (no DB)** | Code-First (Minimal) | Modeling, Rules, AspNetCore | [ASP.NET](../tutorials/aspnet-core-integration.md) |
| **Legacy modernization** | Database-First → Hybrid | Rules → Snapshot/Diff | [DB-First](../tutorials/db-first-walkthrough.md) |
| **Microservices** | Code-First + Snapshots | Modeling, Snapshot, CLI | [Code-First](../tutorials/code-first-walkthrough.md) |

## Still Not Sure?

If you're still unsure which workflow to choose:

1. **Start with Database-First** if you have an existing database - it's the easiest path
2. **Try Code-First** if you're building something new - it provides the most long-term value
3. **Consider Hybrid** if you have a complex migration scenario - it gives you flexibility

You can always switch workflows later or use different approaches for different parts of your application.

## Next Steps

Now that you've chosen your workflow:

- 📖 **[Next Steps Guide](next-steps.md)** - Continue your learning journey
- 🎓 **Tutorials** - Follow a complete walkthrough for your chosen workflow
  - [Code-First Tutorial](../tutorials/code-first-walkthrough.md)
  - [Database-First Tutorial](../tutorials/db-first-walkthrough.md)
  - [Hybrid Tutorial](../tutorials/hybrid-workflow.md)
- 📚 **[How-To Guides](../how-to/index.md)** - Task-oriented guides for specific operations

## Get Help

- **[GitHub Issues](https://github.com/JerrettDavis/JD.Domain/issues)** - Report problems or ask questions
- **[Samples](../../samples/)** - Browse working examples
- **[API Reference](../../api/index.md)** - Complete API documentation
