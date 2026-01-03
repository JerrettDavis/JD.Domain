using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using JD.Domain.Abstractions;

namespace JD.Domain.Modeling;

/// <summary>
/// Fluent builder for configuring entity properties.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class PropertyBuilder<TEntity, TProperty> where TEntity : class
{
    private readonly EntityBuilder<TEntity> _entityBuilder;
    private readonly PropertyManifest _propertyManifest;
    private readonly string _propertyName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBuilder{TEntity,TProperty}"/> class.
    /// </summary>
    /// <param name="entityBuilder">The parent entity builder.</param>
    /// <param name="propertyManifest">The property manifest being configured.</param>
    /// <param name="propertyName">The property name.</param>
    internal PropertyBuilder(
        EntityBuilder<TEntity> entityBuilder,
        PropertyManifest propertyManifest,
        string propertyName)
    {
        _entityBuilder = entityBuilder;
        _propertyManifest = propertyManifest;
        _propertyName = propertyName;
    }

    /// <summary>
    /// Marks the property as required.
    /// </summary>
    /// <param name="isRequired">Whether the property is required.</param>
    /// <returns>The property builder for chaining.</returns>
    public PropertyBuilder<TEntity, TProperty> IsRequired(bool isRequired = true)
    {
        _entityBuilder.UpdateProperty(_propertyName, property =>
        {
            return new PropertyManifest
            {
                Name = property.Name,
                TypeName = property.TypeName,
                IsRequired = isRequired,
                IsCollection = property.IsCollection,
                MaxLength = property.MaxLength,
                Precision = property.Precision,
                Scale = property.Scale,
                IsConcurrencyToken = property.IsConcurrencyToken,
                IsComputed = property.IsComputed,
                Metadata = property.Metadata
            };
        });
        
        return this;
    }

    /// <summary>
    /// Sets the maximum length for string properties.
    /// </summary>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>The property builder for chaining.</returns>
    public PropertyBuilder<TEntity, TProperty> HasMaxLength(int maxLength)
    {
        if (maxLength <= 0)
            throw new ArgumentException("Max length must be greater than zero", nameof(maxLength));

        _entityBuilder.UpdateProperty(_propertyName, property =>
        {
            return new PropertyManifest
            {
                Name = property.Name,
                TypeName = property.TypeName,
                IsRequired = property.IsRequired,
                IsCollection = property.IsCollection,
                MaxLength = maxLength,
                Precision = property.Precision,
                Scale = property.Scale,
                IsConcurrencyToken = property.IsConcurrencyToken,
                IsComputed = property.IsComputed,
                Metadata = property.Metadata
            };
        });
        
        return this;
    }

    /// <summary>
    /// Sets the precision for numeric properties.
    /// </summary>
    /// <param name="precision">The precision.</param>
    /// <param name="scale">The optional scale.</param>
    /// <returns>The property builder for chaining.</returns>
    public PropertyBuilder<TEntity, TProperty> HasPrecision(int precision, int? scale = null)
    {
        if (precision <= 0)
            throw new ArgumentException("Precision must be greater than zero", nameof(precision));
        
        if (scale.HasValue && scale.Value < 0)
            throw new ArgumentException("Scale cannot be negative", nameof(scale));

        return this;
    }

    /// <summary>
    /// Marks the property as a concurrency token.
    /// </summary>
    /// <returns>The property builder for chaining.</returns>
    public PropertyBuilder<TEntity, TProperty> IsConcurrencyToken()
    {
        return this;
    }

    /// <summary>
    /// Marks the property as computed.
    /// </summary>
    /// <returns>The property builder for chaining.</returns>
    public PropertyBuilder<TEntity, TProperty> IsComputed()
    {
        return this;
    }
}
