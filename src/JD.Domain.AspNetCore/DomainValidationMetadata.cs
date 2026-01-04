namespace JD.Domain.AspNetCore;

/// <summary>
/// Metadata marker for endpoints that require domain validation.
/// </summary>
public sealed class DomainValidationMetadata
{
    /// <summary>
    /// Gets the type to validate.
    /// </summary>
    public Type ValidationType { get; }

    /// <summary>
    /// Gets the rule set to evaluate. Null means use default.
    /// </summary>
    public string? RuleSet { get; }

    /// <summary>
    /// Gets whether to stop on first error. Null means use default from options.
    /// </summary>
    public bool? StopOnFirstError { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationMetadata"/> class.
    /// </summary>
    /// <param name="validationType">The type to validate.</param>
    /// <param name="ruleSet">Optional rule set name.</param>
    /// <param name="stopOnFirstError">Optional stop on first error override.</param>
    public DomainValidationMetadata(
        Type validationType,
        string? ruleSet = null,
        bool? stopOnFirstError = null)
    {
        ValidationType = validationType ??
            throw new ArgumentNullException(nameof(validationType));
        RuleSet = ruleSet;
        StopOnFirstError = stopOnFirstError;
    }
}

/// <summary>
/// Builder for <see cref="DomainValidationMetadata"/>.
/// </summary>
public sealed class DomainValidationMetadataBuilder
{
    private readonly Type _type;
    private string? _ruleSet;
    private bool? _stopOnFirstError;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationMetadataBuilder"/> class.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    public DomainValidationMetadataBuilder(Type type)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    /// Sets the rule set to evaluate.
    /// </summary>
    public DomainValidationMetadataBuilder WithRuleSet(string ruleSet)
    {
        _ruleSet = ruleSet;
        return this;
    }

    /// <summary>
    /// Sets whether to stop on first error.
    /// </summary>
    public DomainValidationMetadataBuilder StopOnFirstError(bool stop = true)
    {
        _stopOnFirstError = stop;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="DomainValidationMetadata"/> instance.
    /// </summary>
    /// <returns>A new <see cref="DomainValidationMetadata"/> instance.</returns>
    public DomainValidationMetadata Build()
    {
        return new DomainValidationMetadata(_type, _ruleSet, _stopOnFirstError);
    }
}
