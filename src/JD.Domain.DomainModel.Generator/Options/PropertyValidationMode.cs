namespace JD.Domain.DomainModel.Generator.Options;

/// <summary>
/// Specifies when property validation occurs in generated domain types.
/// </summary>
public enum PropertyValidationMode
{
    /// <summary>
    /// Validate properties when they are set via property setters.
    /// This provides immediate feedback but may throw exceptions.
    /// </summary>
    OnSet,

    /// <summary>
    /// Validate properties only when explicitly requested via Validate() method.
    /// This allows batch validation but requires explicit calls.
    /// </summary>
    OnDemand,

    /// <summary>
    /// Validate properties lazily - track changes and validate when accessing
    /// related methods or when the domain object is used.
    /// </summary>
    Lazy
}
