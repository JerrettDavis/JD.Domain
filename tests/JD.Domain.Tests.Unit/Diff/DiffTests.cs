using JD.Domain.Abstractions;
using JD.Domain.Diff;
using JD.Domain.Snapshot;
using Xunit;

namespace JD.Domain.Tests.Unit.Diff;

public class DiffTests
{
    private static DomainSnapshot CreateSnapshot(string name, Version version, params EntityManifest[] entities)
    {
        var manifest = new DomainManifest
        {
            Name = name,
            Version = version,
            Entities = entities.ToList()
        };

        var writer = new SnapshotWriter();
        return writer.CreateSnapshot(manifest);
    }

    private static EntityManifest CreateEntity(string name, params PropertyManifest[] properties)
    {
        return new EntityManifest
        {
            Name = name,
            TypeName = $"TestDomain.{name}",
            Properties = properties.ToList(),
            KeyProperties = ["Id"]
        };
    }

    private static PropertyManifest CreateProperty(string name, string typeName, bool isRequired = false)
    {
        return new PropertyManifest
        {
            Name = name,
            TypeName = typeName,
            IsRequired = isRequired
        };
    }

    [Fact]
    public void DiffEngine_Compare_NoChanges_ReturnsEmptyDiff()
    {
        var entity = CreateEntity("Customer",
            CreateProperty("Id", "System.Guid", true),
            CreateProperty("Name", "System.String", true));

        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0), entity);
        var after = CreateSnapshot("TestDomain", new Version(1, 0, 0), entity);

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.False(diff.HasChanges);
        Assert.Equal(0, diff.TotalChanges);
        Assert.False(diff.HasBreakingChanges);
    }

    [Fact]
    public void DiffEngine_Compare_AddedEntity_DetectsAsNonBreaking()
    {
        var customer = CreateEntity("Customer", CreateProperty("Id", "System.Guid", true));
        var order = CreateEntity("Order", CreateProperty("Id", "System.Guid", true));

        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0), customer);
        var after = CreateSnapshot("TestDomain", new Version(1, 1, 0), customer, order);

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.Single(diff.EntityChanges);
        Assert.Equal(ChangeType.Added, diff.EntityChanges[0].ChangeType);
        Assert.Equal("Order", diff.EntityChanges[0].EntityName);
        Assert.False(diff.HasBreakingChanges);
    }

    [Fact]
    public void DiffEngine_Compare_RemovedEntity_DetectsAsBreaking()
    {
        var customer = CreateEntity("Customer", CreateProperty("Id", "System.Guid", true));
        var order = CreateEntity("Order", CreateProperty("Id", "System.Guid", true));

        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0), customer, order);
        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0), customer);

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.True(diff.HasBreakingChanges);
        Assert.Single(diff.EntityChanges);
        Assert.Equal(ChangeType.Removed, diff.EntityChanges[0].ChangeType);
    }

    [Fact]
    public void DiffEngine_Compare_AddedOptionalProperty_DetectsAsNonBreaking()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Name", "System.String", true)));

        var after = CreateSnapshot("TestDomain", new Version(1, 1, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Name", "System.String", true),
                CreateProperty("Email", "System.String", false)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.False(diff.HasBreakingChanges);
        Assert.Single(diff.EntityChanges);
        Assert.Single(diff.EntityChanges[0].PropertyChanges);
        Assert.Equal(ChangeType.Added, diff.EntityChanges[0].PropertyChanges[0].ChangeType);
    }

    [Fact]
    public void DiffEngine_Compare_AddedRequiredProperty_DetectsAsBreaking()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true)));

        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Email", "System.String", true)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.True(diff.HasBreakingChanges);
    }

    [Fact]
    public void DiffEngine_Compare_RemovedProperty_DetectsAsBreaking()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Email", "System.String", false)));

        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.True(diff.HasBreakingChanges);
    }

    [Fact]
    public void DiffEngine_Compare_PropertyTypeChange_DetectsAsBreaking()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Age", "System.String", false)));

        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Age", "System.Int32", false)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.True(diff.HasBreakingChanges);
        var propChange = diff.EntityChanges[0].PropertyChanges[0];
        Assert.Equal("System.String", propChange.OldValue);
        Assert.Equal("System.Int32", propChange.NewValue);
    }

    [Fact]
    public void DiffFormatter_FormatAsMarkdown_IncludesBreakingChanges()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer", CreateProperty("Id", "System.Guid", true)));
        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        var formatter = new DiffFormatter();
        var markdown = formatter.FormatAsMarkdown(diff);

        Assert.Contains("# Domain Diff: TestDomain", markdown);
        Assert.Contains("Breaking Changes", markdown);
        Assert.Contains("Entity 'Customer' removed", markdown);
    }

    [Fact]
    public void DiffFormatter_FormatAsJson_ReturnsValidJson()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer", CreateProperty("Id", "System.Guid", true)));
        var after = CreateSnapshot("TestDomain", new Version(1, 1, 0),
            CreateEntity("Customer", CreateProperty("Id", "System.Guid", true)),
            CreateEntity("Order", CreateProperty("Id", "System.Guid", true)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        var formatter = new DiffFormatter();
        var json = formatter.FormatAsJson(diff);

        Assert.Contains("\"domain\":", json);
        Assert.Contains("\"beforeVersion\":", json);
        Assert.Contains("\"afterVersion\":", json);
        Assert.Contains("\"entityChanges\":", json);
    }

    [Fact]
    public void MigrationPlanGenerator_Generate_IncludesAllSections()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Email", "System.String", false)));

        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        var generator = new MigrationPlanGenerator();
        var plan = generator.Generate(diff);

        Assert.Contains("# Migration Plan:", plan);
        Assert.Contains("## Summary", plan);
        Assert.Contains("## Recommended Actions", plan);
        Assert.Contains("**Testing**", plan);
    }

    [Fact]
    public void MigrationPlanGenerator_Generate_NoChanges_ReportsNoMigration()
    {
        var entity = CreateEntity("Customer", CreateProperty("Id", "System.Guid", true));
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0), entity);
        var after = CreateSnapshot("TestDomain", new Version(1, 0, 0), entity);

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        var generator = new MigrationPlanGenerator();
        var plan = generator.Generate(diff);

        Assert.Contains("No changes detected", plan);
    }

    [Fact]
    public void BreakingChangeClassifier_EntityRemoval_IsBreaking()
    {
        var classifier = new BreakingChangeClassifier();

        Assert.True(classifier.IsEntityRemovalBreaking());
        Assert.False(classifier.IsEntityAdditionBreaking());
    }

    [Fact]
    public void BreakingChangeClassifier_PropertyChanges_ClassifiesCorrectly()
    {
        var classifier = new BreakingChangeClassifier();

        Assert.True(classifier.IsPropertyRemovalBreaking());
        Assert.True(classifier.IsPropertyTypeChangeBreaking());
        Assert.True(classifier.IsPropertyAdditionBreaking(isRequired: true));
        Assert.False(classifier.IsPropertyAdditionBreaking(isRequired: false));
    }

    [Fact]
    public void BreakingChangeClassifier_RequiredChange_ClassifiesCorrectly()
    {
        var classifier = new BreakingChangeClassifier();

        // Optional -> Required is breaking
        Assert.True(classifier.IsRequiredChangeBreaking(wasOptional: true, isNowRequired: true));

        // Required -> Optional is not breaking
        Assert.False(classifier.IsRequiredChangeBreaking(wasOptional: false, isNowRequired: false));
    }

    [Fact]
    public void DiffEngine_Compare_PropertyRequiredChange_BreakingWhenOptionalToRequired()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Email", "System.String", false)));

        var afterEntity = CreateEntity("Customer",
            CreateProperty("Id", "System.Guid", true),
            CreateProperty("Email", "System.String", true));

        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0), afterEntity);

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.True(diff.HasBreakingChanges);
    }

    [Fact]
    public void DiffEngine_Compare_PropertyRequiredChange_NonBreakingWhenRequiredToOptional()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Email", "System.String", true)));

        var afterEntity = CreateEntity("Customer",
            CreateProperty("Id", "System.Guid", true),
            CreateProperty("Email", "System.String", false));

        var after = CreateSnapshot("TestDomain", new Version(1, 1, 0), afterEntity);

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.False(diff.HasBreakingChanges);
    }

    [Fact]
    public void DiffEngine_Compare_ModifiedEntity_DetectsChanges()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Name", "System.String", true)));

        var after = CreateSnapshot("TestDomain", new Version(1, 1, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Name", "System.String", true),
                CreateProperty("Email", "System.String", false)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.Equal(1, diff.EntityChanges.Count);
        Assert.Equal(ChangeType.Modified, diff.EntityChanges[0].ChangeType);
        Assert.Single(diff.EntityChanges[0].PropertyChanges);
    }

    [Fact]
    public void DiffEngine_Compare_MultipleChanges_CountsCorrectly()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true)),
            CreateEntity("Order",
                CreateProperty("Id", "System.Guid", true)));

        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Email", "System.String", false)),
            CreateEntity("Product",
                CreateProperty("Id", "System.Guid", true)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);

        Assert.True(diff.HasChanges);
        Assert.Equal(3, diff.TotalChanges); // 1 modified, 1 removed, 1 added
    }

    [Fact]
    public void DiffFormatter_FormatAsJson_ContainsRequiredFields()
    {
        var customer = CreateEntity("Customer", CreateProperty("Id", "System.Guid", true));
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0), customer);
        var after = CreateSnapshot("TestDomain", new Version(1, 1, 0), customer);

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);
        var formatter = new DiffFormatter();

        var json = formatter.FormatAsJson(diff);

        Assert.Contains("\"hasBreakingChanges\":", json);
        Assert.Contains("\"totalChanges\":", json);
        Assert.Contains("\"domain\":", json);
    }

    [Fact]
    public void DiffFormatter_FormatAsMarkdown_NoChanges_IndicatesNoChanges()
    {
        var entity = CreateEntity("Customer", CreateProperty("Id", "System.Guid", true));
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0), entity);
        var after = CreateSnapshot("TestDomain", new Version(1, 0, 0), entity);

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);
        var formatter = new DiffFormatter();

        var markdown = formatter.FormatAsMarkdown(diff);

        Assert.Contains("No changes", markdown);
    }

    [Fact]
    public void DiffFormatter_FormatAsMarkdown_WithNonBreakingChanges_ShowsAdditions()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer", CreateProperty("Id", "System.Guid", true)));

        var after = CreateSnapshot("TestDomain", new Version(1, 1, 0),
            CreateEntity("Customer", CreateProperty("Id", "System.Guid", true)),
            CreateEntity("Order", CreateProperty("Id", "System.Guid", true)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);
        var formatter = new DiffFormatter();

        var markdown = formatter.FormatAsMarkdown(diff);

        Assert.Contains("Entity 'Order' added", markdown);
        Assert.Contains("**Breaking Changes**: No", markdown);
    }

    [Fact]
    public void MigrationPlanGenerator_Generate_WithBreakingChanges_IncludesWarnings()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Email", "System.String", true)));

        var after = CreateSnapshot("TestDomain", new Version(2, 0, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);
        var generator = new MigrationPlanGenerator();

        var plan = generator.Generate(diff);

        Assert.Contains("Breaking Changes", plan);
        Assert.Contains("⚠", plan);
    }

    [Fact]
    public void MigrationPlanGenerator_Generate_WithNonBreakingChanges_ShowsAsNonBreaking()
    {
        var before = CreateSnapshot("TestDomain", new Version(1, 0, 0),
            CreateEntity("Customer", CreateProperty("Id", "System.Guid", true)));

        var after = CreateSnapshot("TestDomain", new Version(1, 1, 0),
            CreateEntity("Customer",
                CreateProperty("Id", "System.Guid", true),
                CreateProperty("Email", "System.String", false)));

        var engine = new DiffEngine();
        var diff = engine.Compare(before, after);
        var generator = new MigrationPlanGenerator();

        var plan = generator.Generate(diff);

        Assert.Contains("Non-Breaking Changes", plan);
        Assert.DoesNotContain("⚠", plan);
    }

    [Fact]
    public void BreakingChangeClassifier_KeyChange_IsBreaking()
    {
        var classifier = new BreakingChangeClassifier();
        Assert.True(classifier.IsKeyChangeBreaking());
    }

    [Fact]
    public void BreakingChangeClassifier_EntityOperations_ClassifiesCorrectly()
    {
        var classifier = new BreakingChangeClassifier();

        Assert.True(classifier.IsEntityRemovalBreaking());
        Assert.False(classifier.IsEntityAdditionBreaking());
    }

    [Fact]
    public void BreakingChangeClassifier_PropertyOperations_ClassifiesCorrectly()
    {
        var classifier = new BreakingChangeClassifier();

        Assert.True(classifier.IsPropertyRemovalBreaking());
        Assert.True(classifier.IsPropertyAdditionBreaking(isRequired: true));
        Assert.False(classifier.IsPropertyAdditionBreaking(isRequired: false));
        Assert.True(classifier.IsPropertyTypeChangeBreaking());
    }

    [Fact]
    public void BreakingChangeClassifier_ValueObjectAndEnumOperations_ClassifiesCorrectly()
    {
        var classifier = new BreakingChangeClassifier();

        Assert.True(classifier.IsValueObjectRemovalBreaking());
        Assert.True(classifier.IsEnumRemovalBreaking());
        Assert.True(classifier.IsEnumValueRemovalBreaking());
    }

    [Fact]
    public void BreakingChangeClassifier_IndexOperations_AreNonBreaking()
    {
        var classifier = new BreakingChangeClassifier();

        Assert.False(classifier.IsIndexAdditionBreaking());
        Assert.False(classifier.IsIndexRemovalBreaking());
    }

    [Fact]
    public void BreakingChangeClassifier_RuleChanges_AreNonBreaking()
    {
        var classifier = new BreakingChangeClassifier();
        Assert.False(classifier.IsRuleChangeBreaking());
    }
}
