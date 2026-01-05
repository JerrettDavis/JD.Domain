# Source Generators
Use JD.Domain generators to create manifests, construction-safe domain types, and FluentValidation validators automatically.
**Time:** 60 minutes | **Level:** Intermediate-Advanced

## What You'll Learn
- Manifest source generator setup and IDE registration
- Domain model and FluentValidation generation pipeline
- Baking invariants and validation into generated types
- Generator options and deterministic output
- Troubleshooting generator issues

## Prerequisites
- Completion of [Domain Modeling](domain-modeling.md)
- Basic familiarity with source generators

## 1. Install Packages
```powershell
# Manifest generation (attributes + source generator)
dotnet add package JD.Domain.ManifestGeneration
dotnet add package JD.Domain.ManifestGeneration.Generator

# Domain model + FluentValidation generators
dotnet add package JD.Domain.DomainModel.Generator
dotnet add package JD.Domain.FluentValidation.Generator

# Generator pipeline dependency
dotnet add package Microsoft.CodeAnalysis.CSharp
```

## 2. Mark Entities and Generate the Manifest
```csharp
using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;

[assembly: GenerateManifest("ECommerce", Version = "1.0.0", Namespace = "MyApp.Domain")]

namespace MyApp.Domain;

[DomainEntity(TableName = "Customers", Schema = "dbo")]
public class Customer
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Email { get; set; } = string.Empty;
}
```

Build once and the manifest appears as `ECommerceManifest.GeneratedManifest`.

## 3. Define Invariants and Validators
```csharp
using JD.Domain.Abstractions;
using JD.Domain.Rules;

var createRules = new RuleSetBuilder<Customer>("Create")
    .Invariant("Customer.Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
    .WithMessage("Name is required")
    .Validator("Customer.Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
    .WithMessage("Email is required")
    .Build();

var defaultRules = new RuleSetBuilder<Customer>("Default")
    .Invariant("Customer.Email.Format", c => c.Email.Contains("@"))
    .WithMessage("Email must be valid")
    .Build();
```

## 4. Compose a Manifest with Rules
```csharp
var baseManifest = ECommerceManifest.GeneratedManifest;
var ruleSets = new List<RuleSetManifest> { createRules, defaultRules };

var manifest = new DomainManifest
{
    Name = baseManifest.Name,
    Version = baseManifest.Version,
    Entities = baseManifest.Entities,
    ValueObjects = baseManifest.ValueObjects,
    Enums = baseManifest.Enums,
    Configurations = baseManifest.Configurations,
    Sources = baseManifest.Sources,
    Metadata = baseManifest.Metadata,
    RuleSets = ruleSets
};
```

## 5. Generate Domain Types and Validators
```csharp
using System.IO;
using System.Threading;
using JD.Domain.DomainModel.Generator;
using JD.Domain.FluentValidation.Generator;
using JD.Domain.Generators.Core;
using Microsoft.CodeAnalysis.CSharp;

var compilation = CSharpCompilation.Create("MyApp.Domain");
var context = new GeneratorContext
{
    Manifest = manifest,
    Compilation = compilation,
    CancellationToken = CancellationToken.None,
    Properties =
    {
        ["Namespace"] = "MyApp.Domain",
        ["DomainTypePrefix"] = "Domain",
        ["CreateRuleSet"] = "Create",
        ["DefaultRuleSet"] = "Default",
        ["ValidationMode"] = "OnSet"
    }
};

var pipeline = new GeneratorPipeline()
    .Add(new DomainModelGenerator())
    .Add(new FluentValidationGenerator());

foreach (var file in pipeline.Execute(context))
{
    Directory.CreateDirectory("Generated");
    File.WriteAllText(Path.Combine("Generated", file.FileName), file.Content);
}
```

## 6. Use the Generated Types
```csharp
var result = DomainCustomer.Create(
    name: "Ada Lovelace",
    email: "ada@example.com");

if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error.Message);
    }
}

var validator = new CustomerDefaultValidator();
```

## Generator Options
Set these in `GeneratorContext.Properties`:
- `Namespace`: root namespace for generated types
- `DomainTypePrefix`: default "Domain"
- `GenerateWithMethods`: true/false
- `GeneratePartialClasses`: true/false
- `CreateRuleSet`: rule set name used for `Create(...)`
- `DefaultRuleSet`: rule set name used for `Validate(...)`
- `GenerateImplicitConversion`: true/false
- `IncludeDomainContext`: true/false
- `ValidationMode`: `OnSet`, `OnDemand`, or `Lazy`

## Troubleshooting
If the manifest isn't generated, confirm both packages are referenced and the generator is registered as an analyzer when using project references. See [Generate Manifests Automatically](../how-to/generate-automatic-manifests.md) for details.

## API Reference
- [DomainModelGenerator](../../api/JD.Domain.DomainModel.Generator.DomainModelGenerator.yml)
- [FluentValidationGenerator](../../api/JD.Domain.FluentValidation.Generator.FluentValidationGenerator.yml)

## Next Steps
- [Generate Domain Types](../how-to/generate-domain-types.md)
- [Generate FluentValidation Validators](../how-to/generate-fluentvalidation.md)
