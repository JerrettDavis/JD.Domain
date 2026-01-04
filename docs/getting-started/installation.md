# Installation

This guide walks you through installing JD.Domain packages based on your workflow and requirements.

## Prerequisites

Before installing JD.Domain, ensure you have:

- **.NET SDK 10.0 or later** installed
  ```bash
  dotnet --version  # Should show 10.0.x or higher
  ```

- **Basic C# project** (console app, class library, web API, etc.)
  ```bash
  dotnet new webapi -n MyProject
  cd MyProject
  ```

- **(Optional) Entity Framework Core 10.0** if using EF integration
- **(Optional) ASP.NET Core 10.0** if using web integration

## Installation by Workflow

Choose the installation steps based on your workflow:

### Code-First Workflow

For greenfield projects starting with domain definitions:

```bash
# Core packages
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime

# Configuration DSL (for EF Core config generation)
dotnet add package JD.Domain.Configuration

# Optional: EF Core integration
dotnet add package JD.Domain.EFCore

# Optional: Rich domain type generator
dotnet add package JD.Domain.DomainModel.Generator

# Optional: FluentValidation generator
dotnet add package JD.Domain.FluentValidation.Generator
```

### Database-First Workflow

For existing projects with EF Core scaffolded entities:

```bash
# Core packages for rules
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime

# Optional: Rich domain type generator
dotnet add package JD.Domain.DomainModel.Generator

# Optional: FluentValidation generator
dotnet add package JD.Domain.FluentValidation.Generator

# Optional: ASP.NET Core integration
dotnet add package JD.Domain.AspNetCore
```

### Hybrid Workflow

For projects mixing code-first and database-first with version tracking:

```bash
# All core packages
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
dotnet add package JD.Domain.Configuration
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime

# EF Core integration
dotnet add package JD.Domain.EFCore

# Snapshot and diff tools
dotnet add package JD.Domain.Snapshot
dotnet add package JD.Domain.Diff

# Generators
dotnet add package JD.Domain.DomainModel.Generator
dotnet add package JD.Domain.FluentValidation.Generator

# CLI tool for version management
dotnet tool install -g JD.Domain.Cli
```

## Installation by Feature

Alternatively, install packages based on specific features you need:

### Domain Modeling

Define your domain model using fluent DSL:

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Modeling
```

**Use when:** You want to define entities, value objects, and enums using code.

### Business Rules

Add validation rules and invariants:

```bash
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
```

**Use when:** You need to define and evaluate business rules at runtime.

### EF Core Integration

Apply domain configurations to Entity Framework Core:

```bash
dotnet add package JD.Domain.EFCore
```

**Use when:** You want to generate EF Core `ModelBuilder` configurations from domain manifests.

**Requires:** JD.Domain.Abstractions

### ASP.NET Core Integration

Add automatic validation middleware and endpoint filters:

```bash
dotnet add package JD.Domain.Validation
dotnet add package JD.Domain.AspNetCore
```

**Use when:** Building web APIs that need automatic request validation.

**Requires:** JD.Domain.Abstractions, JD.Domain.Rules, JD.Domain.Runtime

### Rich Domain Types

Generate construction-safe domain types with `Result<T>`:

```bash
dotnet add package JD.Domain.DomainModel.Generator
```

**Use when:** You want compile-time safe domain types that enforce invariants.

**Requires:** JD.Domain.Abstractions, JD.Domain.Rules

### FluentValidation Generation

Generate FluentValidation validators from domain rules:

```bash
dotnet add package JD.Domain.FluentValidation.Generator
```

**Use when:** You need FluentValidation validators for API request validation.

**Requires:** JD.Domain.Abstractions, JD.Domain.Rules, FluentValidation 11.x

### Version Management

Track domain evolution with snapshots and diffs:

```bash
dotnet add package JD.Domain.Snapshot
dotnet add package JD.Domain.Diff

