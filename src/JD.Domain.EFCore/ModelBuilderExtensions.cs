using JD.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace JD.Domain.EFCore;

/// <summary>
/// Extension methods for applying domain manifests to EF Core ModelBuilder.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the domain manifest to the EF Core model builder.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder.</param>
    /// <param name="manifest">The domain manifest to apply.</param>
    /// <returns>The model builder for chaining.</returns>
    public static ModelBuilder ApplyDomainManifest(
        this ModelBuilder modelBuilder,
        DomainManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(manifest);

        // Apply entity configurations from the manifest
        foreach (var entity in manifest.Entities)
        {
            ApplyEntityConfiguration(modelBuilder, entity);
        }

        // Apply configurations from configuration manifests
        foreach (var config in manifest.Configurations)
        {
            ApplyConfigurationManifest(modelBuilder, config);
        }

        return modelBuilder;
    }

    private static void ApplyEntityConfiguration(ModelBuilder modelBuilder, EntityManifest entity)
    {
        // Find the entity type in the model
        var entityType = modelBuilder.Model.FindEntityType(entity.TypeName);
        if (entityType == null)
        {
            // Entity not registered in model yet, skip
            return;
        }

        // Apply table mapping if specified
        if (!string.IsNullOrEmpty(entity.TableName))
        {
            if (!string.IsNullOrEmpty(entity.SchemaName))
            {
                modelBuilder.Entity(entity.TypeName).ToTable(entity.TableName, entity.SchemaName);
            }
            else
            {
                modelBuilder.Entity(entity.TypeName).ToTable(entity.TableName);
            }
        }

        // Apply key configuration if specified
        if (entity.KeyProperties.Count > 0)
        {
            modelBuilder.Entity(entity.TypeName).HasKey(entity.KeyProperties.ToArray());
        }

        // Apply property configurations
        foreach (var property in entity.Properties)
        {
            ApplyPropertyConfiguration(modelBuilder, entity.TypeName, property);
        }
    }

    private static void ApplyPropertyConfiguration(
        ModelBuilder modelBuilder,
        string entityTypeName,
        PropertyManifest property)
    {
        var entityBuilder = modelBuilder.Entity(entityTypeName);
        var propertyBuilder = entityBuilder.Property(property.Name);

        // Apply required configuration
        if (property.IsRequired)
        {
            propertyBuilder.IsRequired();
        }

        // Apply max length if specified
        if (property.MaxLength.HasValue)
        {
            propertyBuilder.HasMaxLength(property.MaxLength.Value);
        }
    }

    private static void ApplyConfigurationManifest(
        ModelBuilder modelBuilder,
        ConfigurationManifest config)
    {
        // Find the entity type in the model
        var entityType = modelBuilder.Model.FindEntityType(config.EntityTypeName);
        if (entityType == null)
        {
            // Entity not registered in model yet, skip
            return;
        }

        // Apply table mapping
        if (!string.IsNullOrEmpty(config.TableName))
        {
            if (!string.IsNullOrEmpty(config.SchemaName))
            {
                modelBuilder.Entity(config.EntityTypeName)
                    .ToTable(config.TableName, config.SchemaName);
            }
            else
            {
                modelBuilder.Entity(config.EntityTypeName)
                    .ToTable(config.TableName);
            }
        }

        // Apply indexes
        foreach (var index in config.Indexes)
        {
            var indexBuilder = modelBuilder.Entity(config.EntityTypeName)
                .HasIndex(index.Properties.ToArray());

            if (index.IsUnique)
            {
                indexBuilder.IsUnique();
            }

            if (!string.IsNullOrEmpty(index.Filter))
            {
                indexBuilder.HasFilter(index.Filter);
            }
        }

        // Apply key configuration
        if (config.KeyProperties.Count > 0)
        {
            modelBuilder.Entity(config.EntityTypeName)
                .HasKey(config.KeyProperties.ToArray());
        }

        // Apply property configurations
        foreach (var propertyConfig in config.PropertyConfigurations)
        {
            var propertyBuilder = modelBuilder.Entity(config.EntityTypeName)
                .Property(propertyConfig.Key);

            var propManifest = propertyConfig.Value;
            
            if (propManifest.IsRequired)
            {
                propertyBuilder.IsRequired();
            }

            if (propManifest.MaxLength.HasValue)
            {
                propertyBuilder.HasMaxLength(propManifest.MaxLength.Value);
            }
        }
    }
}
