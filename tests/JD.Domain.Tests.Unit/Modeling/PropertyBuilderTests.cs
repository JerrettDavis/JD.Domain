using JD.Domain.Abstractions;
using JD.Domain.Modeling;

namespace JD.Domain.Tests.Unit.Modeling;

public sealed class PropertyBuilderTests
{
    private sealed class Pricing
    {
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    private sealed class ValueObjectSample
    {
        public int Amount { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? Notes { get; set; }
    }

    [Fact]
    public void HasPrecision_SetsPrecisionAndScale()
    {
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Pricing>(e => e.Property(x => x.Amount).HasPrecision(10, 2))
            .BuildManifest();

        var amount = manifest.Entities[0].Properties.Single(p => p.Name == nameof(Pricing.Amount));
        Assert.Equal(10, amount.Precision);
        Assert.Equal(2, amount.Scale);
    }

    [Fact]
    public void HasPrecision_ThrowsOnInvalidValues()
    {
        var builder = new EntityBuilder<Pricing>();

        Assert.Throws<ArgumentException>(() => builder.Property(x => x.Amount).HasPrecision(0));
        Assert.Throws<ArgumentException>(() => builder.Property(x => x.Amount).HasPrecision(2, -1));
    }

    [Fact]
    public void IsConcurrencyToken_SetsFlag()
    {
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Pricing>(e => e.Property(x => x.RowVersion).IsConcurrencyToken())
            .BuildManifest();

        var rowVersion = manifest.Entities[0].Properties.Single(p => p.Name == nameof(Pricing.RowVersion));
        Assert.True(rowVersion.IsConcurrencyToken);
    }

    [Fact]
    public void IsComputed_SetsFlag()
    {
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Pricing>(e => e.Property(x => x.Amount).IsComputed())
            .BuildManifest();

        var amount = manifest.Entities[0].Properties.Single(p => p.Name == nameof(Pricing.Amount));
        Assert.True(amount.IsComputed);
    }

    [Fact]
    public void HasMaxLength_ThrowsOnInvalidValue()
    {
        var builder = new EntityBuilder<Pricing>();

        Assert.Throws<ArgumentException>(() => builder.Property(x => x.Notes!).HasMaxLength(0));
    }

    [Fact]
    public void EntityBuilder_ThrowsOnInvalidPropertyExpression()
    {
        var builder = new EntityBuilder<Pricing>();

        Assert.Throws<ArgumentException>(() => builder.Property(x => x.ToString()!));
    }

    [Fact]
    public void ValueObjectBuilder_DetectsCollectionsAndNullability()
    {
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .ValueObject<ValueObjectSample>()
            .BuildManifest();

        var valueObject = manifest.ValueObjects.Single();
        var tags = valueObject.Properties.Single(p => p.Name == nameof(ValueObjectSample.Tags));
        var notes = valueObject.Properties.Single(p => p.Name == nameof(ValueObjectSample.Notes));

        Assert.True(tags.IsCollection);
        Assert.False(notes.IsCollection);
    }
}
