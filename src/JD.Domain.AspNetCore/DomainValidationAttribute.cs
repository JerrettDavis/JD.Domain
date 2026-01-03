using JD.Domain.Abstractions;
using JD.Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace JD.Domain.AspNetCore;

/// <summary>
/// Action filter attribute that performs domain validation on action parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class DomainValidationAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// Gets or sets the type to validate. If null, validates all class parameters.
    /// </summary>
    public Type? ValidationType { get; set; }

    /// <summary>
    /// Gets or sets the rule set to evaluate.
    /// </summary>
    public string? RuleSet { get; set; }

    /// <summary>
    /// Gets or sets whether to stop on first error.
    /// </summary>
    public bool StopOnFirstError { get; set; }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var engine = context.HttpContext.RequestServices.GetService<IDomainEngine>();

        // If no engine is registered, skip validation
        if (engine is null)
        {
            await next();
            return;
        }

        var options = context.HttpContext.RequestServices
            .GetRequiredService<DomainValidationOptions>();
        var factory = context.HttpContext.RequestServices
            .GetRequiredService<ValidationProblemDetailsFactory>();

        var evalOptions = new RuleEvaluationOptions
        {
            RuleSet = RuleSet ?? options.DefaultRuleSet,
            StopOnFirstError = StopOnFirstError || options.StopOnFirstError,
            IncludeInfo = options.IncludeInfo
        };

        // Get arguments to validate
        var argumentsToValidate = GetArgumentsToValidate(context);

        foreach (var (_, argument) in argumentsToValidate)
        {
            if (argument is null) continue;

            var result = await EvaluateDynamicAsync(engine, argument, evalOptions);

            if (!result.IsValid)
            {
                var problemDetails = factory.CreateFromResult(
                    result,
                    context.HttpContext,
                    options.ValidationFailureStatusCode);

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = options.ValidationFailureStatusCode
                };
                return;
            }
        }

        await next();
    }

    private IEnumerable<(string Name, object? Value)> GetArgumentsToValidate(
        ActionExecutingContext context)
    {
        if (ValidationType is not null)
        {
            // Validate specific type
            foreach (var arg in context.ActionArguments)
            {
                if (arg.Value?.GetType() == ValidationType)
                {
                    yield return (arg.Key, arg.Value);
                }
            }
        }
        else
        {
            // Validate all class types (excluding primitives, strings, etc.)
            foreach (var arg in context.ActionArguments)
            {
                if (arg.Value is not null &&
                    arg.Value.GetType().IsClass &&
                    arg.Value is not string)
                {
                    yield return (arg.Key, arg.Value);
                }
            }
        }
    }

    private static async ValueTask<RuleEvaluationResult> EvaluateDynamicAsync(
        IDomainEngine engine,
        object instance,
        RuleEvaluationOptions options)
    {
        // Use reflection to call the generic EvaluateAsync method
        var method = typeof(IDomainEngine)
            .GetMethod(nameof(IDomainEngine.EvaluateAsync))!
            .MakeGenericMethod(instance.GetType());

        var task = (ValueTask<RuleEvaluationResult>)method
            .Invoke(engine, [instance, options, CancellationToken.None])!;

        return await task;
    }
}
