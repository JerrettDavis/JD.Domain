using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace JD.Domain.ManifestGeneration.Generator;

/// <summary>
/// Source generator that creates DomainManifest instances from entity classes
/// marked with [DomainEntity], [DomainValueObject], or [GenerateManifest] attributes.
/// </summary>
[Generator]
public sealed class ManifestSourceGenerator : IIncrementalGenerator
{
    private const string DomainEntityAttribute = "JD.Domain.ManifestGeneration.DomainEntityAttribute";
    private const string DomainValueObjectAttribute = "JD.Domain.ManifestGeneration.DomainValueObjectAttribute";
    private const string GenerateManifestAttribute = "JD.Domain.ManifestGeneration.GenerateManifestAttribute";
    private const string ExcludeFromManifestAttribute = "JD.Domain.ManifestGeneration.ExcludeFromManifestAttribute";

    /// <summary>
    /// Initializes the incremental generator.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register post-initialization output for diagnostics if needed
        context.RegisterPostInitializationOutput(ctx =>
        {
            // Could add marker interfaces or additional attributes here if needed
        });

        // Pipeline 1: Find entity classes marked with [DomainEntity]
        var entityProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DomainEntityAttribute,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => TransformEntity(ctx, ct))
            .Where(static e => e is not null)
            .Select(static (e, _) => e!);

        // Pipeline 2: Find value object classes marked with [DomainValueObject]
        var valueObjectProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                DomainValueObjectAttribute,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => TransformValueObject(ctx, ct))
            .Where(static vo => vo is not null)
            .Select(static (vo, _) => vo!);

        // Pipeline 3: Find assembly-level [GenerateManifest] attribute
        var assemblyProvider = context.CompilationProvider
            .Select(static (compilation, ct) => ExtractAssemblyManifestInfo(compilation, ct));

        // Combine all sources and generate
        var combined = entityProvider
            .Collect()
            .Combine(valueObjectProvider.Collect())
            .Combine(assemblyProvider);

        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            var ((entities, valueObjects), assemblyInfo) = source;

            if (assemblyInfo is null && entities.IsEmpty && valueObjects.IsEmpty)
            {
                return; // Nothing to generate
            }

            GenerateManifestCode(spc, entities, valueObjects, assemblyInfo);
        });
    }

    /// <summary>
    /// Transforms a class with [DomainEntity] into entity metadata.
    /// </summary>
    private static EntityInfo? TransformEntity(GeneratorAttributeSyntaxContext context, System.Threading.CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        // Check if excluded
        if (HasAttribute(classSymbol, ExcludeFromManifestAttribute))
        {
            return null;
        }

        var attribute = context.Attributes.First();
        var tableName = GetAttributeNamedArgument<string>(attribute, "TableName");
        var schema = GetAttributeNamedArgument<string>(attribute, "Schema");
        var description = GetAttributeNamedArgument<string>(attribute, "Description");

        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !HasAttribute(p, ExcludeFromManifestAttribute))
            .Select(p => AnalyzeProperty(p, ct))
            .Where(p => p is not null)
            .ToImmutableArray();

        return new EntityInfo(
            Name: classSymbol.Name,
            FullTypeName: classSymbol.ToDisplayString(),
            Namespace: classSymbol.ContainingNamespace?.ToDisplayString(),
            TableName: tableName,
            Schema: schema,
            Description: description,
            Properties: properties!);
    }

    /// <summary>
    /// Transforms a class with [DomainValueObject] into value object metadata.
    /// </summary>
    private static ValueObjectInfo? TransformValueObject(GeneratorAttributeSyntaxContext context, System.Threading.CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        // Check if excluded
        if (HasAttribute(classSymbol, ExcludeFromManifestAttribute))
        {
            return null;
        }

        var attribute = context.Attributes.First();
        var description = GetAttributeNamedArgument<string>(attribute, "Description");

        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !HasAttribute(p, ExcludeFromManifestAttribute))
            .Select(p => AnalyzeProperty(p, ct))
            .Where(p => p is not null)
            .ToImmutableArray();

        return new ValueObjectInfo(
            Name: classSymbol.Name,
            FullTypeName: classSymbol.ToDisplayString(),
            Namespace: classSymbol.ContainingNamespace?.ToDisplayString(),
            Description: description,
            Properties: properties!);
    }

    /// <summary>
    /// Analyzes a property and extracts metadata from data annotations.
    /// </summary>
    private static PropertyInfo? AnalyzeProperty(IPropertySymbol property, System.Threading.CancellationToken ct)
    {
        if (property.DeclaredAccessibility != Accessibility.Public || property.IsStatic)
        {
            return null;
        }

        // Get property type information
        var typeName = property.Type.ToDisplayString();
        var isNullable = property.Type.NullableAnnotation == NullableAnnotation.Annotated ||
                        (property.Type is INamedTypeSymbol namedType &&
                         namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T);

        // Extract data annotation attributes
        var isRequired = !isNullable || HasAttribute(property, "System.ComponentModel.DataAnnotations.RequiredAttribute");
        var isKey = HasAttribute(property, "System.ComponentModel.DataAnnotations.KeyAttribute");

        int? maxLength = null;
        var maxLengthAttr = property.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.MaxLengthAttribute");
        if (maxLengthAttr is not null && maxLengthAttr.ConstructorArguments.Length > 0)
        {
            maxLength = (int?)maxLengthAttr.ConstructorArguments[0].Value;
        }

        return new PropertyInfo(
            Name: property.Name,
            TypeName: typeName,
            IsRequired: isRequired,
            IsKey: isKey,
            MaxLength: maxLength);
    }

    /// <summary>
    /// Extracts assembly-level manifest information from [assembly: GenerateManifest(...)].
    /// </summary>
    private static AssemblyManifestInfo? ExtractAssemblyManifestInfo(Compilation compilation, System.Threading.CancellationToken ct)
    {
        var attribute = compilation.Assembly.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == GenerateManifestAttribute);

        if (attribute is null)
        {
            return null;
        }

        var name = attribute.ConstructorArguments.FirstOrDefault().Value as string;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var version = GetAttributeNamedArgument<string>(attribute, "Version");
        var outputPath = GetAttributeNamedArgument<string>(attribute, "OutputPath");
        var ns = GetAttributeNamedArgument<string>(attribute, "Namespace");

        return new AssemblyManifestInfo(
            Name: name!,
            Version: version,
            OutputPath: outputPath,
            Namespace: ns);
    }

    /// <summary>
    /// Generates the manifest source code.
    /// </summary>
    private static void GenerateManifestCode(
        SourceProductionContext context,
        ImmutableArray<EntityInfo> entities,
        ImmutableArray<ValueObjectInfo> valueObjects,
        AssemblyManifestInfo? assemblyInfo)
    {
        var manifestName = assemblyInfo?.Name ?? "Generated";
        var manifestNamespace = assemblyInfo?.Namespace ?? "JD.Domain.Generated";
        var version = assemblyInfo?.Version ?? "1.0.0";

        var emitter = new ManifestEmitter(manifestName, manifestNamespace, version);
        var code = emitter.EmitManifest(entities, valueObjects);

        var fileName = $"{manifestName}Manifest.g.cs";
        context.AddSource(fileName, SourceText.From(code, Encoding.UTF8));
    }

    /// <summary>
    /// Checks if a symbol has a specific attribute.
    /// </summary>
    private static bool HasAttribute(ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == attributeName);
    }

    /// <summary>
    /// Gets a named argument value from an attribute.
    /// </summary>
    private static T? GetAttributeNamedArgument<T>(AttributeData attribute, string argumentName)
    {
        var argument = attribute.NamedArguments
            .FirstOrDefault(na => na.Key == argumentName);

        return argument.Value.Value is T value ? value : default;
    }
}

/// <summary>
/// Metadata about an entity class.
/// </summary>
internal sealed record EntityInfo(
    string Name,
    string FullTypeName,
    string? Namespace,
    string? TableName,
    string? Schema,
    string? Description,
    ImmutableArray<PropertyInfo> Properties);

/// <summary>
/// Metadata about a value object class.
/// </summary>
internal sealed record ValueObjectInfo(
    string Name,
    string FullTypeName,
    string? Namespace,
    string? Description,
    ImmutableArray<PropertyInfo> Properties);

/// <summary>
/// Metadata about a property.
/// </summary>
internal sealed record PropertyInfo(
    string Name,
    string TypeName,
    bool IsRequired,
    bool IsKey,
    int? MaxLength);

/// <summary>
/// Assembly-level manifest configuration.
/// </summary>
internal sealed record AssemblyManifestInfo(
    string Name,
    string? Version,
    string? OutputPath,
    string? Namespace);