# Install CLI tool globally
dotnet tool install -g JD.Domain.Cli
```

**Use when:** You need to track domain changes, detect breaking changes, or generate migration plans.

**Requires:** JD.Domain.Abstractions

### T4 Templates

Integrate with T4 templates for code generation:

```bash
dotnet add package JD.Domain.T4.Shims
```

**Use when:** Using T4 templates for custom code generation.

**Requires:** JD.Domain.Abstractions

## Package Reference Quick Reference

| Package | Version | Target Framework |
|---------|---------|-----------------|
| JD.Domain.Abstractions | 1.0.0 | netstandard2.0 |
| JD.Domain.Modeling | 1.0.0 | net10.0 |
| JD.Domain.Configuration | 1.0.0 | net10.0 |
| JD.Domain.Rules | 1.0.0 | net10.0 |
| JD.Domain.Runtime | 1.0.0 | net10.0 |
| JD.Domain.Validation | 1.0.0 | net10.0 |
| JD.Domain.AspNetCore | 1.0.0 | net10.0 |
| JD.Domain.EFCore | 1.0.0 | net10.0 |
| JD.Domain.Generators.Core | 1.0.0 | netstandard2.0 |
| JD.Domain.DomainModel.Generator | 1.0.0 | netstandard2.0 |
| JD.Domain.FluentValidation.Generator | 1.0.0 | netstandard2.0 |
| JD.Domain.Snapshot | 1.0.0 | net10.0 |
| JD.Domain.Diff | 1.0.0 | net10.0 |
| JD.Domain.Cli | 1.0.0 | net10.0 (global tool) |
| JD.Domain.T4.Shims | 1.0.0 | net10.0 |

## Installing CLI Tools

The CLI tool provides commands for snapshot creation, diff comparison, and migration planning.

### Global Installation

Install globally to use from any directory:

```bash
dotnet tool install -g JD.Domain.Cli
```

Verify installation:

```bash
jd-domain --version
```

### Local Installation

Install as a local tool in your project:

```bash
# Create tool manifest if not exists
dotnet new tool-manifest

# Install locally
dotnet tool install JD.Domain.Cli
```

Run using `dotnet` prefix:

```bash
dotnet jd-domain --version
```

### Updating CLI Tools

```bash
# Update global tool
dotnet tool update -g JD.Domain.Cli

# Update local tool
dotnet tool update JD.Domain.Cli
```

### Uninstalling CLI Tools

```bash
# Uninstall global tool
dotnet tool uninstall -g JD.Domain.Cli

# Uninstall local tool
dotnet tool uninstall JD.Domain.Cli
```

## Verifying Installation

After installing packages, verify they're correctly referenced in your project file:

```bash
dotnet list package
```

You should see output like:

```
Project 'MyProject' has the following package references
   [net10.0]:
   Top-level Package                              Requested
   > JD.Domain.Abstractions                      1.0.0
   > JD.Domain.Modeling                          1.0.0
   > JD.Domain.Rules                             1.0.0
   > JD.Domain.Runtime                           1.0.0
```

## Minimal Project File Example

Here's a complete `.csproj` with common JD.Domain packages:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core packages -->
    <PackageReference Include="JD.Domain.Abstractions" Version="1.0.0" />
    <PackageReference Include="JD.Domain.Modeling" Version="1.0.0" />
    <PackageReference Include="JD.Domain.Rules" Version="1.0.0" />
    <PackageReference Include="JD.Domain.Runtime" Version="1.0.0" />

    <!-- EF Core integration -->
    <PackageReference Include="JD.Domain.EFCore" Version="1.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.1" />

    <!-- ASP.NET Core integration -->
    <PackageReference Include="JD.Domain.AspNetCore" Version="1.0.0" />

    <!-- Generators (source generators are analyzers) -->
    <PackageReference Include="JD.Domain.DomainModel.Generator" Version="1.0.0" />
    <PackageReference Include="JD.Domain.FluentValidation.Generator" Version="1.0.0" />
  </ItemGroup>

</Project>
```

## Troubleshooting

### Package Not Found

If you get "package not found" errors:

1. Ensure you're using .NET 10.0 SDK or later
2. Clear NuGet cache: `dotnet nuget locals all --clear`
3. Check package source: `dotnet nuget list source`

### Version Conflicts

If you encounter version conflicts with EF Core or ASP.NET Core:

```bash
# Check all package versions
dotnet list package --include-transitive

# Update to compatible versions
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.1
dotnet add package Microsoft.AspNetCore.App --version 10.0.0
```

### Source Generator Not Running

If source generators aren't producing code:

1. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build
   ```

2. Check generator is referenced as analyzer:
   ```xml
   <PackageReference Include="JD.Domain.DomainModel.Generator" Version="1.0.0" />
   ```

3. Look for generated files in `obj/` folder:
   ```bash
   find obj -name "*Generated.cs"
   ```

## Next Steps

Now that you have JD.Domain installed, continue with:

- **[Quick Start](quick-start.md)** - Build your first domain model in 5 minutes
- **[Choose Your Workflow](choose-workflow.md)** - Decide which approach fits your project
- **[Code-First Tutorial](../tutorials/code-first-walkthrough.md)** - Complete walkthrough of code-first development

## See Also

- [Package Overview](index.md#package-overview) - Detailed package descriptions
- [Requirements](index.md#requirements) - Framework version requirements
- [API Reference](../../api/index.md) - Complete API documentation
