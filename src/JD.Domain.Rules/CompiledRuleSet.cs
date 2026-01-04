using JD.Domain.Abstractions;

namespace JD.Domain.Rules;

/// <summary>
/// Represents a compiled rule set that can be evaluated at runtime.
/// </summary>
/// <typeparam name="T">The type this rule set applies to.</typeparam>
public sealed class CompiledRuleSet<T> where T : class
{
    private readonly List<CompiledRule<T>> _rules;

    /// <summary>
    /// Gets the name of the rule set.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the target type name.
    /// </summary>
    public string TargetType { get; }

    /// <summary>
    /// Gets the compiled rules in this set.
    /// </summary>
    public IReadOnlyList<CompiledRule<T>> Rules => _rules.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledRuleSet{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the rule set.</param>
    /// <param name="rules">The compiled rules.</param>
    public CompiledRuleSet(string name, IEnumerable<CompiledRule<T>> rules)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

        Name = name;
        TargetType = typeof(T).FullName ?? typeof(T).Name;
        _rules = rules?.ToList() ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <summary>
    /// Evaluates all rules against the specified instance.
    /// </summary>
    /// <param name="instance">The instance to evaluate.</param>
    /// <param name="options">The evaluation options.</param>
    /// <returns>The evaluation result.</returns>
    public RuleEvaluationResult Evaluate(T instance, RuleEvaluationOptions? options = null)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        options ??= RuleEvaluationOptions.Default;

        var errors = new List<DomainError>();
        var warnings = new List<DomainError>();
        var info = new List<DomainError>();
        var rulesEvaluated = 0;

        foreach (var rule in _rules)
        {
            rulesEvaluated++;

            bool passed;
            try
            {
                passed = rule.Evaluate(instance);
            }
            catch (Exception ex)
            {
                // Rule evaluation failed due to exception - treat as failed
                passed = false;
                var error = new DomainError
                {
                    Code = rule.Manifest.Id,
                    Message = $"Rule evaluation failed: {ex.Message}",
                    Severity = RuleSeverity.Error
                };
                errors.Add(error);

                if (options.StopOnFirstError)
                {
                    break;
                }
                continue;
            }

            if (!passed)
            {
                var error = new DomainError
                {
                    Code = rule.Manifest.Id,
                    Message = rule.Manifest.Message ?? $"Rule {rule.Manifest.Id} failed",
                    Severity = rule.Manifest.Severity
                };

                switch (rule.Manifest.Severity)
                {
                    case RuleSeverity.Info:
                        if (options.IncludeInfo)
                        {
                            info.Add(error);
                        }
                        break;
                    case RuleSeverity.Warning:
                        warnings.Add(error);
                        break;
                    case RuleSeverity.Error:
                    case RuleSeverity.Critical:
                        errors.Add(error);
                        if (options.StopOnFirstError)
                        {
                            goto done;
                        }
                        break;
                }
            }
        }

    done:
        return new RuleEvaluationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors.AsReadOnly(),
            Warnings = warnings.AsReadOnly(),
            Info = info.AsReadOnly(),
            RulesEvaluated = rulesEvaluated,
            RuleSetsEvaluated = new List<string> { Name }.AsReadOnly()
        };
    }
}
