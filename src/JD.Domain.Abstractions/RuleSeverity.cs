namespace JD.Domain.Abstractions;

/// <summary>
/// Defines the severity level of a rule violation or validation error.
/// </summary>
public enum RuleSeverity
{
    /// <summary>
    /// Informational message that doesn't indicate a problem.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning that should be reviewed but doesn't prevent operation.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error that indicates a rule violation but may be recoverable.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical error that prevents operation and must be addressed.
    /// </summary>
    Critical = 3
}
