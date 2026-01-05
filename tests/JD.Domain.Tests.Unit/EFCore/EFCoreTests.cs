using JD.Domain.Abstractions;
using JD.Domain.EFCore;
using Microsoft.EntityFrameworkCore;

namespace JD.Domain.Tests.Unit.EFCore;

public class EFCoreTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    private DbContextOptions<TestDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void ApplyDomainManifest_ThrowsOnNullModelBuilder()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0)
        };

        Assert.Throws<ArgumentNullException>(() =>
            ((ModelBuilder)null!).ApplyDomainManifest(manifest));
    }

    [Fact]
    public void ApplyDomainManifest_ThrowsOnNullManifest()
    {
        using var context = new TestDbContext(CreateOptions());
        var modelBuilder = new ModelBuilder();

        Assert.Throws<ArgumentNullException>(() =>
            modelBuilder.ApplyDomainManifest(null!));
    }

    [Fact]
    public void ApplyDomainManifest_WithEntityConfiguration_AppliesTableMapping()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "TestEntity",
                    TypeName = typeof(TestEntity).FullName!,
                    TableName = "test_entities",
                    SchemaName = "dbo"
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));

        Assert.NotNull(entityType);
        Assert.Equal("test_entities", entityType.GetTableName());
        Assert.Equal("dbo", entityType.GetSchema());
    }

    [Fact]
    public void ApplyDomainManifest_WithEntityConfiguration_TableNameOnly()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "TestEntity",
                    TypeName = typeof(TestEntity).FullName!,
                    TableName = "test_entities"
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));

        Assert.NotNull(entityType);
        Assert.Equal("test_entities", entityType.GetTableName());
    }

    [Fact]
    public void ApplyDomainManifest_WithKeyProperties_AppliesKey()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "TestEntity",
                    TypeName = typeof(TestEntity).FullName!,
                    KeyProperties = ["Id"]
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>().HasNoKey();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));

        Assert.NotNull(entityType);
        var key = entityType.FindPrimaryKey();
        Assert.NotNull(key);
        Assert.Single(key.Properties);
        Assert.Equal("Id", key.Properties[0].Name);
    }

    [Fact]
    public void ApplyDomainManifest_WithPropertyConfiguration_AppliesRequired()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "TestEntity",
                    TypeName = typeof(TestEntity).FullName!,
                    Properties =
                    [
                        new PropertyManifest
                        {
                            Name = "Name",
                            TypeName = "System.String",
                            IsRequired = true
                        }
                    ]
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));
        var property = entityType!.FindProperty("Name");

        Assert.NotNull(property);
        Assert.False(property.IsNullable);
    }

    [Fact]
    public void ApplyDomainManifest_WithPropertyConfiguration_AppliesMaxLength()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "TestEntity",
                    TypeName = typeof(TestEntity).FullName!,
                    Properties =
                    [
                        new PropertyManifest
                        {
                            Name = "Name",
                            TypeName = "System.String",
                            MaxLength = 100
                        }
                    ]
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));
        var property = entityType!.FindProperty("Name");

        Assert.NotNull(property);
        Assert.Equal(100, property.GetMaxLength());
    }

    [Fact]
    public void ApplyDomainManifest_WithConfiguration_TableNameOnly()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Configurations =
            [
                new ConfigurationManifest
                {
                    EntityName = "TestEntity",
                    EntityTypeName = typeof(TestEntity).FullName!,
                    TableName = "config_entities"
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));

        Assert.NotNull(entityType);
        Assert.Equal("config_entities", entityType.GetTableName());
    }

    [Fact]
    public void ApplyDomainManifest_SkipsConfiguration_WhenEntityMissing()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Configurations =
            [
                new ConfigurationManifest
                {
                    EntityName = "MissingEntity",
                    EntityTypeName = "Missing.Type",
                    TableName = "ignored"
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        Assert.Null(model.FindEntityType("Missing.Type"));
    }

    [Fact]
    public void ApplyDomainManifest_WithConfigurationManifest_AppliesTableMapping()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Configurations =
            [
                new ConfigurationManifest
                {
                    EntityName = "TestEntity",
                    EntityTypeName = typeof(TestEntity).FullName!,
                    TableName = "configured_entities",
                    SchemaName = "config"
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));

        Assert.NotNull(entityType);
        Assert.Equal("configured_entities", entityType.GetTableName());
        Assert.Equal("config", entityType.GetSchema());
    }

    [Fact]
    public void ApplyDomainManifest_WithConfigurationManifest_AppliesIndexes()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Configurations =
            [
                new ConfigurationManifest
                {
                    EntityName = "TestEntity",
                    EntityTypeName = typeof(TestEntity).FullName!,
                    Indexes =
                    [
                        new IndexManifest
                        {
                            Properties = ["Name"],
                            IsUnique = true,
                            Filter = "Name IS NOT NULL"
                        }
                    ]
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));
        var indexes = entityType!.GetIndexes();

        Assert.NotEmpty(indexes);
        var index = indexes.First();
        Assert.True(index.IsUnique);
        Assert.Equal("Name IS NOT NULL", index.GetFilter());
    }

    [Fact]
    public void ApplyDomainManifest_WithConfigurationManifest_AppliesKeyProperties()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Configurations =
            [
                new ConfigurationManifest
                {
                    EntityName = "TestEntity",
                    EntityTypeName = typeof(TestEntity).FullName!,
                    KeyProperties = ["Id"]
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>().HasNoKey();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));
        var key = entityType!.FindPrimaryKey();

        Assert.NotNull(key);
        Assert.Single(key.Properties);
        Assert.Equal("Id", key.Properties[0].Name);
    }

    [Fact]
    public void ApplyDomainManifest_WithConfigurationManifest_AppliesPropertyConfigurations()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Configurations =
            [
                new ConfigurationManifest
                {
                    EntityName = "TestEntity",
                    EntityTypeName = typeof(TestEntity).FullName!,
                    PropertyConfigurations = new Dictionary<string, PropertyConfigurationManifest>
                    {
                        ["Name"] = new PropertyConfigurationManifest
                        {
                            PropertyName = "Name",
                            IsRequired = true,
                            MaxLength = 200
                        }
                    }
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));
        var property = entityType!.FindProperty("Name");

        Assert.NotNull(property);
        Assert.False(property.IsNullable);
        Assert.Equal(200, property.GetMaxLength());
    }

    [Fact]
    public void ApplyDomainManifest_SkipsUnregisteredEntities()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "UnregisteredEntity",
                    TypeName = "NonExistent.Entity",
                    TableName = "test"
                }
            ]
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();

        // Should not throw
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        Assert.Null(model.FindEntityType("NonExistent.Entity"));
    }

    [Fact]
    public void ApplyDomainManifest_EmptyManifest_DoesNothing()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0)
        };

        var builder = new ModelBuilder();
        builder.Entity<TestEntity>();

        // Should not throw
        builder.ApplyDomainManifest(manifest);

        var model = builder.FinalizeModel();
        var entityType = model.FindEntityType(typeof(TestEntity));
        Assert.NotNull(entityType);
    }

    [Fact]
    public void ApplyDomainManifest_ReturnsModelBuilder()
    {
        var manifest = new DomainManifest
        {
            Name = "Test",
            Version = new Version(1, 0, 0)
        };

        var builder = new ModelBuilder();
        var result = builder.ApplyDomainManifest(manifest);

        Assert.Same(builder, result);
    }
}
