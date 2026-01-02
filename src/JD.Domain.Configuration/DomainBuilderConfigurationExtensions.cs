using System;
using System.Collections.Generic;
using System.Linq;

using JD.Domain.Abstractions;
using JD.Domain.Modeling;

namespace JD.Domain.Configuration;

/// <summary>
/// Extension methods for adding EF Core-compatible configuration to the domain builder.
/// </summary>
public static class DomainBuilderConfigurationExtensions
{
    /// <summary>
    /// Adds configuration for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="builder">The domain builder.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The domain builder for chaining.</returns>
    public static DomainBuilder Configure<T>(
        this DomainBuilder builder,
        Action<EntityConfigurationBuilder<T>> configure) where T : class
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var configBuilder = new EntityConfigurationBuilder<T>();
        configure(configBuilder);

        var configuration = configBuilder.Build();
        builder.AddConfiguration(configuration);

        return builder;
    }
}
