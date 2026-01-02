using JD.Domain.Abstractions;

namespace JD.Domain.Configuration;

/// <summary>
/// Fluent builder for configuring indexes.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class IndexBuilder<T> where T : class
{
    private readonly EntityConfigurationBuilder<T> _configBuilder;
    private IndexManifest _index;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexBuilder{T}"/> class.
    /// </summary>
    /// <param name="configBuilder">The parent configuration builder.</param>
    /// <param name="index">The index being configured.</param>
    internal IndexBuilder(EntityConfigurationBuilder<T> configBuilder, IndexManifest index)
    {
        _configBuilder = configBuilder;
        _index = index;
    }

    /// <summary>
    /// Marks the index as unique.
    /// </summary>
    /// <param name="isUnique">Whether the index is unique.</param>
    /// <returns>The index builder for chaining.</returns>
    public IndexBuilder<T> IsUnique(bool isUnique = true)
    {
        _index = new IndexManifest
        {
            Properties = _index.Properties,
            IsUnique = isUnique,
            Filter = _index.Filter,
            Metadata = _index.Metadata
        };

        _configBuilder.ReplaceIndex(_index);
        return this;
    }

    /// <summary>
    /// Sets a filter for the index.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <returns>The index builder for chaining.</returns>
    public IndexBuilder<T> HasFilter(string filter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);

        _index = new IndexManifest
        {
            Properties = _index.Properties,
            IsUnique = _index.IsUnique,
            Filter = filter,
            Metadata = _index.Metadata
        };

        _configBuilder.ReplaceIndex(_index);
        return this;
    }
}
