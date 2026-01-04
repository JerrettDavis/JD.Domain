namespace JD.Domain.Abstractions;

/// <summary>
/// Exception thrown when domain validation fails during property set operations.
/// </summary>
public sealed class DomainValidationException : Exception
{
    /// <summary>
    /// Gets the domain errors that caused the validation failure.
    /// </summary>
    public IReadOnlyList<DomainError> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationException"/> class.
    /// </summary>
    /// <param name="errors">The domain errors that caused the failure.</param>
    public DomainValidationException(IReadOnlyList<DomainError> errors)
        : base(FormatMessage(errors))
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationException"/> class.
    /// </summary>
    /// <param name="error">The domain error that caused the failure.</param>
    public DomainValidationException(DomainError error)
        : this(new[] { error ?? throw new ArgumentNullException(nameof(error)) })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DomainValidationException(string message)
        : base(message)
    {
        Errors = new[] { DomainError.Create("ValidationError", message) };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new[] { DomainError.Create("ValidationError", message) };
    }

    private static string FormatMessage(IReadOnlyList<DomainError> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            return "Domain validation failed.";
        }

        if (errors.Count == 1)
        {
            return errors[0].Message;
        }

        return $"Domain validation failed with {errors.Count} errors: {errors[0].Message}";
    }
}
