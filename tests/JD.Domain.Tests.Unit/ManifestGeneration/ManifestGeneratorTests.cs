using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using JD.Domain.ManifestGeneration.Generator;

namespace JD.Domain.Tests.Unit.ManifestGeneration;

public class ManifestGeneratorTests
{
    [Fact]
    public void Generator_ProducesManifest_ForDomainEntityAttribute()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("TestDomain", Version = "1.0.0")]

            namespace TestApp;

            [DomainEntity(TableName = "Customers", Schema = "dbo")]
            public class Customer
            {
                [Key]
                public int Id { get; set; }

                [Required]
                [MaxLength(200)]
                public string Name { get; set; } = string.Empty;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);

        var generatedCode = output[0].SourceText.ToString();

        // Verify basic manifest structure
        Assert.Contains("class TestDomainManifest", generatedCode);
        Assert.Contains("GeneratedManifest", generatedCode);
        Assert.Contains("DomainManifest", generatedCode);

        // Verify entity is included
        Assert.Contains("Customer", generatedCode);
        Assert.Contains("EntityManifest", generatedCode);

        // Verify table configuration
        Assert.Contains("Customers", generatedCode);
        Assert.Contains("dbo", generatedCode);

        // Verify properties are extracted
        Assert.Contains("Id", generatedCode);
        Assert.Contains("Name", generatedCode);
    }

    [Fact]
    public void Generator_HandlesValueObjects()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

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
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);

        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("ValueObjects", generatedCode);
        Assert.Contains("Address", generatedCode);
        Assert.Contains("Street", generatedCode);
        Assert.Contains("City", generatedCode);
    }

    [Fact]
    public void Generator_HandlesMultipleEntities()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

            [DomainEntity]
            public class Customer
            {
                [Key]
                public int Id { get; set; }
            }

            [DomainEntity]
            public class Order
            {
                [Key]
                public int OrderId { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);

        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("Customer", generatedCode);
        Assert.Contains("Order", generatedCode);
        Assert.Contains("Entities", generatedCode);
    }

    [Fact]
    public void Generator_ExcludesPropertiesWithExcludeAttribute()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

            [DomainEntity]
            public class User
            {
                [Key]
                public int Id { get; set; }

                public string Username { get; set; } = string.Empty;

                [ExcludeFromManifest]
                public DateTime InternalTimestamp { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);

        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("Username", generatedCode);
        Assert.DoesNotContain("InternalTimestamp", generatedCode);
    }

    [Fact]
    public void Generator_UsesCustomNamespace_WhenSpecified()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0", Namespace = "Custom.Namespace")]

            namespace TestApp;

            [DomainEntity]
            public class Product
            {
                [Key]
                public int Id { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);

        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("namespace Custom.Namespace", generatedCode);
    }

    [Fact]
    public void Generator_IncludesVersionInManifest()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "2.5.1")]

            namespace TestApp;

            [DomainEntity]
            public class Item
            {
                [Key]
                public int Id { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);

        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("Version = new Version(\"2.5.1\")", generatedCode);
    }

    [Fact]
    public void Generator_HandlesNullableReferenceTypes()
    {
        var source = """
            #nullable enable
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

            [DomainEntity]
            public class Document
            {
                [Key]
                public int Id { get; set; }

                public string Title { get; set; } = string.Empty;

                public string? Description { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);

        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("Title", generatedCode);
        Assert.Contains("Description", generatedCode);
    }

    [Fact]
    public void Generator_GeneratesSourcesMetadata()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

            [DomainEntity]
            public class Item
            {
                [Key]
                public int Id { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);

        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("Sources", generatedCode);
        Assert.Contains("SourceInfo", generatedCode);
        Assert.Contains("Generator", generatedCode);
    }

    [Fact]
    public void Generator_SkipsWhenNoAttributes()
    {
        var source = """
            namespace TestApp;

            public class PlainType
            {
                public int Id { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.Empty(output);
    }

    [Fact]
    public void Generator_ExcludesEntityMarkedWithExcludeAttribute()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

            [ExcludeFromManifest]
            [DomainEntity]
            public class HiddenEntity
            {
                [Key]
                public int Id { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        var generatedCode = output[0].SourceText.ToString();
        Assert.DoesNotContain("HiddenEntity", generatedCode);
    }

    [Fact]
    public void Generator_ExcludesValueObjectMarkedWithExcludeAttribute()
    {
        var source = """
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

            [ExcludeFromManifest]
            [DomainValueObject]
            public class HiddenValueObject
            {
                public string Street { get; set; } = string.Empty;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        var generatedCode = output[0].SourceText.ToString();
        Assert.DoesNotContain("HiddenValueObject", generatedCode);
    }

    [Fact]
    public void Generator_SkipsWhenManifestNameEmpty()
    {
        var source = """
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("", Version = "1.0.0")]

            namespace TestApp;

            [DomainEntity]
            public class Customer
            {
                public int Id { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.Single(output);
        Assert.Contains("class GeneratedManifest", output[0].SourceText.ToString());
    }

    [Fact]
    public void Generator_IncludesDescriptionsAndKeyProperties()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

            [DomainEntity(Description = "Entity description", TableName = "Customers", Schema = "dbo")]
            public class Customer
            {
                [Key]
                public int Id { get; set; }
            }

            [DomainValueObject(Description = "Value object description")]
            public class Address
            {
                public string Street { get; set; } = string.Empty;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("Description = \"Entity description\"", generatedCode);
        Assert.Contains("Description = \"Value object description\"", generatedCode);
        Assert.Contains("KeyProperties", generatedCode);
        Assert.Contains("TableName = \"Customers\"", generatedCode);
        Assert.Contains("SchemaName = \"dbo\"", generatedCode);
    }

    [Fact]
    public void Generator_ExcludesNonPublicOrStaticProperties()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0")]

            namespace TestApp;

            [DomainEntity]
            public class Secret
            {
                [Key]
                public int Id { get; set; }

                private string Hidden { get; set; } = string.Empty;

                public static string Shared { get; set; } = string.Empty;
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        var generatedCode = output[0].SourceText.ToString();

        Assert.Contains("Id", generatedCode);
        Assert.DoesNotContain("Hidden", generatedCode);
        Assert.DoesNotContain("Shared", generatedCode);
    }

    [Fact]
    public void Generator_AllowsOutputPathArgument()
    {
        var source = """
            using System.ComponentModel.DataAnnotations;
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0", OutputPath = "snapshots/test.json")]

            namespace TestApp;

            [DomainEntity]
            public class Customer
            {
                [Key]
                public int Id { get; set; }
            }
            """;

        var (diagnostics, output) = RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(output);
    }

    [Fact]
    public void ManifestSourceGenerator_TransformEntity_ReturnsNull_WhenSymbolNotNamedType()
    {
        var context = CreateAttributeContextForMethod();
        var method = typeof(ManifestSourceGenerator).GetMethod("TransformEntity", BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object?[] { context, CancellationToken.None });

        Assert.Null(result);
    }

    [Fact]
    public void ManifestSourceGenerator_TransformValueObject_ReturnsNull_WhenSymbolNotNamedType()
    {
        var context = CreateAttributeContextForMethod();
        var method = typeof(ManifestSourceGenerator).GetMethod("TransformValueObject", BindingFlags.NonPublic | BindingFlags.Static);

        var result = method!.Invoke(null, new object?[] { context, CancellationToken.None });

        Assert.Null(result);
    }

    [Fact]
    public void ManifestSourceGenerator_ExtractAssemblyManifestInfo_ReadsOutputPath()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("""
            using JD.Domain.ManifestGeneration;

            [assembly: GenerateManifest("Test", Version = "1.0.0", OutputPath = "snapshots/test.json")]

            namespace TestApp;
            """);

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var method = typeof(ManifestSourceGenerator).GetMethod("ExtractAssemblyManifestInfo", BindingFlags.NonPublic | BindingFlags.Static);
        var result = method!.Invoke(null, new object?[] { compilation, CancellationToken.None });

        Assert.NotNull(result);
        var outputPath = result!.GetType().GetProperty("OutputPath")!.GetValue(result) as string;
        Assert.Equal("snapshots/test.json", outputPath);
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<GeneratedSourceResult> GeneratedSources)
        RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = GetMetadataReferences();

        // Add reference to the ManifestGeneration attributes assembly
        var manifestGenerationAssembly = typeof(JD.Domain.ManifestGeneration.GenerateManifestAttribute).Assembly;
        references.Add(MetadataReference.CreateFromFile(manifestGenerationAssembly.Location));

        // Add reference to Abstractions
        var abstractionsAssembly = typeof(JD.Domain.Abstractions.DomainManifest).Assembly;
        references.Add(MetadataReference.CreateFromFile(abstractionsAssembly.Location));

        var dataAnnotationsAssembly = typeof(System.ComponentModel.DataAnnotations.KeyAttribute).Assembly;
        references.Add(MetadataReference.CreateFromFile(dataAnnotationsAssembly.Location));

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        var generator = new ManifestSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();

        return (diagnostics, runResult.GeneratedTrees.Length > 0
            ? runResult.Results[0].GeneratedSources
            : ImmutableArray<GeneratedSourceResult>.Empty);
    }

    private static List<MetadataReference> GetMetadataReferences()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location))
            .ToList();

        // Add reference to the ManifestGeneration attributes assembly
        var manifestGenerationAssembly = typeof(JD.Domain.ManifestGeneration.GenerateManifestAttribute).Assembly;
        references.Add(MetadataReference.CreateFromFile(manifestGenerationAssembly.Location));

        // Add reference to Abstractions
        var abstractionsAssembly = typeof(JD.Domain.Abstractions.DomainManifest).Assembly;
        references.Add(MetadataReference.CreateFromFile(abstractionsAssembly.Location));

        var dataAnnotationsAssembly = typeof(System.ComponentModel.DataAnnotations.KeyAttribute).Assembly;
        references.Add(MetadataReference.CreateFromFile(dataAnnotationsAssembly.Location));

        return references;
    }

    private static GeneratorAttributeSyntaxContext CreateAttributeContextForMethod()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("""
            namespace TestApp;

            public class Sample
            {
                [System.Obsolete]
                public void Execute() { }
            }
            """);

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var symbol = semanticModel.GetDeclaredSymbol(methodSyntax)!;

        var ctor = typeof(GeneratorAttributeSyntaxContext).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single();
        return (GeneratorAttributeSyntaxContext)ctor.Invoke(new object?[]
        {
            methodSyntax,
            symbol,
            semanticModel,
            symbol.GetAttributes().ToImmutableArray()
        });
    }
}
