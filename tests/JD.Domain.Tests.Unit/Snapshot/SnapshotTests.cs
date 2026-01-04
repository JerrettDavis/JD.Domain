using JD.Domain.Abstractions;
using JD.Domain.Snapshot;

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

    [Fact]
    public void DomainSnapshot_Create_ThrowsOnNullManifest()
    {
        Assert.Throws<ArgumentNullException>(() => DomainSnapshot.Create(null!, "hash123"));
    }

    [Fact]
    public void DomainSnapshot_Create_ThrowsOnNullHash()
    {
        var manifest = CreateTestManifest();
        Assert.Throws<ArgumentException>(() => DomainSnapshot.Create(manifest, null!));
    }

    [Fact]
    public void DomainSnapshot_Create_ThrowsOnEmptyHash()
    {
        var manifest = CreateTestManifest();
        Assert.Throws<ArgumentException>(() => DomainSnapshot.Create(manifest, string.Empty));
    }

    [Fact]
    public void SnapshotWriter_Constructor_WithOptions_UsesProvidedOptions()
    {
        var options = new SnapshotOptions { IndentedJson = false };
        var writer = new SnapshotWriter(options);
        var manifest = CreateTestManifest();
        var snapshot = writer.CreateSnapshot(manifest);

        var json = writer.Serialize(snapshot);

        Assert.DoesNotContain("\n", json); // Not indented
    }

    [Fact]
    public void SnapshotWriter_CreateSnapshot_ThrowsOnNullManifest()
    {
        var writer = new SnapshotWriter();
        Assert.Throws<ArgumentNullException>(() => writer.CreateSnapshot(null!));
    }

    [Fact]
    public void SnapshotWriter_Serialize_ThrowsOnNullSnapshot()
    {
        var writer = new SnapshotWriter();
        Assert.Throws<ArgumentNullException>(() => writer.Serialize(null!));
    }

    [Fact]
    public void SnapshotWriter_SerializeManifest_ThrowsOnNullManifest()
    {
        var writer = new SnapshotWriter();
        Assert.Throws<ArgumentNullException>(() => writer.SerializeManifest(null!));
    }

    [Fact]
    public void SnapshotWriter_Serialize_WithComplexManifest_IncludesAllElements()
    {
        var manifest = new DomainManifest
        {
            Name = "ComplexDomain",
            Version = new Version(2, 0, 0),
            Hash = "test-hash",
            Entities =
            [
                new EntityManifest
                {
                    Name = "Customer",
                    TypeName = "ComplexDomain.Customer",
                    Namespace = "ComplexDomain",
                    TableName = "Customers",
                    SchemaName = "dbo",
                    Properties =
                    [
                        new PropertyManifest
                        {
                            Name = "Id",
                            TypeName = "System.Guid",
                            IsRequired = true
                        },
                        new PropertyManifest
                        {
                            Name = "Name",
                            TypeName = "System.String",
                            IsRequired = true,
                            MaxLength = 100
                        },
                        new PropertyManifest
                        {
                            Name = "Tags",
                            TypeName = "System.Collections.Generic.List<string>",
                            IsCollection = true
                        },
                        new PropertyManifest
                        {
                            Name = "Balance",
                            TypeName = "System.Decimal",
                            Precision = 18,
                            Scale = 2
                        },
                        new PropertyManifest
                        {
                            Name = "RowVersion",
                            TypeName = "System.Byte[]",
                            IsConcurrencyToken = true
                        },
                        new PropertyManifest
                        {
                            Name = "CreatedDate",
                            TypeName = "System.DateTime",
                            IsComputed = true
                        }
                    ],
                    KeyProperties = ["Id"],
                    Metadata = new Dictionary<string, object?> { ["Description"] = "Customer entity" }
                }
            ],
            ValueObjects =
            [
                new ValueObjectManifest
                {
                    Name = "Address",
                    TypeName = "ComplexDomain.Address",
                    Namespace = "ComplexDomain",
                    Properties =
                    [
                        new PropertyManifest { Name = "Street", TypeName = "System.String", MaxLength = 200 }
                    ],
                    Metadata = new Dictionary<string, object?> { ["ValueObject"] = true }
                }
            ],
            Enums =
            [
                new EnumManifest
                {
                    Name = "Status",
                    TypeName = "ComplexDomain.Status",
                    Namespace = "ComplexDomain",
                    UnderlyingType = "System.Byte",
                    Values = new Dictionary<string, object> { ["Active"] = 1, ["Inactive"] = 2 },
                    Metadata = new Dictionary<string, object?> { ["Enum"] = true }
                }
            ],
            RuleSets =
            [
                new RuleSetManifest
                {
                    Name = "Default",
                    TargetType = "ComplexDomain.Customer",
                    Rules =
                    [
                        new RuleManifest
                        {
                            Id = "NameRequired",
                            Category = "Invariant",
                            TargetType = "ComplexDomain.Customer",
                            Message = "Name is required",
                            Severity = RuleSeverity.Critical,
                            Expression = "!string.IsNullOrEmpty(Name)",
                            Tags = ["Validation", "Critical"],
                            Metadata = new Dictionary<string, object?> { ["Priority"] = 1 }
                        }
                    ],
                    Includes = ["BaseRules"],
                    Metadata = new Dictionary<string, object?> { ["RuleSet"] = true }
                }
            ],
            Configurations =
            [
                new ConfigurationManifest
                {
                    EntityName = "Customer",
                    EntityTypeName = "ComplexDomain.Customer",
                    TableName = "Customers",
                    SchemaName = "dbo",
                    KeyProperties = ["Id"],
                    PropertyConfigurations = new Dictionary<string, PropertyConfigurationManifest>
                    {
                        ["Name"] = new PropertyConfigurationManifest
                        {
                            PropertyName = "Name",
                            ColumnName = "customer_name",
                            ColumnType = "nvarchar(100)",
                            IsRequired = true,
                            MaxLength = 100,
                            IsUnicode = true
                        },
                        ["Balance"] = new PropertyConfigurationManifest
                        {
                            PropertyName = "Balance",
                            Precision = 18,
                            Scale = 2,
                            DefaultValue = "0",
                            DefaultValueSql = "0.00",
                            ValueGenerated = "OnAdd"
                        },
                        ["Total"] = new PropertyConfigurationManifest
                        {
                            PropertyName = "Total",
                            ComputedColumnSql = "Balance * 1.1",
                            IsConcurrencyToken = true
                        }
                    },
                    Indexes =
                    [
                        new IndexManifest
                        {
                            Name = "IX_Customer_Name",
                            Properties = ["Name"],
                            IsUnique = true,
                            Filter = "Name IS NOT NULL",
                            IncludedProperties = ["Balance"]
                        }
                    ],
                    Relationships =
                    [
                        new RelationshipManifest
                        {
                            PrincipalEntity = "Customer",
                            DependentEntity = "Order",
                            RelationshipType = "OneToMany",
                            PrincipalNavigation = "Orders",
                            DependentNavigation = "Customer",
                            ForeignKeyProperties = ["CustomerId"],
                            IsRequired = true,
                            DeleteBehavior = "Cascade"
                        },
                        new RelationshipManifest
                        {
                            PrincipalEntity = "Product",
                            DependentEntity = "Category",
                            RelationshipType = "ManyToMany",
                            JoinEntity = "ProductCategory"
                        }
                    ],
                    Metadata = new Dictionary<string, object?> { ["Configuration"] = true }
                }
            ],
            Sources =
            [
                new SourceInfo
                {
                    Type = "Code",
                    Location = "Customer.cs",
                    Timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Metadata = new Dictionary<string, string> { ["Author"] = "System" }
                }
            ],
            Metadata = new Dictionary<string, object?> { ["Version"] = "2.0" }
        };

        var writer = new SnapshotWriter(new SnapshotOptions { IncludeSchema = true });
        var snapshot = writer.CreateSnapshot(manifest);

        var json = writer.Serialize(snapshot);

        Assert.Contains("\"$schema\":", json);
        Assert.Contains("\"namespace\": \"ComplexDomain\"", json);
        Assert.Contains("\"tableName\"", json);
        Assert.Contains("\"schemaName\"", json);
        Assert.Contains("\"maxLength\"", json);
        Assert.Contains("\"isCollection\": true", json);
        Assert.Contains("\"precision\"", json);
        Assert.Contains("\"scale\"", json);
        Assert.Contains("\"isConcurrencyToken\": true", json);
        Assert.Contains("\"isComputed\": true", json);
        Assert.Contains("\"underlyingType\": \"System.Byte\"", json);
        Assert.Contains("\"severity\": \"Critical\"", json);
        Assert.Contains("\"expression\"", json);
        Assert.Contains("\"includes\"", json);
        Assert.Contains("\"columnName\"", json);
        Assert.Contains("\"columnType\"", json);
        Assert.Contains("\"isUnicode\"", json);
        Assert.Contains("\"defaultValue\"", json);
        Assert.Contains("\"defaultValueSql\"", json);
        Assert.Contains("\"computedColumnSql\"", json);
        Assert.Contains("\"valueGenerated\"", json);
        Assert.Contains("\"isUnique\": true", json);
        Assert.Contains("\"filter\"", json);
        Assert.Contains("\"includedProperties\"", json);
        Assert.Contains("\"principalNavigation\"", json);
        Assert.Contains("\"dependentNavigation\"", json);
        Assert.Contains("\"foreignKeyProperties\"", json);
        Assert.Contains("\"deleteBehavior\"", json);
        Assert.Contains("\"joinEntity\"", json);
        Assert.Contains("\"sources\"", json);
        Assert.Contains("\"hash\": \"test-hash\"", json);
    }

    [Fact]
    public void SnapshotWriter_SerializeManifest_DirectSerialization_ProducesCanonicalJson()
    {
        var manifest = CreateTestManifest();
        var writer = new SnapshotWriter();

        var json = writer.SerializeManifest(manifest);

        Assert.Contains("\"name\": \"TestDomain\"", json);
        Assert.Contains("\"version\": \"1.0.0\"", json);
    }

    [Fact]
    public void SnapshotReader_Deserialize_ThrowsOnNullJson()
    {
        var reader = new SnapshotReader();
        Assert.Throws<ArgumentException>(() => reader.Deserialize(null!));
    }

    [Fact]
    public void SnapshotReader_Deserialize_ThrowsOnEmptyJson()
    {
        var reader = new SnapshotReader();
        Assert.Throws<ArgumentException>(() => reader.Deserialize(string.Empty));
    }

    [Fact]
    public void SnapshotReader_DeserializeManifest_ThrowsOnNullJson()
    {
        var reader = new SnapshotReader();
        Assert.Throws<ArgumentException>(() => reader.DeserializeManifest(null!));
    }

    [Fact]
    public void SnapshotReader_DeserializeManifest_ThrowsOnEmptyJson()
    {
        var reader = new SnapshotReader();
        Assert.Throws<ArgumentException>(() => reader.DeserializeManifest(string.Empty));
    }

    [Fact]
    public void SnapshotReader_Deserialize_ParsesComplexSnapshot()
    {
        // Use the complex manifest from the writer test
        var manifest = new DomainManifest
        {
            Name = "ComplexDomain",
            Version = new Version(2, 0, 0),
            Hash = "test-hash",
            Entities =
            [
                new EntityManifest
                {
                    Name = "Customer",
                    TypeName = "ComplexDomain.Customer",
                    Namespace = "ComplexDomain",
                    TableName = "Customers",
                    SchemaName = "dbo",
                    Properties =
                    [
                        new PropertyManifest
                        {
                            Name = "Id",
                            TypeName = "System.Guid",
                            IsRequired = true
                        },
                        new PropertyManifest
                        {
                            Name = "Tags",
                            TypeName = "System.Collections.Generic.List<string>",
                            IsCollection = true
                        },
                        new PropertyManifest
                        {
                            Name = "Balance",
                            TypeName = "System.Decimal",
                            Precision = 18,
                            Scale = 2
                        },
                        new PropertyManifest
                        {
                            Name = "RowVersion",
                            TypeName = "System.Byte[]",
                            IsConcurrencyToken = true
                        },
                        new PropertyManifest
                        {
                            Name = "CreatedDate",
                            TypeName = "System.DateTime",
                            IsComputed = true
                        }
                    ],
                    KeyProperties = ["Id"],
                    Metadata = new Dictionary<string, object?> { ["Description"] = "Customer entity" }
                }
            ],
            ValueObjects =
            [
                new ValueObjectManifest
                {
                    Name = "Address",
                    TypeName = "ComplexDomain.Address",
                    Namespace = "ComplexDomain",
                    Properties =
                    [
                        new PropertyManifest { Name = "Street", TypeName = "System.String", MaxLength = 200 }
                    ]
                }
            ],
            Enums =
            [
                new EnumManifest
                {
                    Name = "Status",
                    TypeName = "ComplexDomain.Status",
                    Namespace = "ComplexDomain",
                    UnderlyingType = "System.Byte",
                    Values = new Dictionary<string, object> { ["Active"] = 1, ["Inactive"] = 2 }
                }
            ],
            RuleSets =
            [
                new RuleSetManifest
                {
                    Name = "Default",
                    TargetType = "ComplexDomain.Customer",
                    Rules =
                    [
                        new RuleManifest
                        {
                            Id = "NameRequired",
                            Category = "Invariant",
                            TargetType = "ComplexDomain.Customer",
                            Message = "Name is required",
                            Severity = RuleSeverity.Critical,
                            Expression = "!string.IsNullOrEmpty(Name)",
                            Tags = ["Validation", "Critical"]
                        }
                    ],
                    Includes = ["BaseRules"]
                }
            ],
            Configurations =
            [
                new ConfigurationManifest
                {
                    EntityName = "Customer",
                    EntityTypeName = "ComplexDomain.Customer",
                    TableName = "Customers",
                    SchemaName = "dbo",
                    KeyProperties = ["Id"],
                    PropertyConfigurations = new Dictionary<string, PropertyConfigurationManifest>
                    {
                        ["Name"] = new PropertyConfigurationManifest
                        {
                            PropertyName = "Name",
                            ColumnName = "customer_name",
                            ColumnType = "nvarchar(100)",
                            IsRequired = true,
                            MaxLength = 100,
                            Precision = 10,
                            Scale = 2,
                            IsConcurrencyToken = false,
                            IsUnicode = true,
                            ValueGenerated = "OnAdd",
                            DefaultValue = "N/A",
                            DefaultValueSql = "''",
                            ComputedColumnSql = "UPPER([Name])"
                        }
                    },
                    Indexes =
                    [
                        new IndexManifest
                        {
                            Name = "IX_Customer_Name",
                            Properties = ["Name"],
                            IsUnique = true,
                            Filter = "Name IS NOT NULL",
                            IncludedProperties = ["Balance"]
                        }
                    ],
                    Relationships =
                    [
                        new RelationshipManifest
                        {
                            PrincipalEntity = "Customer",
                            DependentEntity = "Order",
                            RelationshipType = "OneToMany",
                            PrincipalNavigation = "Orders",
                            DependentNavigation = "Customer",
                            ForeignKeyProperties = ["CustomerId"],
                            IsRequired = true,
                            DeleteBehavior = "Cascade",
                            JoinEntity = null
                        }
                    ]
                }
            ],
            Sources =
            [
                new SourceInfo
                {
                    Type = "Code",
                    Location = "Customer.cs",
                    Timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Metadata = new Dictionary<string, string> { ["Author"] = "System" }
                }
            ]
        };

        var writer = new SnapshotWriter();
        var snapshot = writer.CreateSnapshot(manifest);
        var json = writer.Serialize(snapshot);

        var reader = new SnapshotReader();
        var deserialized = reader.Deserialize(json);

        Assert.Equal(snapshot.Name, deserialized.Name);
        Assert.Equal(snapshot.Version, deserialized.Version);
        Assert.Equal(snapshot.Hash, deserialized.Hash);
        Assert.Equal(manifest.Entities.Count, deserialized.Manifest.Entities.Count);
        Assert.Equal(manifest.ValueObjects.Count, deserialized.Manifest.ValueObjects.Count);
        Assert.Equal(manifest.Enums.Count, deserialized.Manifest.Enums.Count);
        Assert.Equal(manifest.RuleSets.Count, deserialized.Manifest.RuleSets.Count);
        Assert.Equal(manifest.Configurations.Count, deserialized.Manifest.Configurations.Count);
        Assert.Equal(manifest.Sources.Count, deserialized.Manifest.Sources.Count);

        // Verify entity details
        var entity = deserialized.Manifest.Entities[0];
        Assert.Equal("Customer", entity.Name);
        Assert.Equal("ComplexDomain", entity.Namespace);
        Assert.Equal("Customers", entity.TableName);
        Assert.Equal("dbo", entity.SchemaName);
        Assert.Equal(5, entity.Properties.Count);

        // Verify property details
        var tagsProp = entity.Properties.First(p => p.Name == "Tags");
        Assert.True(tagsProp.IsCollection);

        var balanceProp = entity.Properties.First(p => p.Name == "Balance");
        Assert.Equal(18, balanceProp.Precision);
        Assert.Equal(2, balanceProp.Scale);

        var rowVersionProp = entity.Properties.First(p => p.Name == "RowVersion");
        Assert.True(rowVersionProp.IsConcurrencyToken);

        var createdDateProp = entity.Properties.First(p => p.Name == "CreatedDate");
        Assert.True(createdDateProp.IsComputed);

        // Verify enum
        var enumManifest = deserialized.Manifest.Enums[0];
        Assert.Equal("ComplexDomain", enumManifest.Namespace);
        Assert.Equal("System.Byte", enumManifest.UnderlyingType);
        Assert.Equal(2, enumManifest.Values.Count);

        // Verify rule
        var ruleSetManifest = deserialized.Manifest.RuleSets[0];
        Assert.Single(ruleSetManifest.Includes);
        Assert.Equal("BaseRules", ruleSetManifest.Includes[0]);
        var rule = ruleSetManifest.Rules[0];
        Assert.Equal(RuleSeverity.Critical, rule.Severity);
        Assert.Equal("!string.IsNullOrEmpty(Name)", rule.Expression);
        Assert.Equal(2, rule.Tags.Count);

        // Verify configuration
        var config = deserialized.Manifest.Configurations[0];
        Assert.Equal("Customers", config.TableName);
        Assert.Equal("dbo", config.SchemaName);
        Assert.Single(config.PropertyConfigurations);
        var propConfig = config.PropertyConfigurations["Name"];
        Assert.Equal("customer_name", propConfig.ColumnName);
        Assert.Equal("nvarchar(100)", propConfig.ColumnType);
        Assert.True(propConfig.IsRequired);
        Assert.Equal(100, propConfig.MaxLength);
        Assert.Equal(10, propConfig.Precision);
        Assert.Equal(2, propConfig.Scale);
        Assert.True(propConfig.IsUnicode);
        Assert.Equal("OnAdd", propConfig.ValueGenerated);
        Assert.Equal("N/A", propConfig.DefaultValue);
        Assert.Equal("''", propConfig.DefaultValueSql);
        Assert.Equal("UPPER([Name])", propConfig.ComputedColumnSql);

        // Verify index
        var index = config.Indexes[0];
        Assert.Equal("IX_Customer_Name", index.Name);
        Assert.True(index.IsUnique);
        Assert.Equal("Name IS NOT NULL", index.Filter);
        Assert.Single(index.IncludedProperties);

        // Verify relationship
        var rel = config.Relationships[0];
        Assert.Equal("Orders", rel.PrincipalNavigation);
        Assert.Equal("Customer", rel.DependentNavigation);
        Assert.True(rel.IsRequired);
        Assert.Equal("Cascade", rel.DeleteBehavior);
        Assert.Single(rel.ForeignKeyProperties);

        // Verify source
        var source = deserialized.Manifest.Sources[0];
        Assert.Equal("Code", source.Type);
        Assert.Equal("Customer.cs", source.Location);
        Assert.NotNull(source.Timestamp);
        Assert.Single(source.Metadata);
        Assert.Equal("System", source.Metadata["Author"]);
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
