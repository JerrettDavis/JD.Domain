using JD.Domain.Abstractions;
using JD.Domain.Validation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JD.Domain.AspNetCore;

/// <summary>
/// <see cref="IExceptionHandler"/> implementation for <see cref="DomainValidationException"/>.
/// Use this with .NET 8+ exception handling middleware.
/// </summary>
public sealed class DomainExceptionHandler : IExceptionHandler
{
    private readonly DomainValidationOptions _options;
    private readonly ValidationProblemDetailsFactory _factory;
    private readonly ILogger<DomainExceptionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainExceptionHandler"/> class.
    /// </summary>
    public DomainExceptionHandler(
        DomainValidationOptions options,
        ValidationProblemDetailsFactory factory,
        ILogger<DomainExceptionHandler> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainValidationException domainException)
        {
            return false;
        }

        _logger.LogWarning(domainException,
            "Domain validation exception handled: {ErrorCount} errors",
            domainException.Errors.Count);

        var problemDetails = _factory.CreateFromException(
            domainException,
            httpContext,
            _options.ValidationFailureStatusCode);

        httpContext.Response.StatusCode = _options.ValidationFailureStatusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}
