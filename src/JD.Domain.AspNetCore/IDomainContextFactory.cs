using JD.Domain.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JD.Domain.AspNetCore;

/// <summary>
/// Interface for creating <see cref="DomainContext"/> from <see cref="HttpContext"/>.
/// </summary>
public interface IDomainContextFactory
{
    /// <summary>
    /// Creates a <see cref="DomainContext"/> from the current HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>A new <see cref="DomainContext"/> instance.</returns>
    DomainContext CreateContext(HttpContext httpContext);
}
