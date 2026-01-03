using JD.Domain.Configuration;
using JD.Domain.Modeling;

namespace JD.Domain.Tests.Unit.Configuration;

public class ConfigurationTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    [Fact]
    public void EntityConfigurationBuilder_ToTable_SetsTableName()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c.ToTable("test_entities"));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Equal("test_entities", manifest.Configurations[0].TableName);
        Assert.Null(manifest.Configurations[0].SchemaName);
    }

    [Fact]
    public void EntityConfigurationBuilder_ToTable_WithSchema_SetsBoth()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c.ToTable("test_entities", "dbo"));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Equal("test_entities", manifest.Configurations[0].TableName);
        Assert.Equal("dbo", manifest.Configurations[0].SchemaName);
    }

    [Fact]
    public void EntityConfigurationBuilder_ToTable_ThrowsOnNullTableName()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.ToTable(null!));
    }

    [Fact]
    public void EntityConfigurationBuilder_ToTable_ThrowsOnEmptyTableName()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.ToTable(string.Empty));
    }

    [Fact]
    public void EntityConfigurationBuilder_ToTable_ThrowsOnWhitespaceTableName()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.ToTable("   "));
    }

    [Fact]
    public void EntityConfigurationBuilder_HasIndex_CreatesIndex()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c.HasIndex("Name"));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Single(manifest.Configurations[0].Indexes);
        Assert.Single(manifest.Configurations[0].Indexes[0].Properties);
        Assert.Equal("Name", manifest.Configurations[0].Indexes[0].Properties[0]);
    }

    [Fact]
    public void EntityConfigurationBuilder_HasIndex_MultipleProperties_CreatesCompositeIndex()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c.HasIndex("Name", "Email"));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Single(manifest.Configurations[0].Indexes);
        Assert.Equal(2, manifest.Configurations[0].Indexes[0].Properties.Count);
        Assert.Equal("Name", manifest.Configurations[0].Indexes[0].Properties[0]);
        Assert.Equal("Email", manifest.Configurations[0].Indexes[0].Properties[1]);
    }

    [Fact]
    public void EntityConfigurationBuilder_HasIndex_ThrowsOnNullProperties()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        Assert.Throws<ArgumentNullException>(() => builder.HasIndex(null!));
    }

    [Fact]
    public void EntityConfigurationBuilder_HasIndex_ThrowsOnEmptyProperties()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.HasIndex());
    }

    [Fact]
    public void EntityConfigurationBuilder_WithMetadata_AddsMetadata()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c
            .WithMetadata("key1", "value1")
            .WithMetadata("key2", 42));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Equal(2, manifest.Configurations[0].Metadata.Count);
        Assert.Equal("value1", manifest.Configurations[0].Metadata["key1"]);
        Assert.Equal(42, manifest.Configurations[0].Metadata["key2"]);
    }

    [Fact]
    public void EntityConfigurationBuilder_WithMetadata_ThrowsOnNullKey()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.WithMetadata(null!, "value"));
    }

    [Fact]
    public void EntityConfigurationBuilder_WithMetadata_ThrowsOnEmptyKey()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.WithMetadata(string.Empty, "value"));
    }

    [Fact]
    public void EntityConfigurationBuilder_WithMetadata_ThrowsOnWhitespaceKey()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.WithMetadata("   ", "value"));
    }

    [Fact]
    public void EntityConfigurationBuilder_Build_SetsEntityName()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => { });

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Equal("TestEntity", manifest.Configurations[0].EntityName);
        Assert.Equal(typeof(TestEntity).FullName, manifest.Configurations[0].EntityTypeName);
    }

    [Fact]
    public void EntityConfigurationBuilder_Build_DefaultsEmptyCollections()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => { });

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Empty(manifest.Configurations[0].Indexes);
        Assert.Empty(manifest.Configurations[0].Relationships);
        Assert.Empty(manifest.Configurations[0].Metadata);
    }

    [Fact]
    public void EntityConfigurationBuilder_FluentChaining_Works()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c
            .ToTable("test_entities", "dbo")
            .WithMetadata("version", 1));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Equal("test_entities", manifest.Configurations[0].TableName);
        Assert.Equal("dbo", manifest.Configurations[0].SchemaName);
        Assert.Single(manifest.Configurations[0].Metadata);
    }

    [Fact]
    public void IndexBuilder_IsUnique_SetsUniqueFlag()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c.HasIndex("Name").IsUnique());

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Single(manifest.Configurations[0].Indexes);
        Assert.True(manifest.Configurations[0].Indexes[0].IsUnique);
    }

    [Fact]
    public void IndexBuilder_IsUnique_WithFalse_SetsFlag()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c.HasIndex("Name").IsUnique(false));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Single(manifest.Configurations[0].Indexes);
        Assert.False(manifest.Configurations[0].Indexes[0].IsUnique);
    }

    [Fact]
    public void IndexBuilder_HasFilter_SetsFilter()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c.HasIndex("Name").HasFilter("Name IS NOT NULL"));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Single(manifest.Configurations[0].Indexes);
        Assert.Equal("Name IS NOT NULL", manifest.Configurations[0].Indexes[0].Filter);
    }

    [Fact]
    public void IndexBuilder_HasFilter_ThrowsOnNullFilter()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        var indexBuilder = builder.HasIndex("Name");

        Assert.Throws<ArgumentException>(() => indexBuilder.HasFilter(null!));
    }

    [Fact]
    public void IndexBuilder_HasFilter_ThrowsOnEmptyFilter()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        var indexBuilder = builder.HasIndex("Name");

        Assert.Throws<ArgumentException>(() => indexBuilder.HasFilter(string.Empty));
    }

    [Fact]
    public void IndexBuilder_HasFilter_ThrowsOnWhitespaceFilter()
    {
        var builder = new EntityConfigurationBuilder<TestEntity>();
        var indexBuilder = builder.HasIndex("Name");

        Assert.Throws<ArgumentException>(() => indexBuilder.HasFilter("   "));
    }

    [Fact]
    public void IndexBuilder_FluentChaining_Works()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c => c.HasIndex("Name")
            .IsUnique()
            .HasFilter("Name IS NOT NULL"));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Single(manifest.Configurations[0].Indexes);
        Assert.True(manifest.Configurations[0].Indexes[0].IsUnique);
        Assert.Equal("Name IS NOT NULL", manifest.Configurations[0].Indexes[0].Filter);
    }

    [Fact]
    public void DomainBuilderConfigurationExtensions_Configure_AddsConfiguration()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();

        domain.Configure<TestEntity>(c => c.ToTable("test_entities"));

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Equal("test_entities", manifest.Configurations[0].TableName);
    }

    [Fact]
    public void DomainBuilderConfigurationExtensions_Configure_ThrowsOnNullBuilder()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((DomainBuilder)null!).Configure<TestEntity>(c => { }));
    }

    [Fact]
    public void DomainBuilderConfigurationExtensions_Configure_ThrowsOnNullConfigure()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        Assert.Throws<ArgumentNullException>(() =>
            domain.Configure<TestEntity>(null!));
    }

    [Fact]
    public void EntityConfigurationBuilder_MultipleIndexes_Works()
    {
        var domain = JD.Domain.Modeling.Domain.Create("TestDomain");
        domain.Entity<TestEntity>();
        domain.Configure<TestEntity>(c =>
        {
            c.HasIndex("Name").IsUnique();
            c.HasIndex("Email");
        });

        var manifest = domain.BuildManifest();

        Assert.Single(manifest.Configurations);
        Assert.Equal(2, manifest.Configurations[0].Indexes.Count);
        Assert.True(manifest.Configurations[0].Indexes[0].IsUnique);
        Assert.False(manifest.Configurations[0].Indexes[1].IsUnique);
    }
}
