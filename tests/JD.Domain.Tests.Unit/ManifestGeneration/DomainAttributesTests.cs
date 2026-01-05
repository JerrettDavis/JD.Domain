using JD.Domain.ManifestGeneration;

namespace JD.Domain.Tests.Unit.ManifestGeneration;

public sealed class DomainAttributesTests
{
    [Fact]
    public void DomainEntityAttribute_SetsProperties()
    {
        var attribute = new DomainEntityAttribute
        {
            TableName = "Customers",
            Schema = "dbo",
            Description = "Customer entity"
        };

        Assert.Equal("Customers", attribute.TableName);
        Assert.Equal("dbo", attribute.Schema);
        Assert.Equal("Customer entity", attribute.Description);
    }

    [Fact]
    public void DomainValueObjectAttribute_SetsDescription()
    {
        var attribute = new DomainValueObjectAttribute
        {
            Description = "Value object description"
        };

        Assert.Equal("Value object description", attribute.Description);
    }
}
