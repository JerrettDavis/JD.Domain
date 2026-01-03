using JD.Domain.Abstractions;
using JD.Domain.Snapshot;
using Xunit;

namespace JD.Domain.Tests.Unit.Snapshot;

public class SnapshotTests
{
    private static DomainManifest CreateTestManifest()
    {
        return new DomainManifest
        {
            Name = "TestDomain",
            Version = new Version(1, 0, 0),
            CreatedAt = DateTimeOffset.UtcNow,
            Entities =
            [
                new EntityManifest
                {
                    Name = "Customer",
                    TypeName = "TestDomain.Customer",
                    Properties =
                    [
                        new PropertyManifest { Name = "Id", TypeName = "System.Guid", IsRequired = true },
                        new PropertyManifest { Name = "Name", TypeName = "System.String", IsRequired = true, MaxLength = 100 },
                        new PropertyManifest { Name = "Email", TypeName = "System.String", IsRequired = false }
                    ],
                    KeyProperties = ["Id"]
                }
            ],
            RuleSets =
            [
                new RuleSetManifest
                {
                    Name = "Default",
                    TargetType = "TestDomain.Customer",
                    Rules =
                    [
                        new RuleManifest
                        {
                            Id = "Customer.Name.Required",
                            Category = "Invariant",
                            TargetType = "TestDomain.Customer",
                            Message = "Name is required",
                            Severity = RuleSeverity.Error
                        }
                    ]
                }
            ]
        };
    }

    [Fact]
    public void SnapshotWriter_CreateSnapshot_GeneratesHashAndTimestamp()
    {
        var manifest = CreateTestManifest();
        var writer = new SnapshotWriter();

        var snapshot = writer.CreateSnapshot(manifest);

        Assert.Equal("TestDomain", snapshot.Name);
        Assert.Equal(new Version(1, 0, 0), snapshot.Version);
        Assert.NotEmpty(snapshot.Hash);
        Assert.True(snapshot.CreatedAt > DateTimeOffset.UtcNow.AddMinutes(-1));
        Assert.Same(manifest, snapshot.Manifest);
    }

    [Fact]
    public void SnapshotWriter_CreateSnapshot_SameManifest_ProducesSameHash()
    {
        var manifest = CreateTestManifest();
        var writer = new SnapshotWriter();

        var snapshot1 = writer.CreateSnapshot(manifest);
        var snapshot2 = writer.CreateSnapshot(manifest);

        Assert.Equal(snapshot1.Hash, snapshot2.Hash);
    }

    [Fact]
    public void SnapshotWriter_Serialize_ProducesCanonicalJson()
    {
        var manifest = CreateTestManifest();
        var writer = new SnapshotWriter();
        var snapshot = writer.CreateSnapshot(manifest);

        var json = writer.Serialize(snapshot);

        Assert.Contains("\"$schema\":", json);
        Assert.Contains("\"name\": \"TestDomain\"", json);
        Assert.Contains("\"version\": \"1.0.0\"", json);
        Assert.Contains("\"hash\":", json);
    }

    [Fact]
    public void SnapshotReader_Deserialize_ReturnsCorrectSnapshot()
    {
        var manifest = CreateTestManifest();
        var writer = new SnapshotWriter();
        var originalSnapshot = writer.CreateSnapshot(manifest);
        var json = writer.Serialize(originalSnapshot);

        var reader = new SnapshotReader();
        var deserializedSnapshot = reader.Deserialize(json);

        Assert.Equal(originalSnapshot.Name, deserializedSnapshot.Name);
        Assert.Equal(originalSnapshot.Version, deserializedSnapshot.Version);
        Assert.Equal(originalSnapshot.Hash, deserializedSnapshot.Hash);
        Assert.Equal(manifest.Entities.Count, deserializedSnapshot.Manifest.Entities.Count);
    }

    [Fact]
    public void SnapshotReader_DeserializeManifest_ParsesEntities()
    {
        var manifestJson = """
        {
            "name": "TestDomain",
            "version": "1.0.0",
            "createdAt": "2025-01-01T00:00:00Z",
            "entities": [
                {
                    "name": "Customer",
                    "typeName": "TestDomain.Customer",
                    "properties": [
                        { "name": "Id", "typeName": "System.Guid", "isRequired": true }
                    ],
                    "keyProperties": ["Id"]
                }
            ],
            "valueObjects": [],
            "enums": [],
            "ruleSets": [],
            "configurations": [],
            "sources": []
        }
        """;

        var reader = new SnapshotReader();
        var manifest = reader.DeserializeManifest(manifestJson);

        Assert.Equal("TestDomain", manifest.Name);
        Assert.Single(manifest.Entities);
        Assert.Equal("Customer", manifest.Entities[0].Name);
    }

    [Fact]
    public void SnapshotOptions_GetFilePath_OrganizesByDomainName()
    {
        var options = new SnapshotOptions
        {
            OutputDirectory = "snapshots",
            OrganizeByDomainName = true
        };

        var path = options.GetFilePath("TestDomain", new Version(1, 0, 0));

        Assert.Contains("TestDomain", path);
        Assert.Contains("v1.0.0.json", path);
    }

    [Fact]
    public void SnapshotOptions_FormatFileName_AppliesPattern()
    {
        var options = new SnapshotOptions
        {
            FileNamePattern = "{name}_{version}.json"
        };

        var fileName = options.FormatFileName("TestDomain", new Version(2, 1, 0));

        Assert.Equal("TestDomain_2.1.0.json", fileName);
    }

    [Fact]
    public void SnapshotWriter_Serialize_SortsArraysAlphabetically()
    {
        var manifest = new DomainManifest
        {
            Name = "TestDomain",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest { Name = "Zebra", TypeName = "TestDomain.Zebra", KeyProperties = ["Id"] },
                new EntityManifest { Name = "Apple", TypeName = "TestDomain.Apple", KeyProperties = ["Id"] },
                new EntityManifest { Name = "Mango", TypeName = "TestDomain.Mango", KeyProperties = ["Id"] }
            ]
        };
        var writer = new SnapshotWriter();
        var snapshot = writer.CreateSnapshot(manifest);

        var json = writer.Serialize(snapshot);

        var appleIndex = json.IndexOf("\"Apple\"");
        var mangoIndex = json.IndexOf("\"Mango\"");
        var zebraIndex = json.IndexOf("\"Zebra\"");
        Assert.True(appleIndex < mangoIndex);
        Assert.True(mangoIndex < zebraIndex);
    }
}
