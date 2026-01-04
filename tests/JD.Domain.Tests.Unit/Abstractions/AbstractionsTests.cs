using JD.Domain.Abstractions;

namespace JD.Domain.Tests.Unit.Abstractions;

public class ConfigurationManifestTests
{
    [Fact]
    public void ConfigurationManifest_CanBeCreated()
    {
        var config = new ConfigurationManifest
        {
            EntityName = "Customer",
            EntityTypeName = "Domain.Customer",
            TableName = "Customers",
            SchemaName = "dbo"
        };

        Assert.Equal("Customer", config.EntityName);
        Assert.Equal("Domain.Customer", config.EntityTypeName);
        Assert.Equal("Customers", config.TableName);
        Assert.Equal("dbo", config.SchemaName);
    }

    [Fact]
    public void ConfigurationManifest_KeyProperties_InitializesAsEmpty()
    {
        var config = new ConfigurationManifest
        {
            EntityName = "Customer",
            EntityTypeName = "Domain.Customer"
        };

        Assert.NotNull(config.KeyProperties);
        Assert.Empty(config.KeyProperties);
    }

    [Fact]
    public void ConfigurationManifest_PropertyConfigurations_InitializesAsEmpty()
    {
        var config = new ConfigurationManifest
        {
            EntityName = "Customer",
            EntityTypeName = "Domain.Customer"
        };

        Assert.NotNull(config.PropertyConfigurations);
        Assert.Empty(config.PropertyConfigurations);
    }

    [Fact]
    public void ConfigurationManifest_Indexes_InitializesAsEmpty()
    {
        var config = new ConfigurationManifest
        {
            EntityName = "Customer",
            EntityTypeName = "Domain.Customer"
        };

        Assert.NotNull(config.Indexes);
        Assert.Empty(config.Indexes);
    }

    [Fact]
    public void ConfigurationManifest_Relationships_InitializesAsEmpty()
    {
        var config = new ConfigurationManifest
        {
            EntityName = "Customer",
            EntityTypeName = "Domain.Customer"
        };

        Assert.NotNull(config.Relationships);
        Assert.Empty(config.Relationships);
    }

    [Fact]
    public void ConfigurationManifest_Metadata_InitializesAsEmpty()
    {
        var config = new ConfigurationManifest
        {
            EntityName = "Customer",
            EntityTypeName = "Domain.Customer"
        };

        Assert.NotNull(config.Metadata);
        Assert.Empty(config.Metadata);
    }

    [Fact]
    public void ConfigurationManifest_WithCompleteData()
    {
        var propertyConfigs = new Dictionary<string, PropertyConfigurationManifest>
        {
            ["Amount"] = new PropertyConfigurationManifest
            {
                PropertyName = "Amount",
                ColumnName = "OrderAmount"
            }
        };

        var config = new ConfigurationManifest
        {
            EntityName = "Order",
            EntityTypeName = "Domain.Order",
            TableName = "Orders",
            SchemaName = "sales",
            KeyProperties = ["Id"],
            PropertyConfigurations = propertyConfigs,
            Indexes =
            [
                new IndexManifest
                {
                    Name = "IX_Order_Date",
                    Properties = ["OrderDate"]
                }
            ],
            Relationships =
            [
                new RelationshipManifest
                {
                    PrincipalEntity = "Customer",
                    DependentEntity = "Order",
                    RelationshipType = "ManyToOne"
                }
            ],
            Metadata = new Dictionary<string, object?>
            {
                ["CreatedBy"] = "System"
            }
        };

        Assert.Equal("Order", config.EntityName);
        Assert.Single(config.KeyProperties);
        Assert.Single(config.PropertyConfigurations);
        Assert.Single(config.Indexes);
        Assert.Single(config.Relationships);
        Assert.Single(config.Metadata);
    }
}

public class DomainContextTests
{
    [Fact]
    public void DomainContext_CanBeCreated()
    {
        var context = new DomainContext
        {
            CorrelationId = "test-id",
            Actor = "test-user",
            Timestamp = DateTimeOffset.UtcNow,
            Environment = "Test"
        };

        Assert.Equal("test-id", context.CorrelationId);
        Assert.Equal("test-user", context.Actor);
        Assert.Equal("Test", context.Environment);
    }

    [Fact]
    public void DomainContext_Properties_InitializesAsEmpty()
    {
        var context = new DomainContext
        {
            CorrelationId = "test-id"
        };

        Assert.NotNull(context.Properties);
        Assert.Empty(context.Properties);
    }

    [Fact]
    public void DomainContext_WithProperties()
    {
        var context = new DomainContext
        {
            CorrelationId = "test-id",
            Properties = new Dictionary<string, object?>
            {
                ["RequestId"] = "req-123",
                ["IpAddress"] = "127.0.0.1"
            }
        };

        Assert.Equal(2, context.Properties.Count);
        Assert.Equal("req-123", context.Properties["RequestId"]);
        Assert.Equal("127.0.0.1", context.Properties["IpAddress"]);
    }
}

public class PropertyConfigurationManifestTests
{
    [Fact]
    public void PropertyConfigurationManifest_CanBeCreated()
    {
        var config = new PropertyConfigurationManifest
        {
            PropertyName = "FirstName",
            ColumnName = "first_name",
            ColumnType = "varchar(100)",
            IsRequired = true,
            MaxLength = 100
        };

        Assert.Equal("FirstName", config.PropertyName);
        Assert.Equal("first_name", config.ColumnName);
        Assert.Equal("varchar(100)", config.ColumnType);
        Assert.True(config.IsRequired);
        Assert.Equal(100, config.MaxLength);
    }

