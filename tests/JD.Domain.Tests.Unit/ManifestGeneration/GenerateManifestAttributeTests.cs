using JD.Domain.ManifestGeneration;

namespace JD.Domain.Tests.Unit.ManifestGeneration;

public sealed class GenerateManifestAttributeTests
{
    [Fact]
    public void Constructor_ThrowsOnInvalidName()
    {
        Assert.Throws<ArgumentException>(() => new GenerateManifestAttribute(string.Empty));
        Assert.Throws<ArgumentException>(() => new GenerateManifestAttribute(" "));
    }

    [Fact]
    public void Constructor_SetsNameAndProperties()
    {
        var attribute = new GenerateManifestAttribute("TestDomain")
        {
            Version = "1.2.3",
            OutputPath = "out/path",
            Namespace = "Test.Namespace"
        };

        Assert.Equal("TestDomain", attribute.Name);
        Assert.Equal("1.2.3", attribute.Version);
        Assert.Equal("out/path", attribute.OutputPath);
        Assert.Equal("Test.Namespace", attribute.Namespace);
    }
}
