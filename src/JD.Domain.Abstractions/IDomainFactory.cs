using System.Threading;
using System.Threading.Tasks;

namespace JD.Domain.Abstractions;

/// <summary>
/// Defines the contract for creating domain instances with validation.
/// </summary>
public interface IDomainFactory
{
    /// <summary>
    /// Creates a domain instance from the specified input with validation.
    /// </summary>
    /// <typeparam name="TDomain">The type of the domain instance to create.</typeparam>
    /// <param name="input">The input data for creating the instance.</param>
    /// <param name="options">The creation options.</param>
    /// <returns>A result containing the created instance or validation errors.</returns>
    Result<TDomain> Create<TDomain>(
        object input,
        DomainCreateOptions? options = null) where TDomain : class;

    /// <summary>
    /// Creates a domain instance from the specified input with validation asynchronously.
    /// </summary>
    /// <typeparam name="TDomain">The type of the domain instance to create.</typeparam>
    /// <param name="input">The input data for creating the instance.</param>
    /// <param name="options">The creation options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the created instance or validation errors.</returns>
    ValueTask<Result<TDomain>> CreateAsync<TDomain>(
        object input,
        DomainCreateOptions? options = null,
        CancellationToken cancellationToken = default) where TDomain : class;
}
