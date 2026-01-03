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

public class SnapshotStorageTests
{
    private readonly string _testDirectory;

    public SnapshotStorageTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
    }

    private void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    private DomainManifest CreateTestManifest(string name = "TestDomain", Version? version = null)
    {
        return new DomainManifest
        {
            Name = name,
            Version = version ?? new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "Customer",
                    TypeName = "TestDomain.Customer",
                    Properties =
                    [
                        new PropertyManifest { Name = "Id", TypeName = "System.Guid", IsRequired = true }
                    ],
                    KeyProperties = ["Id"]
                }
            ]
        };
    }

    [Fact]
    public void Save_WritesFileToCorrectLocation()
    {
        try
        {
            var options = new SnapshotOptions
            {
                OutputDirectory = _testDirectory,
                OrganizeByDomainName = true
            };
            var storage = new SnapshotStorage(options);
            var manifest = CreateTestManifest();

            var snapshot = storage.Save(manifest);

            Assert.NotNull(snapshot);
            var filePath = options.GetFilePath(snapshot.Name, snapshot.Version);
            Assert.True(File.Exists(filePath));
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void Save_ThrowsOnNullManifest()
    {
        var storage = new SnapshotStorage();

        Assert.Throws<ArgumentNullException>(() => storage.Save(null!));
    }

    [Fact]
    public void SaveSnapshot_WritesFileAndReturnsPath()
    {
        try
        {
            var options = new SnapshotOptions { OutputDirectory = _testDirectory };
            var storage = new SnapshotStorage(options);
            var manifest = CreateTestManifest();
            var writer = new SnapshotWriter();
            var snapshot = writer.CreateSnapshot(manifest);

            var filePath = storage.SaveSnapshot(snapshot);

            Assert.NotNull(filePath);
            Assert.True(File.Exists(filePath));
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void SaveSnapshot_ThrowsOnNullSnapshot()
    {
        var storage = new SnapshotStorage();

        Assert.Throws<ArgumentNullException>(() => storage.SaveSnapshot(null!));
    }

    [Fact]
    public void Load_ByFilePath_ReturnsSnapshot()
    {
        try
        {
            var options = new SnapshotOptions { OutputDirectory = _testDirectory };
            var storage = new SnapshotStorage(options);
            var manifest = CreateTestManifest();
            var snapshot = storage.Save(manifest);
            var filePath = options.GetFilePath(snapshot.Name, snapshot.Version);

            var loaded = storage.Load(filePath);

            Assert.Equal(snapshot.Name, loaded.Name);
            Assert.Equal(snapshot.Version, loaded.Version);
            Assert.Equal(snapshot.Hash, loaded.Hash);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void Load_ByNameAndVersion_ReturnsSnapshot()
    {
        try
        {
            var options = new SnapshotOptions { OutputDirectory = _testDirectory };
            var storage = new SnapshotStorage(options);
            var manifest = CreateTestManifest();
            var snapshot = storage.Save(manifest);

            var loaded = storage.Load(snapshot.Name, snapshot.Version);

            Assert.Equal(snapshot.Name, loaded.Name);
            Assert.Equal(snapshot.Version, loaded.Version);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void Load_NonExistentFile_ThrowsFileNotFoundException()
    {
        var storage = new SnapshotStorage();

        Assert.Throws<FileNotFoundException>(() => storage.Load("nonexistent.json"));
    }

    [Fact]
    public void Load_NullOrEmptyPath_ThrowsArgumentException()
    {
        var storage = new SnapshotStorage();

        Assert.Throws<ArgumentException>(() => storage.Load(string.Empty));
        Assert.Throws<ArgumentException>(() => storage.Load(null!));
    }

    [Fact]
    public void Exists_ExistingSnapshot_ReturnsTrue()
    {
        try
        {
            var options = new SnapshotOptions { OutputDirectory = _testDirectory };
            var storage = new SnapshotStorage(options);
            var manifest = CreateTestManifest();
            var snapshot = storage.Save(manifest);

            var exists = storage.Exists(snapshot.Name, snapshot.Version);

            Assert.True(exists);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void Exists_NonExistentSnapshot_ReturnsFalse()
    {
        var options = new SnapshotOptions { OutputDirectory = _testDirectory };
        var storage = new SnapshotStorage(options);

        var exists = storage.Exists("NonExistent", new Version(1, 0, 0));

        Assert.False(exists);
    }

    [Fact]
    public void ListVersions_ReturnsAllVersions()
    {
        try
        {
            var options = new SnapshotOptions
            {
                OutputDirectory = _testDirectory,
                OrganizeByDomainName = true
            };
            var storage = new SnapshotStorage(options);
            storage.Save(CreateTestManifest("TestDomain", new Version(1, 0, 0)));
            storage.Save(CreateTestManifest("TestDomain", new Version(1, 1, 0)));
            storage.Save(CreateTestManifest("TestDomain", new Version(2, 0, 0)));

            var versions = storage.ListVersions("TestDomain");

            Assert.Equal(3, versions.Count);
            Assert.Equal(new Version(1, 0, 0), versions[0]);
            Assert.Equal(new Version(1, 1, 0), versions[1]);
            Assert.Equal(new Version(2, 0, 0), versions[2]);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void ListVersions_NonExistentDomain_ReturnsEmpty()
    {
        var options = new SnapshotOptions { OutputDirectory = _testDirectory };
        var storage = new SnapshotStorage(options);

        var versions = storage.ListVersions("NonExistent");

        Assert.Empty(versions);
    }

    [Fact]
    public void GetLatest_ReturnsNewestVersion()
    {
        try
        {
            var options = new SnapshotOptions
            {
                OutputDirectory = _testDirectory,
                OrganizeByDomainName = true
            };
            var storage = new SnapshotStorage(options);
            storage.Save(CreateTestManifest("TestDomain", new Version(1, 0, 0)));
            storage.Save(CreateTestManifest("TestDomain", new Version(1, 1, 0)));
            storage.Save(CreateTestManifest("TestDomain", new Version(2, 0, 0)));

            var latest = storage.GetLatest("TestDomain");

            Assert.NotNull(latest);
            Assert.Equal(new Version(2, 0, 0), latest.Version);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void GetLatest_NoSnapshots_ReturnsNull()
    {
        var options = new SnapshotOptions { OutputDirectory = _testDirectory };
        var storage = new SnapshotStorage(options);

        var latest = storage.GetLatest("NonExistent");

        Assert.Null(latest);
    }

    [Fact]
    public void Delete_ExistingSnapshot_ReturnsTrue()
    {
        try
        {
            var options = new SnapshotOptions { OutputDirectory = _testDirectory };
            var storage = new SnapshotStorage(options);
            var manifest = CreateTestManifest();
            var snapshot = storage.Save(manifest);

            var deleted = storage.Delete(snapshot.Name, snapshot.Version);

            Assert.True(deleted);
            Assert.False(storage.Exists(snapshot.Name, snapshot.Version));
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void Delete_NonExistentSnapshot_ReturnsFalse()
    {
        var options = new SnapshotOptions { OutputDirectory = _testDirectory };
        var storage = new SnapshotStorage(options);

        var deleted = storage.Delete("NonExistent", new Version(1, 0, 0));

        Assert.False(deleted);
    }

    [Fact]
    public void Constructor_WithoutOptions_UsesDefaults()
    {
        var storage = new SnapshotStorage();

        Assert.NotNull(storage);
    }

    [Fact]
    public void Constructor_WithOptions_UsesProvidedOptions()
    {
        var options = new SnapshotOptions
        {
            OutputDirectory = _testDirectory,
            OrganizeByDomainName = false
        };

        var storage = new SnapshotStorage(options);

        Assert.NotNull(storage);
    }
}
