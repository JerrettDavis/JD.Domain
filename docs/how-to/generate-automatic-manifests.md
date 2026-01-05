# Generate Manifests Automatically

Learn how to use source generators to automatically create domain manifests from your entity classes without manual string writing.

## Overview

The `JD.Domain.ManifestGeneration.Generator` source generator automatically analyzes your entity classes at compile-time and generates `DomainManifest` instances based on your code structure and data annotations.

**Key Benefits:**
- ✅ **No manual string writing** - Metadata extracted automatically from your code
- ✅ **Compile-time generation** - No runtime reflection overhead
- ✅ **Type-safe** - Uses actual class and property names from code
- ✅ **Respects sources of truth** - Entity classes, data annotations, and fluent configurations
- ✅ **Opt-in** - Explicit attributes required, no magic discovery

## Prerequisites

Install the required packages:

```powershell
# Attributes (referenced by your code)
dotnet add package JD.Domain.ManifestGeneration

# Source generator (private assets, analyzer)
dotnet add package JD.Domain.ManifestGeneration.Generator
```

## Basic Usage

### 1. Mark Your Entities

Add attributes to your entity classes:

```csharp
using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;

// Assembly-level manifest configuration
[assembly: GenerateManifest("ECommerce", Version = "1.0.0")]

namespace MyApp.Domain;

[DomainEntity(TableName = "Customers", Schema = "dbo")]
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(500)]
    public string Email { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    // Opt-out specific properties
    [ExcludeFromManifest]
    public DateTime InternalTimestamp { get; set; }
}

[DomainValueObject]
public class Address
{
    [Required]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
}
```

### 2. Build Your Project

The generator runs automatically at build time:

```powershell
dotnet build
```

### 3. Use the Generated Manifest

The generator creates a static class with your manifest:

```csharp
using MyApp.Domain;

// Access the auto-generated manifest
var manifest = ECommerceManifest.GeneratedManifest;

Console.WriteLine($"Domain: {manifest.Name}");
Console.WriteLine($"Version: {manifest.Version}");
Console.WriteLine($"Entities: {manifest.Entities.Count}");
Console.WriteLine($"Value Objects: {manifest.ValueObjects.Count}");
```

## Configuration Options

### Assembly-Level Attribute

Configure manifest generation for the entire assembly:

```csharp
[assembly: GenerateManifest(
    "MyDomain",                          // Manifest name (required)
    Version = "1.0.0",                   // Version string
    Namespace = "MyApp.Generated",       // Custom namespace (default: JD.Domain.Generated)
    OutputPath = "manifest.json"         // Optional JSON output (future feature)
)]
```

### Entity Attribute

Configure entity-specific settings:

```csharp
[DomainEntity(
    TableName = "tbl_Customers",        // Database table name
    Schema = "sales",                    // Database schema
    Description = "Customer entity"      // Documentation
)]
public class Customer { }
```

### Value Object Attribute

Mark classes as value objects:

```csharp
[DomainValueObject(
    Description = "Postal address"       // Documentation
)]
public class Address { }
```

### Exclusion Attribute

Exclude specific classes or properties:

```csharp
[DomainEntity]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Property excluded from manifest
    [ExcludeFromManifest]
    public byte[] InternalData { get; set; }
}

// Entire class excluded
[ExcludeFromManifest]
public class InternalAuditLog { }
```

## Supported Data Annotations

The generator automatically extracts metadata from standard data annotations:

| Attribute | Extracted Metadata |
|-----------|-------------------|
| `[Key]` | Marks property as primary key |
| `[Required]` | Sets `IsRequired = true` |
| `[MaxLength(n)]` | Sets `MaxLength = n` |
| Nullable reference types (`string?`) | Sets `IsRequired = false` |
| Non-nullable value types (`int`, `DateTime`) | Sets `IsRequired = true` |

## Advanced Scenarios

### Multiple Entities

Mark multiple entities in the same assembly:

```csharp
[assembly: GenerateManifest("ECommerce", Version = "1.0.0")]

[DomainEntity]
public class Customer { }

[DomainEntity]
public class Order { }

[DomainEntity]
public class Product { }
```

All entities are included in a single manifest.

### Custom Namespaces

Generate manifests in a specific namespace:

```csharp
[assembly: GenerateManifest(
    "MyDomain",
    Version = "1.0.0",
    Namespace = "MyCompany.MyApp.Manifests"
)]
```

The generated class will be in `MyCompany.MyApp.Manifests.MyDomainManifest`.

### Combining with Rules

Auto-generated manifests can be extended with rules:

```csharp
// Manifest generated automatically from entities
var baseManifest = ECommerceManifest.GeneratedManifest;

// Add business rules manually
var customerRules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Email.Valid", c => IsValidEmail(c.Email))
    .WithMessage("Email must be valid")
    .Build();

// Create extended manifest
var extendedManifest = new DomainManifest
{
    Name = baseManifest.Name,
    Version = baseManifest.Version,
    Entities = baseManifest.Entities,
    ValueObjects = baseManifest.ValueObjects,
    RuleSets = new List<RuleSetManifest> { customerRules }
};
```

## Troubleshooting

### Generator Not Running

**Problem:** No manifest code is generated after build.

**Solutions:**
1. Ensure you have both packages installed:
   - `JD.Domain.ManifestGeneration`
   - `JD.Domain.ManifestGeneration.Generator`

2. If you're referencing the generator project directly, ensure the reference is marked as an analyzer:
   ```xml
   <ProjectReference Include="..\..\src\JD.Domain.ManifestGeneration.Generator\JD.Domain.ManifestGeneration.Generator.csproj"
                     OutputItemType="Analyzer"
                     ReferenceOutputAssembly="false" />
   ```

3. Clean and rebuild:
   ```powershell
   dotnet clean
   dotnet build
   ```

### Missing Entities

**Problem:** Some entities are not included in the generated manifest.

**Solutions:**
1. Ensure the class has the `[DomainEntity]` or `[DomainValueObject]` attribute
2. Check that the class is not marked with `[ExcludeFromManifest]`
3. Verify the `[assembly: GenerateManifest(...)]` attribute is present
4. Ensure the class is `public`

### Property Not Extracted

**Problem:** A property is missing from the generated manifest.

**Possible Causes:**
- Property is marked with `[ExcludeFromManifest]`
- Property is not `public`
- Property is `static`

## Next Steps

- [Use Generated Manifests with EF Core](~/docs/how-to/apply-to-modelbuilder.md)
- [Generate Domain Types from Manifests](~/docs/how-to/generate-domain-types.md)
- [Generate FluentValidation Validators](~/docs/how-to/generate-fluentvalidation.md)
- [Create Snapshots for Version Management](~/docs/how-to/create-snapshots.md)

## See Also

- [Source Generators Concept](~/docs/concepts/source-generators.md)
- [Domain Manifest Concept](~/docs/concepts/domain-manifest.md)
- [Manifest Generation Sample](~/samples/ManifestGeneration.Sample)
