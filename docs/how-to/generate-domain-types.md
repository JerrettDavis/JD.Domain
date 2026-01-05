# Generate Domain Types
Create construction-safe domain types with `Result<T>` that enforce invariants and validation.

## Goal
Generate rich domain types that wrap entities, validate on construction, and optionally enforce rules on property setters.

## Prerequisites
- `JD.Domain.DomainModel.Generator`
- `JD.Domain.Generators.Core`
- `Microsoft.CodeAnalysis.CSharp`
- A `DomainManifest` that includes rule sets

## Steps
### 1. Prepare a Manifest with Rules
```csharp
using JD.Domain.Abstractions;
using JD.Domain.Rules;

var baseManifest = ECommerceManifest.GeneratedManifest;
var rules = new List<RuleSetManifest>
{
    new RuleSetBuilder<Customer>("Create")
        .Invariant("Customer.Name.Required", c => !string.IsNullOrWhiteSpace(c.Name))
        .WithMessage("Name is required")
        .Build()
};

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
    RuleSets = rules
};
```

### 2. Run the Generator Pipeline
```csharp
using JD.Domain.DomainModel.Generator;
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

var generator = new DomainModelGenerator();
foreach (var file in generator.Generate(context))
{
    Directory.CreateDirectory("Generated");
    File.WriteAllText(Path.Combine("Generated", file.FileName), file.Content);
}
```

### 3. Include Generated Files
```xml
<ItemGroup>
  <Compile Include="Generated\**\*.g.cs" />
</ItemGroup>
```

### 4. Use Generated Domain Types
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
```

## Result
Generated domain types enforce invariants during construction and, depending on `ValidationMode`, during property updates.

## See Also
- [Source Generators Tutorial](../tutorials/source-generators.md)
- [Create Invariants](create-invariants.md)
