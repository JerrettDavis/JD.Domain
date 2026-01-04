# Automatic Manifest Generation Design

## Problem Statement

Current manifest generation requires manual effort in both code-first and db-first workflows:

**DB-First:**
```csharp
// After scaffolding: dotnet ef dbcontext scaffold ...
// Users must manually create manifests:
var manifest = new DomainManifest {
    Name = "Blogging",
    Entities = [
        new EntityManifest {
            Name = "Blog",
            TypeName = "MyApp.Data.Blog",
            Properties = [...] // Manual transcription
        }
    ]
};
```

**Code-First:**
```csharp
// Users must manually build via DSL:
var manifest = Domain.Create("Blogging")
    .Entity<Blog>(e => {
        e.Key(x => x.Id);
        e.Property(x => x.Name).IsRequired();
    })
    .BuildManifest();
```

**Pain Points:**
- Requires duplicating information already present in code
- Error-prone manual transcription
- No automatic discovery of existing entities
- Difficult migration from legacy codebases

## Proposed Solution: Opt-In Source Generators

Create a new package **JD.Domain.ManifestGeneration.Generator** that automatically generates manifests from:
1. EF Core DbContext models (db-first)
2. Attributed entity classes (code-first)
3. Assembly-level scanning

### Architecture

```
JD.Domain.ManifestGeneration.Generator/
├── Attributes/
│   ├── GenerateManifestAttribute.cs          [assembly] or [class]
│   ├── DomainEntityAttribute.cs              [class]
│   ├── DomainValueObjectAttribute.cs         [class]
│   └── ExcludeFromManifestAttribute.cs       [class] or [property]
├── Analyzers/
│   ├── DbContextAnalyzer.cs                  Introspects IModel
│   ├── EntityAnalyzer.cs                     Discovers entity classes
│   └── PropertyAnalyzer.cs                   Extracts property metadata
├── Generators/
│   ├── ManifestGenerator.cs                  Main IIncrementalGenerator
│   ├── DbContextManifestGenerator.cs         EF Core model → manifest
│   └── EntityManifestGenerator.cs            Class attributes → manifest
└── Emitters/
    └── ManifestEmitter.cs                    Emits DomainManifest code
```

## Usage Scenarios

### Scenario 1: DB-First with DbContext Discovery

```csharp
using JD.Domain.ManifestGeneration;

// Opt-in: Mark DbContext for manifest generation
[GenerateManifest("Blogging", Version = "1.0.0")]
public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Generator reads IModel metadata
        modelBuilder.Entity<Blog>(b => {
            b.HasKey(x => x.Id);
            b.Property(x => x.Url).IsRequired().HasMaxLength(500);
            b.HasIndex(x => x.Url).IsUnique();
        });
    }
}

// GENERATED CODE: BloggingContext.Manifest.g.cs
public static partial class BloggingContext
{
    public static DomainManifest GeneratedManifest { get; } = new DomainManifest
    {
        Name = "Blogging",
        Version = new Version(1, 0, 0),
        Source = "DbContext:BloggingContext",
        Entities = [
            new EntityManifest {
                Name = "Blog",
                TypeName = "MyApp.Data.Blog",
                TableName = "Blogs",
                Properties = [
                    new PropertyManifest { Name = "Id", TypeName = "System.Int32", IsRequired = true },
                    new PropertyManifest { Name = "Url", TypeName = "System.String", IsRequired = true, MaxLength = 500 }
                ],
                KeyProperties = ["Id"],
                Indexes = [
                    new IndexManifest { Properties = ["Url"], IsUnique = true }
                ]
            }
        ]
    };
}
```

### Scenario 2: Code-First with Entity Attributes

```csharp
using JD.Domain.ManifestGeneration;

// Assembly-level opt-in
[assembly: GenerateManifest("ECommerce", Version = "1.0.0")]

// Mark entities for discovery
[DomainEntity(TableName = "Customers", Schema = "dbo")]
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [ExcludeFromManifest] // Opt-out specific properties
    public DateTime InternalTimestamp { get; set; }
}

[DomainValueObject]
public class Address
{
    [Required]
    public string Street { get; set; }

    [MaxLength(100)]
    public string City { get; set; }
}

// GENERATED CODE: ECommerce.Manifest.g.cs
public static class ECommerceManifest
{
    public static DomainManifest GeneratedManifest { get; } = new DomainManifest
    {
        Name = "ECommerce",
        Version = new Version(1, 0, 0),
        Source = "Assembly:MyApp",
        Entities = [
            new EntityManifest {
                Name = "Customer",
                TypeName = "MyApp.Customer",
                TableName = "Customers",
                SchemaName = "dbo",
                Properties = [
                    new PropertyManifest { Name = "Id", TypeName = "System.Int32", IsRequired = true },
                    new PropertyManifest { Name = "Name", TypeName = "System.String", IsRequired = true, MaxLength = 200 },
                    new PropertyManifest { Name = "Email", TypeName = "System.String", IsRequired = false }
                ],
                KeyProperties = ["Id"]
            }
        ],
        ValueObjects = [
            new ValueObjectManifest {
                Name = "Address",
                TypeName = "MyApp.Address",
                Properties = [
                    new PropertyManifest { Name = "Street", TypeName = "System.String", IsRequired = true },
                    new PropertyManifest { Name = "City", TypeName = "System.String", IsRequired = false, MaxLength = 100 }
                ]
            }
        ]
    };
}
```

