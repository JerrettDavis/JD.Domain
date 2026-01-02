using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JD.Domain.Abstractions;

/// <summary>
/// Defines the contract for evaluating domain rules against instances.
/// </summary>
public interface IDomainEngine
{
    /// <summary>
    /// Evaluates domain rules against the specified instance asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the instance to evaluate.</typeparam>
    /// <param name="instance">The instance to evaluate.</param>
    /// <param name="options">The evaluation options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The evaluation result.</returns>
    ValueTask<RuleEvaluationResult> EvaluateAsync<T>(
        T instance,
        RuleEvaluationOptions? options = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Evaluates domain rules against the specified instance synchronously.
    /// </summary>
    /// <typeparam name="T">The type of the instance to evaluate.</typeparam>
    /// <param name="instance">The instance to evaluate.</param>
    /// <param name="options">The evaluation options.</param>
    /// <returns>The evaluation result.</returns>
    RuleEvaluationResult Evaluate<T>(
        T instance,
        RuleEvaluationOptions? options = null) where T : class;
}
