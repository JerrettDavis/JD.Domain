using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JD.Domain.Abstractions;

/// <summary>
/// Represents a domain error with detailed information about a rule violation or validation failure.
/// </summary>
public sealed class DomainError
{
    /// <summary>
    /// Gets the error code identifying the type of error.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the target property or path where the error occurred, if applicable.
    /// </summary>
    public string? Target { get; init; }

    /// <summary>
    /// Gets the severity level of the error.
    /// </summary>
    public RuleSeverity Severity { get; init; } = RuleSeverity.Error;

    /// <summary>
    /// Gets the collection of tags associated with this error for categorization.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets additional metadata associated with this error.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();

    /// <summary>
    /// Creates a new domain error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new domain error instance.</returns>
    public static DomainError Create(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(code));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));

        return new DomainError
        {
            Code = code,
            Message = message
        };
    }

    /// <summary>
    /// Creates a new domain error with the specified code, message, and target.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The target property or path.</param>
    /// <returns>A new domain error instance.</returns>
    public static DomainError Create(string code, string message, string target)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(code));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
        if (string.IsNullOrWhiteSpace(target)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(target));

        return new DomainError
        {
            Code = code,
            Message = message,
            Target = target
        };
    }
}