### Scenario 3: Hybrid - DbContext + Rules DSL

```csharp
// Auto-generate base manifest from DbContext
[GenerateManifest("Blogging", Version = "1.0.0")]
public partial class BloggingContext : DbContext
{
    // ... EF configuration
}

// Manually extend with rules (separate file)
public static class BloggingManifestExtensions
{
    public static DomainManifest WithRules(this DomainManifest manifest)
    {
        var rules = new RuleSetBuilder<Blog>("Default")
            .Invariant("Url.Required", b => !string.IsNullOrWhiteSpace(b.Url))
            .Invariant("Url.Valid", b => Uri.IsWellFormedUriString(b.Url, UriKind.Absolute))
            .Build();

        manifest.RuleSets.Add(rules);
        return manifest;
    }
}

// Usage:
var manifest = BloggingContext.GeneratedManifest.WithRules();
```

## Implementation Details

### Attribute Definitions

```csharp
namespace JD.Domain.ManifestGeneration;

/// <summary>
/// Marks a DbContext or assembly for automatic manifest generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class GenerateManifestAttribute : Attribute
{
    public string Name { get; }
    public string? Version { get; set; }
    public string? OutputPath { get; set; } // Optional JSON file output

    public GenerateManifestAttribute(string name) => Name = name;
}

/// <summary>
/// Marks a class as a domain entity for manifest inclusion.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DomainEntityAttribute : Attribute
{
    public string? TableName { get; set; }
    public string? Schema { get; set; }
}

/// <summary>
/// Marks a class as a value object for manifest inclusion.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DomainValueObjectAttribute : Attribute
{
}

/// <summary>
/// Excludes a class or property from manifest generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class ExcludeFromManifestAttribute : Attribute
{
}
```

### Generator Pipeline

```csharp
[Generator]
public class ManifestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Pipeline 1: DbContext discovery
        var dbContexts = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "JD.Domain.ManifestGeneration.GenerateManifestAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, _) => AnalyzeDbContext(ctx))
            .Where(ctx => ctx is not null);

        // Pipeline 2: Entity attribute discovery
        var entities = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "JD.Domain.ManifestGeneration.DomainEntityAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, _) => AnalyzeEntity(ctx))
            .Where(e => e is not null);

        // Pipeline 3: Assembly-level discovery
        var assemblyManifests = context.CompilationProvider
            .Select((compilation, _) => AnalyzeAssembly(compilation));

        // Combine and emit
        context.RegisterSourceOutput(
            dbContexts.Combine(entities).Combine(assemblyManifests),
            (spc, source) => EmitManifest(spc, source));
    }

    private DbContextInfo? AnalyzeDbContext(GeneratorAttributeSyntaxContext context)
    {
        // 1. Get DbContext class symbol
        // 2. Find OnModelCreating method
        // 3. Introspect modelBuilder calls via semantic analysis
        // 4. Extract entity types, properties, keys, indexes, relationships
        // 5. Return DbContextInfo with manifest data
    }

    private EntityInfo? AnalyzeEntity(GeneratorAttributeSyntaxContext context)
    {
        // 1. Get entity class symbol
        // 2. Extract properties with data annotations
        // 3. Detect key properties ([Key], Id convention)
        // 4. Extract validation attributes
        // 5. Return EntityInfo with manifest data
    }

    private AssemblyManifestInfo? AnalyzeAssembly(Compilation compilation)
    {
        // 1. Check for [assembly: GenerateManifest(...)]
        // 2. Scan all types in assembly for [DomainEntity], [DomainValueObject]
        // 3. Aggregate into assembly-level manifest
        // 4. Return AssemblyManifestInfo
    }
}
```

### DbContext Model Introspection

**Challenge:** Roslyn semantic analysis of `OnModelCreating` is complex.

**Solution:** Use **hybrid approach**:

1. **Design-time approach**: Generate manifest at build time via source generator
   - Analyze `modelBuilder.Entity<T>()` calls syntactically
   - Limited to statically analyzable configurations

