using JD.Domain.Modeling;

namespace JD.Domain.Rules;

/// <summary>
/// Extension methods for adding rules to the domain builder.
/// </summary>
public static class DomainBuilderRulesExtensions
{
    /// <summary>
    /// Adds a rule set for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="builder">The domain builder.</param>
    /// <param name="configure">The rule configuration action.</param>
    /// <returns>The domain builder for chaining.</returns>
    public static DomainBuilder Rules<T>(
        this DomainBuilder builder,
        Action<RuleSetBuilder<T>> configure) where T : class
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var ruleSetBuilder = new RuleSetBuilder<T>();
        configure(ruleSetBuilder);

        var ruleSet = ruleSetBuilder.Build();
        builder.AddRuleSet(ruleSet);

        return builder;
    }

    /// <summary>
    /// Adds a named rule set for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="builder">The domain builder.</param>
    /// <param name="name">The name of the rule set (e.g., "Create", "Update").</param>
    /// <param name="configure">The rule configuration action.</param>
    /// <returns>The domain builder for chaining.</returns>
    public static DomainBuilder Rules<T>(
        this DomainBuilder builder,
        string name,
        Action<RuleSetBuilder<T>> configure) where T : class
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var ruleSetBuilder = new RuleSetBuilder<T>(name);
        configure(ruleSetBuilder);

        var ruleSet = ruleSetBuilder.Build();
        builder.AddRuleSet(ruleSet);

        return builder;
    }
}
