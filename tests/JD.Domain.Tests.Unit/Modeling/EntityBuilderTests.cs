using System.Reflection;
using JD.Domain.Abstractions;
using JD.Domain.Modeling;

namespace JD.Domain.Tests.Unit.Modeling;

public sealed class EntityBuilderTests
{
    private sealed class SampleEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class FieldEntity
    {
        public string Name = string.Empty;
    }

    [Fact]
    public void Property_CreatesManifest_WhenPropertyMissing()
    {
        var builder = new EntityBuilder<SampleEntity>();
        var field = typeof(EntityBuilder<SampleEntity>).GetField("_properties", BindingFlags.NonPublic | BindingFlags.Instance);
        var properties = Assert.IsType<List<PropertyManifest>>(field?.GetValue(builder));
        properties.Clear();

        builder.Property(x => x.Name);

        Assert.Single(properties);
        Assert.Equal(nameof(SampleEntity.Name), properties[0].Name);
    }

    [Fact]
    public void WithMetadata_AddsMetadataToManifest()
    {
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<SampleEntity>(e => e.WithMetadata("Origin", "Tests"))
            .BuildManifest();

        var entity = manifest.Entities.Single();
        Assert.Equal("Tests", entity.Metadata["Origin"]);
    }

    [Fact]
    public void Property_WithFieldExpression_Throws()
    {
        var builder = new EntityBuilder<FieldEntity>();

        Assert.Throws<ArgumentException>(() => builder.Property(x => x.Name));
    }
}