2. **Runtime approach** (alternative): Use reflection to read `IModel`
   - Access compiled DbContext's model metadata
   - More accurate but requires runtime execution
   - Could be done via MSBuild task instead of source generator

**Recommendation:** Start with **attribute-based approach** (Scenario 2), defer DbContext introspection to v2.

### Generated Code Structure

```csharp
// File: {ManifestName}.Manifest.g.cs

#nullable enable

namespace JD.Domain.Generated;

using JD.Domain.Abstractions;

/// <summary>
/// Auto-generated manifest for {ManifestName}.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("JD.Domain.ManifestGeneration.Generator", "1.0.0")]
public static class {ManifestName}Manifest
{
    public static DomainManifest GeneratedManifest { get; } = new()
    {
        Name = "{ManifestName}",
        Version = new System.Version({Major}, {Minor}, {Patch}),
        Source = "{Source}",
        CreatedAt = new System.DateTimeOffset({Timestamp}, System.TimeSpan.Zero),
        Entities =
        [
            // Generated entities...
        ],
        ValueObjects =
        [
            // Generated value objects...
        ],
        Enums =
        [
            // Generated enums...
        ]
    };

    /// <summary>
    /// Saves the generated manifest to a JSON file.
    /// </summary>
    public static void SaveSnapshot(string path)
    {
        var writer = new JD.Domain.Snapshot.SnapshotWriter();
        writer.WriteSnapshot(GeneratedManifest, path);
    }
}
```

### Integration with Existing Generators

```csharp
// Users can chain generators:
// 1. ManifestGeneration.Generator → produces manifest code
// 2. DomainModel.Generator → consumes manifest to generate proxies
// 3. FluentValidation.Generator → consumes manifest to generate validators

// Example project setup:
<ItemGroup>
  <!-- Step 1: Generate manifest from entities -->
  <PackageReference Include="JD.Domain.ManifestGeneration.Generator" Version="0.1.0"
                    PrivateAssets="all" />

  <!-- Step 2: Generate domain proxies from manifest -->
  <PackageReference Include="JD.Domain.DomainModel.Generator" Version="0.1.0"
                    PrivateAssets="all" />

  <!-- Step 3: Generate validators from manifest -->
  <PackageReference Include="JD.Domain.FluentValidation.Generator" Version="0.1.0"
                    PrivateAssets="all" />
</ItemGroup>

// The ManifestGeneration.Generator makes the manifest available to other generators
// via the compilation context.
```

## Package Structure

```
JD.Domain.ManifestGeneration/
├── JD.Domain.ManifestGeneration/              Attributes only (referenced by user code)
│   ├── GenerateManifestAttribute.cs
│   ├── DomainEntityAttribute.cs
│   ├── DomainValueObjectAttribute.cs
│   └── ExcludeFromManifestAttribute.cs
│
└── JD.Domain.ManifestGeneration.Generator/    Source generator (analyzer/private assets)
    ├── ManifestGenerator.cs
    ├── Analyzers/
    ├── Emitters/
    └── Templates/
```

**Dependency:**
- `JD.Domain.ManifestGeneration` → `JD.Domain.Abstractions` (for DomainManifest types)
- `JD.Domain.ManifestGeneration.Generator` → `JD.Domain.Generators.Core` (for infrastructure)

## Incremental Implementation Plan

### Phase 1: Attribute-Based Entity Discovery ⭐ START HERE
**Deliverable:** Generate manifests from `[DomainEntity]` and `[DomainValueObject]` attributes

1. Create `JD.Domain.ManifestGeneration` project with attribute definitions
2. Create `JD.Domain.ManifestGeneration.Generator` project
3. Implement `EntityAnalyzer` to discover attributed classes
4. Implement `PropertyAnalyzer` to extract property metadata
5. Implement `ManifestEmitter` to generate manifest code
6. Write tests using attributed entities
7. Document usage in how-to guides

**Complexity:** Medium
**Value:** High - enables code-first auto-generation immediately

### Phase 2: Assembly-Level Scanning
**Deliverable:** Generate manifests from `[assembly: GenerateManifest(...)]`

1. Implement `AnalyzeAssembly` to scan compilation
2. Support assembly-level attribute with filtering rules
3. Add configuration options (include/exclude patterns)
4. Test with multi-project solutions

**Complexity:** Low
**Value:** Medium - convenience for simple projects

### Phase 3: Enhanced Metadata Extraction
**Deliverable:** Extract indexes, relationships, constraints

1. Analyze `[Index]` attributes (EF Core 5+)
2. Detect navigation properties and foreign keys
3. Extract check constraints and default values
4. Support custom attributes for business rules

**Complexity:** Medium
**Value:** High - parity with manual manifest creation

