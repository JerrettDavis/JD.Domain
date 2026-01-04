using JD.Domain.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JD.Domain.AspNetCore;

/// <summary>
/// Default implementation that creates <see cref="DomainContext"/> from <see cref="HttpContext"/>.
/// </summary>
public sealed class HttpDomainContextFactory : IDomainContextFactory
{
    private readonly DomainValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpDomainContextFactory"/> class.
    /// </summary>
    /// <param name="options">The domain validation options.</param>
    public HttpDomainContextFactory(DomainValidationOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public DomainContext CreateContext(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        // Use custom factory if configured
        if (_options.DomainContextFactory is not null)
        {
            return _options.DomainContextFactory(httpContext);
        }

        // Build properties dictionary
        var properties = new Dictionary<string, object?>(_options.AdditionalContext)
        {
            ["HttpMethod"] = httpContext.Request.Method,
            ["Path"] = httpContext.Request.Path.Value,
            ["UserAgent"] = httpContext.Request.Headers.UserAgent.ToString()
        };

        return new DomainContext
        {
            CorrelationId = httpContext.TraceIdentifier,
            Actor = httpContext.User.Identity?.Name,
            Timestamp = DateTimeOffset.UtcNow,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            Properties = properties
        };
    }
}
