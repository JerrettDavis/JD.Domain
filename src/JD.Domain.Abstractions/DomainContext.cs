using System;
using System.Collections.Generic;

namespace JD.Domain.Abstractions;

/// <summary>
/// Provides context information for rule evaluation and domain operations.
/// </summary>
public sealed class DomainContext
{
    /// <summary>
    /// Gets the correlation ID for tracking related operations.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the user or actor identifier.
    /// </summary>
    public string? Actor { get; init; }

    /// <summary>
    /// Gets the timestamp of the operation.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the environment name (e.g., "Development", "Production").
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Gets additional context properties.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Properties { get; init; } = 
        new Dictionary<string, object?>();

    /// <summary>
    /// Creates an empty domain context.
    /// </summary>
    /// <returns>A new empty domain context.</returns>
    public static DomainContext Empty() => new();

    /// <summary>
    /// Creates a domain context with the specified correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <returns>A new domain context.</returns>
    public static DomainContext WithCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(correlationId));
        
        return new DomainContext
        {
            CorrelationId = correlationId
        };
    }
}
