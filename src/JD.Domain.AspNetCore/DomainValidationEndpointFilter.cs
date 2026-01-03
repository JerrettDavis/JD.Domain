using JD.Domain.Abstractions;
using JD.Domain.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JD.Domain.AspNetCore;

/// <summary>
/// Endpoint filter that performs domain validation on request bodies.
/// </summary>
/// <typeparam name="T">The type to validate.</typeparam>
public sealed class DomainValidationEndpointFilter<T> : IEndpointFilter where T : class
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var engine = context.HttpContext.RequestServices.GetService<IDomainEngine>();

        // If no engine is registered, skip validation
        if (engine is null)
        {
            return await next(context);
        }

        var options = context.HttpContext.RequestServices
            .GetRequiredService<DomainValidationOptions>();
        var factory = context.HttpContext.RequestServices
            .GetRequiredService<ValidationProblemDetailsFactory>();

        // Find the argument of type T
        var argument = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (argument is null)
        {
            return await next(context);
        }

        // Get metadata for custom rule set
        var metadata = context.HttpContext
            .GetEndpoint()?
            .Metadata
            .GetMetadata<DomainValidationMetadata>();

        var evalOptions = new RuleEvaluationOptions
        {
            RuleSet = metadata?.RuleSet ?? options.DefaultRuleSet,
            StopOnFirstError = metadata?.StopOnFirstError ?? options.StopOnFirstError,
            IncludeInfo = options.IncludeInfo
        };

        var result = await engine.EvaluateAsync(argument, evalOptions);

        if (!result.IsValid)
        {
            var problemDetails = factory.CreateFromResult(
                result,
                context.HttpContext,
                options.ValidationFailureStatusCode);

            return Results.Problem(problemDetails);
        }

        return await next(context);
    }
}
