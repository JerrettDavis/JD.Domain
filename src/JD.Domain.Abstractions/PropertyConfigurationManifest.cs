using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JD.Domain.Abstractions;

/// <summary>
/// Represents configuration for a specific property.
/// </summary>
public sealed class PropertyConfigurationManifest
{
    /// <summary>
    /// Gets the property name.
    /// </summary>
    public required string PropertyName { get; init; }

    /// <summary>
    /// Gets the column name, if different from property name.
    /// </summary>
    public string? ColumnName { get; init; }

    /// <summary>
    /// Gets the column type.
    /// </summary>
    public string? ColumnType { get; init; }

    /// <summary>
    /// Gets a value indicating whether the property is required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets the maximum length constraint.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Gets the precision for numeric types.
    /// </summary>
    public int? Precision { get; init; }

    /// <summary>
    /// Gets the scale for numeric types.
    /// </summary>
    public int? Scale { get; init; }

    /// <summary>
    /// Gets a value indicating whether the property is a concurrency token.
    /// </summary>
    public bool IsConcurrencyToken { get; init; }

    /// <summary>
    /// Gets a value indicating whether the property is unicode.
    /// </summary>
    public bool? IsUnicode { get; init; }

    /// <summary>
    /// Gets the value generation strategy.
    /// </summary>
    public string? ValueGenerated { get; init; }

    /// <summary>
    /// Gets the default value expression.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Gets the default SQL expression.
    /// </summary>
    public string? DefaultValueSql { get; init; }

    /// <summary>
    /// Gets the computed SQL expression.
    /// </summary>
    public string? ComputedColumnSql { get; init; }

    /// <summary>
    /// Gets additional property configuration metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
