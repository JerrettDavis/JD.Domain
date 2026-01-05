namespace JD.Domain.Tests.Unit.Modeling;

/// <summary>
/// Tests for the Domain entry point and DomainBuilder.
/// </summary>
public sealed class DomainBuilderTests
{
    // Test entity for demonstrations
    private class Blog
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private enum BlogStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    [Fact]
    public void Create_WithValidName_ReturnsDomainBuilder()
    {
        // Arrange & Act
        var builder = JD.Domain.Modeling.Domain.Create("TestDomain");

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void Create_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => JD.Domain.Modeling.Domain.Create(null!));
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => JD.Domain.Modeling.Domain.Create(string.Empty));
    }

    [Fact]
    public void BuildManifest_WithBasicConfiguration_CreatesManifest()
    {
        // Arrange
        var builder = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Version(1, 0, 0);

        // Act
        var manifest = builder.BuildManifest();

        // Assert
        Assert.NotNull(manifest);
        Assert.Equal("TestDomain", manifest.Name);
        Assert.Equal(new Version(1, 0, 0), manifest.Version);
    }

    [Fact]
    public void Build_ReturnsManifest()
    {
        var builder = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Version(1, 0, 0);

        var manifest = builder.Build();

        Assert.Equal("TestDomain", manifest.Name);
        Assert.Equal(new Version(1, 0, 0), manifest.Version);
    }

    [Fact]
    public void Entity_WithTypeParameter_AddsEntityToManifest()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .BuildManifest();

        // Assert
        Assert.Single(manifest.Entities);
        Assert.Equal("Blog", manifest.Entities[0].Name);
        Assert.Equal(4, manifest.Entities[0].Properties.Count); // Id, Name, Description, CreatedAt
    }

    [Fact]
    public void Entity_WithConfiguration_AppliesConfiguration()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>(e =>
            {
                e.Key(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.ToTable("Blogs", "dbo");
            })
            .BuildManifest();

        // Assert
        var entity = manifest.Entities[0];
        Assert.Equal("Blogs", entity.TableName);
        Assert.Equal("dbo", entity.SchemaName);
        Assert.Contains("Id", entity.KeyProperties);
    }

    [Fact]
    public void Enum_AddsEnumerationToManifest()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Enum<BlogStatus>()
            .BuildManifest();

        // Assert
        Assert.Single(manifest.Enums);
        Assert.Equal("BlogStatus", manifest.Enums[0].Name);
        Assert.Equal(3, manifest.Enums[0].Values.Count);
        Assert.True(manifest.Enums[0].Values.ContainsKey("Draft"));
        Assert.True(manifest.Enums[0].Values.ContainsKey("Published"));
        Assert.True(manifest.Enums[0].Values.ContainsKey("Archived"));
    }

    [Fact]
    public void WithMetadata_AddsMetadataToManifest()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .WithMetadata("Author", "Test Author")
            .WithMetadata("Version", "1.0")
            .BuildManifest();

        // Assert
        Assert.Equal(2, manifest.Metadata.Count);
        Assert.Equal("Test Author", manifest.Metadata["Author"]);
        Assert.Equal("1.0", manifest.Metadata["Version"]);
    }

    [Fact]
    public void BuildManifest_SetsCreatedAtTimestamp()
    {
        // Act
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain").BuildManifest();

        // Assert
        Assert.Equal(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero), manifest.CreatedAt);
    }

    [Fact]
    public void BuildManifest_IncludesDSLSource()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain").BuildManifest();

        // Assert
        Assert.NotEmpty(manifest.Sources);
        var dslSource = manifest.Sources.FirstOrDefault(s => s.Type == "DSL");
        Assert.NotNull(dslSource);
        Assert.Equal("Fluent API", dslSource.Location);
    }

    private class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    [Fact]
    public void Entity_WithMultipleEntities_AddsAllToManifest()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Entity<Post>()
            .BuildManifest();

        // Assert
        Assert.Equal(2, manifest.Entities.Count);
        Assert.Contains(manifest.Entities, e => e.Name == "Blog");
        Assert.Contains(manifest.Entities, e => e.Name == "Post");
    }

    private class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }

    [Fact]
    public void ValueObject_AddsValueObjectToManifest()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .ValueObject<Address>()
            .BuildManifest();

        // Assert
        Assert.Single(manifest.ValueObjects);
        Assert.Equal("Address", manifest.ValueObjects[0].Name);
        Assert.Equal(3, manifest.ValueObjects[0].Properties.Count);
    }

    [Fact]
    public void ValueObject_WithMetadata_AddsMetadataToManifest()
    {
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .ValueObject<Address>(vo => vo.WithMetadata("Source", "Tests"))
            .BuildManifest();

        var valueObject = manifest.ValueObjects.Single();
        Assert.Equal("Tests", valueObject.Metadata["Source"]);
    }
}
