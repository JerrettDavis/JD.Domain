using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JD.Domain.Abstractions;

namespace JD.Domain.Modeling;

/// <summary>
/// Fluent builder for constructing value object manifests.
/// </summary>
/// <typeparam name="T">The value object type.</typeparam>
public sealed class ValueObjectBuilder<T> where T : class
{
    private readonly Type _valueObjectType = typeof(T);
    private readonly List<PropertyManifest> _properties = new();
    private readonly Dictionary<string, object?> _metadata = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueObjectBuilder{T}"/> class.
    /// </summary>
    public ValueObjectBuilder()
    {
        // Auto-discover properties from reflection
        DiscoverProperties();
    }

    /// <summary>
    /// Adds metadata to the value object.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The value object builder for chaining.</returns>
    public ValueObjectBuilder<T> WithMetadata(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the value object manifest.
    /// </summary>
    /// <returns>The constructed value object manifest.</returns>
    internal ValueObjectManifest Build()
    {
        return new ValueObjectManifest
        {
            Name = _valueObjectType.Name,
            TypeName = _valueObjectType.FullName ?? _valueObjectType.Name,
            Namespace = _valueObjectType.Namespace,
            Properties = _properties.ToList().AsReadOnly(),
            Metadata = _metadata.ToDictionary(x => x.Key, x => x.Value) as IReadOnlyDictionary<string, object?>
        };
    }

    private void DiscoverProperties()
    {
        var properties = _valueObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var propertyInfo in properties)
        {
            var propertyType = propertyInfo.PropertyType;
            var isNullable = IsNullableType(propertyType);
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            
            _properties.Add(new PropertyManifest
            {
                Name = propertyInfo.Name,
                TypeName = underlyingType.FullName ?? underlyingType.Name,
                IsRequired = !isNullable && !propertyType.IsValueType,
                IsCollection = IsCollectionType(propertyType)
            });
        }
    }

    private static bool IsNullableType(Type type)
    {
        if (!type.IsValueType)
            return true;
        
        return Nullable.GetUnderlyingType(type) != null;
    }

    private static bool IsCollectionType(Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsArray || 
               (type.IsGenericType && 
                (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                 type.GetGenericTypeDefinition() == typeof(IList<>) ||
                 type.GetGenericTypeDefinition() == typeof(List<>)));
    }
}
