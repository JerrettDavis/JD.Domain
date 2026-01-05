# Generate FluentValidation
Auto-generate FluentValidation validators from JD.Domain rule sets.

## Goal
Create validators that mirror your invariants and validators without manual boilerplate.

## Prerequisites
- `JD.Domain.FluentValidation.Generator`
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
    new RuleSetBuilder<Customer>("Default")
        .Validator("Customer.Email.Required", c => !string.IsNullOrWhiteSpace(c.Email))
        .WithMessage("Email is required")
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
        ["Namespace"] = "MyApp.Domain"
    }
};

var generator = new FluentValidationGenerator();
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

### 4. Use the Validators
```csharp
var validator = new CustomerDefaultValidator();
var result = validator.Validate(customer);
```

## Result
Validators stay in sync with your JD.Domain rules and can be registered in DI like any FluentValidation validator.

## See Also
- [Source Generators Tutorial](../tutorials/source-generators.md)
- [Create Validators](create-validators.md)
