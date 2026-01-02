using JD.Domain.Abstractions;

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
        ArgumentNullException.ThrowIfNull(manifest);
        _manifest = manifest;
    }

    /// <inheritdoc/>
    public ValueTask<RuleEvaluationResult> EvaluateAsync<T>(
        T instance,
        RuleEvaluationOptions? options = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        
        var result = Evaluate(instance, options);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc/>
    public RuleEvaluationResult Evaluate<T>(
        T instance,
        RuleEvaluationOptions? options = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        
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
            Errors = errors.AsReadOnly(),
            Warnings = warnings.AsReadOnly(),
            Info = info.AsReadOnly(),
            RulesEvaluated = rulesEvaluated,
            RuleSetsEvaluated = ruleSetsEvaluated.AsReadOnly()
        };
    }
}
