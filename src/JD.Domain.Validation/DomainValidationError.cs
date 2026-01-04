using JD.Domain.Abstractions;

namespace JD.Domain.Validation;

/// <summary>
/// API-friendly representation of a domain validation error.
/// </summary>
public sealed record DomainValidationError
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
    /// Gets the target property or path where the error occurred.
    /// </summary>
    public string? Target { get; init; }

    /// <summary>
    /// Gets the severity level as a string.
    /// </summary>
    public string Severity { get; init; } = "Error";

    /// <summary>
    /// Gets additional metadata as key-value pairs.
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }

    /// <summary>
    /// Creates a <see cref="DomainValidationError"/> from a <see cref="DomainError"/>.
    /// </summary>
    /// <param name="error">The domain error to convert.</param>
    /// <returns>A new <see cref="DomainValidationError"/> instance.</returns>
    public static DomainValidationError FromDomainError(DomainError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new DomainValidationError
        {
            Code = error.Code,
            Message = error.Message,
            Target = error.Target,
            Severity = error.Severity.ToString(),
            Metadata = error.Metadata.Count > 0
                ? new Dictionary<string, object?>(error.Metadata)
                : null
        };
    }
}
