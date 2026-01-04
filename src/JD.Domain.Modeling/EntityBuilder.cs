using System.Linq.Expressions;
using System.Reflection;
using JD.Domain.Abstractions;

namespace JD.Domain.Modeling;

/// <summary>
/// Fluent builder for constructing entity manifests.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class EntityBuilder<T> where T : class
{
    private readonly Type _entityType = typeof(T);
    private readonly List<PropertyManifest> _properties = [];
    private readonly List<string> _keyProperties = [];
    private string? _tableName;
    private string? _schemaName;
    private readonly Dictionary<string, object?> _metadata = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityBuilder{T}"/> class.
    /// </summary>
    public EntityBuilder()
    {
        // Auto-discover properties from reflection
        DiscoverProperties();
    }

    /// <summary>
    /// Specifies the primary key property.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="propertyExpression">The property selector expression.</param>
    /// <returns>A property builder for further configuration.</returns>
    public PropertyBuilder<T, TProperty> Key<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);

        if (!_keyProperties.Contains(propertyName))
        {
            _keyProperties.Add(propertyName);
        }

        return Property(propertyExpression);
    }

    /// <summary>
    /// Specifies a property for configuration.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="propertyExpression">The property selector expression.</param>
    /// <returns>A property builder for further configuration.</returns>
    public PropertyBuilder<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var propertyName = GetPropertyName(propertyExpression);
        var propertyInfo = GetPropertyInfo(propertyExpression);

        // Find or create property manifest
        var existingProperty = _properties.FirstOrDefault(p => p.Name == propertyName);
        if (existingProperty == null)
        {
            var newProperty = CreatePropertyManifest(propertyInfo);
            _properties.Add(newProperty);
            existingProperty = newProperty;
        }

        return new PropertyBuilder<T, TProperty>(this, existingProperty, propertyName);
    }

    /// <summary>
    /// Specifies the table name for the entity.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="schemaName">The optional schema name.</param>
    /// <returns>The entity builder for chaining.</returns>
    public EntityBuilder<T> ToTable(string tableName, string? schemaName = null)
    {
        if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(tableName));
        _tableName = tableName;
        _schemaName = schemaName;
        return this;
    }

    /// <summary>
    /// Adds metadata to the entity.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The entity builder for chaining.</returns>
    public EntityBuilder<T> WithMetadata(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Builds the entity manifest.
    /// </summary>
    /// <returns>The constructed entity manifest.</returns>
    internal EntityManifest Build()
    {
        return new EntityManifest
        {
            Name = _entityType.Name,
            TypeName = _entityType.FullName ?? _entityType.Name,
            Namespace = _entityType.Namespace,
            Properties = _properties.ToList().AsReadOnly(),
            KeyProperties = _keyProperties.ToList().AsReadOnly(),
            TableName = _tableName,
            SchemaName = _schemaName,
            Metadata = _metadata.ToDictionary(x => x.Key, x => x.Value) as IReadOnlyDictionary<string, object?>
        };
    }

    /// <summary>
    /// Updates a property manifest.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="updater">The update function that returns a new property manifest.</param>
    internal void UpdateProperty(string propertyName, Func<PropertyManifest, PropertyManifest> updater)
    {
        var property = _properties.FirstOrDefault(p => p.Name == propertyName);
        if (property != null)
        {
            _properties.Remove(property);
            var updated = updater(property);
            _properties.Add(updated);
        }
    }

    private void DiscoverProperties()
    {
        var properties = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var propertyInfo in properties)
        {
            if (!_properties.Any(p => p.Name == propertyInfo.Name))
            {
                _properties.Add(CreatePropertyManifest(propertyInfo));
            }
        }
    }

    private PropertyManifest CreatePropertyManifest(PropertyInfo propertyInfo)
    {
        var propertyType = propertyInfo.PropertyType;
        var isNullable = IsNullableType(propertyType);
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        return new PropertyManifest
        {
            Name = propertyInfo.Name,
            TypeName = underlyingType.FullName ?? underlyingType.Name,
            IsRequired = !isNullable && !propertyType.IsValueType,
            IsCollection = IsCollectionType(propertyType)
        };
    }

    private static bool IsNullableType(Type type)
    {
        if (!type.IsValueType)
            return true; // Reference types are nullable by default in this context

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

    private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Invalid property expression", nameof(propertyExpression));
    }

    private static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpression &&
            memberExpression.Member is PropertyInfo propertyInfo)
        {
            return propertyInfo;
        }

        throw new ArgumentException("Invalid property expression", nameof(propertyExpression));
    }
}
