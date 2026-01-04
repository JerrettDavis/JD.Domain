using JD.Domain.Abstractions;
using JD.Domain.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JD.Domain.AspNetCore;

/// <summary>
/// Middleware that handles <see cref="DomainValidationException"/> and converts them to ProblemDetails responses.
/// </summary>
public sealed class DomainValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DomainValidationOptions _options;
    private readonly ValidationProblemDetailsFactory _factory;
    private readonly ILogger<DomainValidationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainValidationMiddleware"/> class.
    /// </summary>
    public DomainValidationMiddleware(
        RequestDelegate next,
        DomainValidationOptions options,
        ValidationProblemDetailsFactory factory,
        ILogger<DomainValidationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainValidationException ex) when (_options.HandleExceptionsGlobally)
        {
            _logger.LogWarning(ex,
                "Domain validation failed for {Path}: {Message}",
                context.Request.Path,
                ex.Message);

            await WriteValidationProblemDetailsAsync(context, ex);
        }
    }

    private async Task WriteValidationProblemDetailsAsync(
        HttpContext context,
        DomainValidationException exception)
    {
        context.Response.StatusCode = _options.ValidationFailureStatusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = _factory.CreateFromException(
            exception,
            context,
            _options.ValidationFailureStatusCode);

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            context.RequestAborted);
    }
}

/// <summary>
/// Extension methods for adding domain validation middleware to the pipeline.
/// </summary>
public static class DomainValidationMiddlewareExtensions
{
    /// <summary>
    /// Adds the domain validation middleware to the request pipeline.
    /// This middleware catches <see cref="DomainValidationException"/> and returns
    /// properly formatted ProblemDetails responses.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseDomainValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<DomainValidationMiddleware>();
    }
}
