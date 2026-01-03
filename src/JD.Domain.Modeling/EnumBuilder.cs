using System;
using System.Collections.Generic;
using System.Linq;
using JD.Domain.Abstractions;

namespace JD.Domain.Modeling;

/// <summary>
/// Fluent builder for constructing enumeration manifests.
/// </summary>
/// <typeparam name="T">The enumeration type.</typeparam>
public sealed class EnumBuilder<T> where T : Enum
{
    private readonly Type _enumType = typeof(T);

    /// <summary>
    /// Builds the enumeration manifest.
    /// </summary>
    /// <returns>The constructed enumeration manifest.</returns>
    internal EnumManifest Build()
    {
        var underlyingType = Enum.GetUnderlyingType(_enumType);
        var values = new Dictionary<string, object>();
        
        foreach (var value in Enum.GetValues(_enumType))
        {
            var name = Enum.GetName(_enumType, value);
            if (name != null)
            {
                values[name] = Convert.ChangeType(value, underlyingType);
            }
        }

        return new EnumManifest
        {
            Name = _enumType.Name,
            TypeName = _enumType.FullName ?? _enumType.Name,
            Namespace = _enumType.Namespace,
            UnderlyingType = underlyingType.FullName ?? underlyingType.Name,
            Values = values.ToDictionary(x => x.Key, x => x.Value) as IReadOnlyDictionary<string, object>
        };
    }
}
