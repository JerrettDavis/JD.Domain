using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JD.Domain.Modeling;

/// <summary>
/// Provides the entry point for defining domain models using the fluent DSL.
/// </summary>
public static class Domain
{
    /// <summary>
    /// Creates a new domain builder with the specified name.
    /// </summary>
    /// <param name="name">The name of the domain.</param>
    /// <returns>A new domain builder instance.</returns>
    public static DomainBuilder Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        return new DomainBuilder(name);
    }
}
