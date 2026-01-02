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
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new DomainBuilder(name);
    }
}
