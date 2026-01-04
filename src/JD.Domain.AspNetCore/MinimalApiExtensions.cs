using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace JD.Domain.AspNetCore;

/// <summary>
/// Extension methods for adding domain validation to Minimal API endpoints.
/// </summary>
public static class MinimalApiExtensions
{
    /// <summary>
    /// Adds domain validation for the request body type.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder WithDomainValidation<T>(
        this RouteHandlerBuilder builder) where T : class
    {
        return builder
            .AddEndpointFilter<DomainValidationEndpointFilter<T>>()
            .WithMetadata(new DomainValidationMetadata(typeof(T)));
    }

    /// <summary>
    /// Adds domain validation with a specific rule set.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="ruleSet">The rule set to evaluate.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder WithDomainValidation<T>(
        this RouteHandlerBuilder builder,
        string ruleSet) where T : class
    {
        return builder
            .AddEndpointFilter<DomainValidationEndpointFilter<T>>()
            .WithMetadata(new DomainValidationMetadata(typeof(T), ruleSet));
    }

    /// <summary>
    /// Adds domain validation with full configuration options.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="configure">Configuration action for validation metadata.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder WithDomainValidation<T>(
        this RouteHandlerBuilder builder,
        Action<DomainValidationMetadataBuilder> configure) where T : class
    {
        var metadataBuilder = new DomainValidationMetadataBuilder(typeof(T));
        configure(metadataBuilder);

        return builder
            .AddEndpointFilter<DomainValidationEndpointFilter<T>>()
            .WithMetadata(metadataBuilder.Build());
    }
}
