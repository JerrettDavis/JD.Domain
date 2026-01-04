using JD.Domain.Abstractions;
using JD.Domain.Runtime;
using JD.Domain.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace JD.Domain.AspNetCore;

/// <summary>
/// Extension methods for registering domain validation services.
/// </summary>
public static class DomainValidationServiceExtensions
{
    /// <summary>
    /// Adds domain validation services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainValidation(
        this IServiceCollection services,
        Action<DomainValidationOptions>? configure = null)
    {
        var options = new DomainValidationOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ValidationProblemDetailsFactory>();
        services.AddScoped<IDomainContextFactory, HttpDomainContextFactory>();

        // Register exception handler for IExceptionHandler pipeline (.NET 8+)
        services.AddExceptionHandler<DomainExceptionHandler>();

        // Configure ProblemDetails
        services.AddProblemDetails(problemOptions =>
        {
            problemOptions.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["correlationId"] =
                    context.HttpContext.TraceIdentifier;
            };
        });

        return services;
    }

    /// <summary>
    /// Adds domain validation services with a specific <see cref="DomainManifest"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="manifest">The domain manifest containing rules and entities.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainValidation(
        this IServiceCollection services,
        DomainManifest manifest,
        Action<DomainValidationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        services.AddDomainValidation(configure);
        services.AddSingleton<IDomainEngine>(_ => DomainRuntime.CreateEngine(manifest));

        return services;
    }

    /// <summary>
    /// Adds domain validation services with a manifest factory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="manifestFactory">Factory function to create the domain manifest.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainValidation(
        this IServiceCollection services,
        Func<IServiceProvider, DomainManifest> manifestFactory,
        Action<DomainValidationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(manifestFactory);

        services.AddDomainValidation(configure);
        services.AddSingleton<IDomainEngine>(sp =>
            DomainRuntime.CreateEngine(manifestFactory(sp)));

        return services;
    }

    /// <summary>
    /// Adds domain validation services with an existing <see cref="IDomainEngine"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="engine">The domain engine to use.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDomainValidation(
        this IServiceCollection services,
        IDomainEngine engine,
        Action<DomainValidationOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(engine);

        services.AddDomainValidation(configure);
        services.AddSingleton(engine);

        return services;
    }
}