    [Fact]
    public void PropertyConfigurationManifest_Metadata_InitializesAsEmpty()
    {
        var config = new PropertyConfigurationManifest
        {
            PropertyName = "Email"
        };

        Assert.NotNull(config.Metadata);
        Assert.Empty(config.Metadata);
    }

    [Fact]
    public void PropertyConfigurationManifest_WithMetadata()
    {
        var config = new PropertyConfigurationManifest
        {
            PropertyName = "Email",
            Metadata = new Dictionary<string, object?>
            {
                ["Index"] = "IX_Email",
                ["Sensitive"] = true
            }
        };

        Assert.Equal(2, config.Metadata.Count);
        Assert.True((bool)config.Metadata["Sensitive"]!);
    }
}

public class IndexManifestTests
{
    [Fact]
    public void IndexManifest_CanBeCreated()
    {
        var index = new IndexManifest
        {
            Name = "IX_Customer_Email",
            Properties = ["Email"],
            IsUnique = true
        };

        Assert.Equal("IX_Customer_Email", index.Name);
        Assert.Single(index.Properties);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void IndexManifest_Properties_InitializesAsEmpty()
    {
        var index = new IndexManifest
        {
            Name = "IX_Test"
        };

        Assert.NotNull(index.Properties);
        Assert.Empty(index.Properties);
    }

    [Fact]
    public void IndexManifest_Metadata_InitializesAsEmpty()
    {
        var index = new IndexManifest
        {
            Name = "IX_Test"
        };

        Assert.NotNull(index.Metadata);
        Assert.Empty(index.Metadata);
    }

    [Fact]
    public void IndexManifest_CompositeIndex()
    {
        var index = new IndexManifest
        {
            Name = "IX_Customer_LastName_FirstName",
            Properties = ["LastName", "FirstName"],
            IsUnique = false
        };

        Assert.Equal(2, index.Properties.Count);
        Assert.Equal("LastName", index.Properties[0]);
        Assert.Equal("FirstName", index.Properties[1]);
        Assert.False(index.IsUnique);
    }
}

public class RelationshipManifestTests
{
    [Fact]
    public void RelationshipManifest_CanBeCreated()
    {
        var relationship = new RelationshipManifest
        {
            PrincipalEntity = "Customer",
            DependentEntity = "Order",
            RelationshipType = "ManyToOne",
            ForeignKeyProperties = ["CustomerId"]
        };

        Assert.Equal("Customer", relationship.PrincipalEntity);
        Assert.Equal("Order", relationship.DependentEntity);
        Assert.Equal("ManyToOne", relationship.RelationshipType);
        Assert.Single(relationship.ForeignKeyProperties);
        Assert.Equal("CustomerId", relationship.ForeignKeyProperties[0]);
    }

    [Fact]
    public void RelationshipManifest_Metadata_InitializesAsEmpty()
    {
        var relationship = new RelationshipManifest
        {
            PrincipalEntity = "Customer",
            DependentEntity = "Order",
            RelationshipType = "OneToMany"
        };

        Assert.NotNull(relationship.Metadata);
        Assert.Empty(relationship.Metadata);
    }

    [Fact]
    public void RelationshipManifest_OneToMany()
    {
        var relationship = new RelationshipManifest
        {
            PrincipalEntity = "Customer",
            DependentEntity = "Order",
            RelationshipType = "OneToMany",
            PrincipalNavigation = "Orders",
            DependentNavigation = "Customer"
        };

        Assert.Equal("OneToMany", relationship.RelationshipType);
        Assert.Equal("Orders", relationship.PrincipalNavigation);
        Assert.Equal("Customer", relationship.DependentNavigation);
    }

    [Fact]
    public void RelationshipManifest_ManyToMany()
    {
        var relationship = new RelationshipManifest
        {
            PrincipalEntity = "Product",
            DependentEntity = "Category",
            RelationshipType = "ManyToMany",
            JoinEntity = "ProductCategory"
        };

        Assert.Equal("ManyToMany", relationship.RelationshipType);
        Assert.Equal("ProductCategory", relationship.JoinEntity);
    }
}

public class DomainErrorTests
{
    [Fact]
    public void DomainError_Create_SetsProperties()
    {
        var error = DomainError.Create("ERR001", "Test error message");

        Assert.Equal("ERR001", error.Code);
        Assert.Equal("Test error message", error.Message);
        Assert.Equal(RuleSeverity.Error, error.Severity);
    }

    [Fact]
    public void DomainError_CreateWithTarget()
    {
        var error = DomainError.Create("WARN001", "Warning message", "Email");

        Assert.Equal("WARN001", error.Code);
        Assert.Equal("Warning message", error.Message);
        Assert.Equal("Email", error.Target);
    }

    [Fact]
    public void DomainError_PropertyAssignment()
    {
        var error = new DomainError
        {
            Code = "ERR002",
            Message = "Another error",
            Target = "Email",
            Severity = RuleSeverity.Critical
        };

        Assert.Equal("ERR002", error.Code);
        Assert.Equal("Another error", error.Message);
        Assert.Equal("Email", error.Target);
        Assert.Equal(RuleSeverity.Critical, error.Severity);
    }
}