### Phase 4: DbContext Model Introspection (Optional)
**Deliverable:** Generate manifests from EF Core `OnModelCreating`

**Option A: Syntactic Analysis (Source Generator)**
- Analyze `modelBuilder.Entity<T>()` calls syntactically
- Limited to simple, statically analyzable configurations
- Complexity: High, Value: Medium

**Option B: Runtime Reflection (MSBuild Task)**
- Load compiled assembly and read `IModel` via reflection
- Full fidelity with all EF Core features
- Requires MSBuild task instead of source generator
- Complexity: Medium, Value: High

**Recommendation:** Defer to v0.2.0, gather user feedback on Phase 1-3 first.

### Phase 5: Integration with Snapshot/Diff
**Deliverable:** Auto-save generated manifests as snapshots

1. Add `OutputPath` to `[GenerateManifest]` attribute
2. Emit JSON snapshot files alongside C# code
3. Integrate with CLI tools for diff/migrate-plan
4. Support incremental snapshot updates

**Complexity:** Low
**Value:** High - enables version management workflows

## Benefits

### For DB-First Users
- ✅ No manual manifest transcription
- ✅ Single source of truth (EF entity classes)
- ✅ Automatic synchronization with scaffolded entities
- ✅ Easy migration from legacy codebases

### For Code-First Users
- ✅ Less boilerplate (attributes vs. DSL)
- ✅ Faster onboarding
- ✅ Convention over configuration
- ✅ Still allows manual DSL for complex scenarios

### For Framework
- ✅ Lowers adoption barrier significantly
- ✅ Competitive with other domain modeling frameworks
- ✅ Enables incremental migration strategies
- ✅ Maintains opt-in philosophy (no magic, explicit attributes)

## Risks & Mitigations

### Risk: DbContext introspection complexity
**Mitigation:** Start with attribute-based approach (Phase 1-3), defer DbContext analysis to later phase after user validation.

### Risk: Generator performance impact
**Mitigation:** Use incremental generator API, cache analysis results, limit scanning to attributed types only.

### Risk: Attribute pollution of domain models
**Mitigation:** Keep attributes minimal and optional. Users can still use DSL for complex scenarios. Attributes are just convenience.

### Risk: Breaking changes in EF Core
**Mitigation:** Target stable EF Core APIs. For DbContext introspection, use runtime reflection (Option B) which is more resilient.

## Alternative Approaches Considered

### 1. Convention-Based Discovery (No Attributes)
**Example:** Scan all classes ending in "Entity" or implementing IEntity
**Rejected:** Too magical, conflicts with opt-in philosophy, high risk of false positives

### 2. Fluent Configuration Files
**Example:** Manifest.cs file with fluent API configuration
**Rejected:** This already exists (Domain.Create() DSL), we want to reduce boilerplate

### 3. CLI Import Command
**Example:** `jd-domain import --assembly MyApp.dll --output manifest.json`
**Considered:** Good for one-time migration, but doesn't solve ongoing synchronization. Could complement source generator approach.

## Success Metrics

- ✅ DB-first users can generate manifests with <5 lines of code (attribute on DbContext)
- ✅ Code-first users can generate manifests with <1 line per entity (attribute on class)
- ✅ Generated manifests are equivalent to manually created ones
- ✅ Generator runs incrementally (no full rebuild required)
- ✅ Clear documentation and samples for both workflows

## Open Questions

1. **Should we support partial manifests?** Allow combining auto-generated + manual manifests?
   - **Answer:** Yes - use extension methods like `.WithRules()` to augment generated manifests

2. **How to handle circular references in relationships?**
   - **Answer:** Generate forward references, rely on manifest merge logic

3. **Should generated manifests be committed to source control?**
   - **Answer:** No (generated code), but snapshots (JSON) should be versioned

4. **Integration with T4 templates?**
   - **Answer:** T4 can consume generated manifests via T4ManifestLoader (already exists)

## Conclusion

**Recommended Path Forward:**

1. ✅ Implement Phase 1 (Attribute-Based Entity Discovery) for v0.2.0
2. ✅ Validate with user feedback and sample projects
3. ✅ Implement Phase 2-3 (Assembly scanning, enhanced metadata) for v0.3.0
4. ⏸️ Defer Phase 4 (DbContext introspection) until user demand is proven
5. ✅ Implement Phase 5 (Snapshot integration) for v0.4.0

This approach:
- Delivers immediate value (Phase 1)
- Maintains opt-in philosophy (explicit attributes)
- Reduces boilerplate for common scenarios
- Preserves flexibility for complex scenarios (manual DSL still available)
- Aligns with existing generator infrastructure
- Enables incremental adoption

**Estimated effort:** 2-3 weeks for Phase 1 (MVP), 1-2 weeks each for Phase 2-5
