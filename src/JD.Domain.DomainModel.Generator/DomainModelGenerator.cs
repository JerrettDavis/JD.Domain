using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JD.Domain.Abstractions;
using JD.Domain.DomainModel.Generator.Generators;
using JD.Domain.DomainModel.Generator.Options;
using JD.Domain.Generators.Core;

namespace JD.Domain.DomainModel.Generator;

/// <summary>
/// Generates domain proxy wrapper types from JD entity manifests.
/// Domain types wrap EF entities and enforce rules in property accessors.
/// </summary>
public sealed class DomainModelGenerator : BaseCodeGenerator
{
    private readonly DomainProxyGenerator _proxyGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainModelGenerator"/> class.
    /// </summary>
    public DomainModelGenerator()
    {
        _proxyGenerator = new DomainProxyGenerator();
    }

    /// <inheritdoc/>
    public override string Name => "DomainModelGenerator";

    /// <inheritdoc/>
    public override bool CanGenerate(GeneratorContext context)
    {
        return context.Manifest.Entities.Count > 0;
    }

    /// <inheritdoc/>
    public override IEnumerable<GeneratedFile> Generate(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        var options = ResolveOptions(context);

        foreach (var entity in context.Manifest.Entities)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Find rule sets that apply to this entity
            var entityRuleSets = context.Manifest.RuleSets
                .Where(rs => MatchesEntity(rs.TargetType, entity))
                .ToList();

            // Generate the domain proxy type
            var file = _proxyGenerator.Generate(context, entity, entityRuleSets, options);
            yield return CreateGeneratedFile(file.FileName, file.Content);
        }
    }

    /// <summary>
    /// Resolves generator options from the context properties.
    /// </summary>
    private static DomainModelOptions ResolveOptions(GeneratorContext context)
    {
        var options = new DomainModelOptions();

        if (context.Properties.TryGetValue("Namespace", out var ns))
        {
            options.Namespace = ns;
        }

        if (context.Properties.TryGetValue("DomainTypePrefix", out var prefix))
        {
            options.DomainTypePrefix = prefix;
        }

        if (context.Properties.TryGetValue("GenerateWithMethods", out var genWith) &&
            bool.TryParse(genWith, out var genWithVal))
        {
            options.GenerateWithMethods = genWithVal;
        }

        if (context.Properties.TryGetValue("GeneratePartialClasses", out var genPartial) &&
            bool.TryParse(genPartial, out var genPartialVal))
        {
            options.GeneratePartialClasses = genPartialVal;
        }

        if (context.Properties.TryGetValue("ValidationMode", out var validationMode) ||
            context.Properties.TryGetValue("PropertyValidationMode", out validationMode))
        {
            if (Enum.TryParse<PropertyValidationMode>(validationMode, true, out var parsedMode))
            {
                options.ValidationMode = parsedMode;
            }
        }

        if (context.Properties.TryGetValue("CreateRuleSet", out var createRs))
        {
            options.CreateRuleSet = createRs;
        }

        if (context.Properties.TryGetValue("DefaultRuleSet", out var defaultRs))
        {
            options.DefaultRuleSet = defaultRs;
        }

        if (context.Properties.TryGetValue("GenerateImplicitConversion", out var genImplicit) &&
            bool.TryParse(genImplicit, out var genImplicitVal))
        {
            options.GenerateImplicitConversion = genImplicitVal;
        }

        if (context.Properties.TryGetValue("IncludeDomainContext", out var inclContext) &&
            bool.TryParse(inclContext, out var inclContextVal))
        {
            options.IncludeDomainContext = inclContextVal;
        }

        return options;
    }

    /// <summary>
    /// Checks if a rule set target type matches an entity.
    /// </summary>
    private static bool MatchesEntity(string ruleSetTargetType, EntityManifest entity)
    {
        // Exact match
        if (ruleSetTargetType == entity.TypeName)
        {
            return true;
        }

        // Match by simple name (without namespace)
        var simpleRuleType = ExtractClassName(ruleSetTargetType);
        var simpleEntityType = ExtractClassName(entity.TypeName);

        return simpleRuleType == simpleEntityType || simpleRuleType == entity.Name;
    }

    /// <summary>
    /// Extracts just the class name from a fully qualified type name.
    /// </summary>
    private static string ExtractClassName(string typeName)
    {
        var plusIndex = typeName.LastIndexOf('+');
        if (plusIndex >= 0)
        {
            return typeName.Substring(plusIndex + 1);
        }

        var dotIndex = typeName.LastIndexOf('.');
        if (dotIndex >= 0)
        {
            return typeName.Substring(dotIndex + 1);
        }

        return typeName;
    }
}
