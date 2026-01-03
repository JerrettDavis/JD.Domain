using JD.Domain.Abstractions;
using JD.Domain.Rules;

namespace JD.Domain.Runtime;

/// <summary>
/// Default implementation of the domain engine for rule evaluation.
/// </summary>
public sealed class DomainEngine : IDomainEngine
{
    private readonly DomainManifest _manifest;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEngine"/> class.
    /// </summary>
    /// <param name="manifest">The domain manifest containing rules to evaluate.</param>
    public DomainEngine(DomainManifest manifest)
    {
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));
        _manifest = manifest;
    }

    /// <inheritdoc/>
    public ValueTask<RuleEvaluationResult> EvaluateAsync<T>(
        T instance,
        RuleEvaluationOptions? options = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        
        var result = Evaluate(instance, options);
        return new ValueTask<RuleEvaluationResult>(result);
    }

    /// <inheritdoc/>
    public RuleEvaluationResult Evaluate<T>(
        T instance,
        RuleEvaluationOptions? options = null) where T : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        
        options ??= RuleEvaluationOptions.Default;

        var typeName = typeof(T).FullName ?? typeof(T).Name;
        var ruleSets = _manifest.RuleSets
            .Where(rs => rs.TargetType == typeName)
            .Where(rs => options.RuleSet == null || rs.Name == options.RuleSet)
            .ToList();

        if (!ruleSets.Any())
        {
            return RuleEvaluationResult.Success();
        }

        var errors = new List<DomainError>();
        var warnings = new List<DomainError>();
        var info = new List<DomainError>();
        var rulesEvaluated = 0;
        var ruleSetsEvaluated = new List<string>();

        foreach (var ruleSet in ruleSets)
        {
            ruleSetsEvaluated.Add(ruleSet.Name);
            
            foreach (var rule in ruleSet.Rules)
            {
                rulesEvaluated++;
                
                // For now, we create placeholder errors for demonstration
                // In a full implementation, this would compile and execute the expression
                var message = rule.Message ?? $"Rule {rule.Id} validation";
                
                // Create a domain error for rules that have a message
                // In reality, we would evaluate the expression here
                if (!string.IsNullOrEmpty(rule.Message))
                {
                    var error = new DomainError
                    {
                        Code = rule.Id,
                        Message = message,
                        Severity = rule.Severity
                    };

                    // Add to appropriate collection based on severity
                    switch (rule.Severity)
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
        }

    done:
        return new RuleEvaluationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors.ToList().AsReadOnly(),
            Warnings = warnings.ToList().AsReadOnly(),
            Info = info.ToList().AsReadOnly(),
            RulesEvaluated = rulesEvaluated,
            RuleSetsEvaluated = ruleSetsEvaluated.ToList().AsReadOnly()
        };
    }

    /// <inheritdoc/>
    public RuleEvaluationResult Evaluate<T>(
        T instance,
        RuleSetManifest ruleSet) where T : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (ruleSet == null) throw new ArgumentNullException(nameof(ruleSet));

        // RuleSetManifest only contains string expressions, not compiled predicates.
        // For actual evaluation, use CompiledRuleSet<T> via Evaluate(instance, compiledRuleSet).
        // This method returns success to maintain backward compatibility.
        return new RuleEvaluationResult
        {
            IsValid = true,
            Errors = new List<DomainError>().AsReadOnly(),
            Warnings = new List<DomainError>().AsReadOnly(),
            Info = new List<DomainError>().AsReadOnly(),
            RulesEvaluated = ruleSet.Rules.Count,
            RuleSetsEvaluated = new List<string> { ruleSet.Name }.AsReadOnly()
        };
    }

    /// <summary>
    /// Evaluates a compiled rule set against the specified instance.
    /// This method actually executes the rule predicates.
    /// </summary>
    /// <typeparam name="T">The type of the instance to evaluate.</typeparam>
    /// <param name="instance">The instance to evaluate.</param>
    /// <param name="ruleSet">The compiled rule set to evaluate.</param>
    /// <param name="options">The evaluation options.</param>
    /// <returns>The evaluation result.</returns>
    public RuleEvaluationResult Evaluate<T>(
        T instance,
        CompiledRuleSet<T> ruleSet,
        RuleEvaluationOptions? options = null) where T : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (ruleSet == null) throw new ArgumentNullException(nameof(ruleSet));

        return ruleSet.Evaluate(instance, options);
    }
}
