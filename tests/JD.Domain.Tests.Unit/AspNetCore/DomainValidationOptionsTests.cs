using JD.Domain.Abstractions;
using JD.Domain.AspNetCore;
using JD.Domain.Modeling;
using JD.Domain.Runtime;
using JD.Domain.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace JD.Domain.Tests.Unit.AspNetCore;

public class DomainValidationOptionsTests
{
    [Fact]
    public void Options_HaveCorrectDefaults()
    {
        var options = new DomainValidationOptions();

        Assert.Null(options.DefaultRuleSet);
        Assert.False(options.StopOnFirstError);
        Assert.False(options.IncludeInfo);
        Assert.True(options.IncludeWarnings);
        Assert.True(options.SuppressGetRequestValidation);
        Assert.Equal(StatusCodes.Status400BadRequest, options.ValidationFailureStatusCode);
        Assert.True(options.HandleExceptionsGlobally);
        Assert.Null(options.DomainContextFactory);
        Assert.NotNull(options.AdditionalContext);
        Assert.Empty(options.AdditionalContext);
    }

    [Fact]
    public void Options_CanBeConfigured()
    {
        var options = new DomainValidationOptions
        {
            DefaultRuleSet = "Create",
            StopOnFirstError = true,
            IncludeInfo = true,
            IncludeWarnings = false,
            SuppressGetRequestValidation = false,
            ValidationFailureStatusCode = 422,
            HandleExceptionsGlobally = false
        };

        Assert.Equal("Create", options.DefaultRuleSet);
        Assert.True(options.StopOnFirstError);
        Assert.True(options.IncludeInfo);
        Assert.False(options.IncludeWarnings);
        Assert.False(options.SuppressGetRequestValidation);
        Assert.Equal(422, options.ValidationFailureStatusCode);
        Assert.False(options.HandleExceptionsGlobally);
    }

    [Fact]
    public void AdditionalContext_CanBeModified()
    {
        var options = new DomainValidationOptions();
        options.AdditionalContext["key"] = "value";

        Assert.Single(options.AdditionalContext);
        Assert.Equal("value", options.AdditionalContext["key"]);
    }

    [Fact]
    public void DomainContextFactory_CanBeSet()
    {
        var options = new DomainValidationOptions();
        options.DomainContextFactory = ctx => new DomainContext
        {
            CorrelationId = "custom-id",
            Actor = "custom-actor"
        };

        Assert.NotNull(options.DomainContextFactory);
    }
}

public class DomainValidationMetadataTests
{
    [Fact]
    public void Constructor_SetsType()
    {
        var metadata = new DomainValidationMetadata(typeof(string));

        Assert.Equal(typeof(string), metadata.ValidationType);
        Assert.Null(metadata.RuleSet);
        Assert.Null(metadata.StopOnFirstError);
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var metadata = new DomainValidationMetadata(typeof(string), "Create", true);

        Assert.Equal(typeof(string), metadata.ValidationType);
        Assert.Equal("Create", metadata.RuleSet);
        Assert.True(metadata.StopOnFirstError);
    }

    [Fact]
    public void Constructor_ThrowsOnNullType()
    {
        Assert.Throws<ArgumentNullException>(() => new DomainValidationMetadata(null!));
    }
}

public class DomainValidationMetadataBuilderTests
{
    [Fact]
    public void Build_CreatesMetadata()
    {
        var builder = new DomainValidationMetadataBuilder(typeof(string));
        var metadata = builder.Build();

        Assert.Equal(typeof(string), metadata.ValidationType);
    }

    [Fact]
    public void WithRuleSet_SetsRuleSet()
    {
        var builder = new DomainValidationMetadataBuilder(typeof(string));
        var metadata = builder.WithRuleSet("Update").Build();

        Assert.Equal("Update", metadata.RuleSet);
    }

    [Fact]
    public void StopOnFirstError_SetsFlag()
    {
        var builder = new DomainValidationMetadataBuilder(typeof(string));
        var metadata = builder.StopOnFirstError().Build();

        Assert.True(metadata.StopOnFirstError);
    }

    [Fact]
    public void Fluent_ChainsCorrectly()
    {
        var builder = new DomainValidationMetadataBuilder(typeof(string));
        var metadata = builder
            .WithRuleSet("Create")
            .StopOnFirstError()
            .Build();

        Assert.Equal("Create", metadata.RuleSet);
        Assert.True(metadata.StopOnFirstError);
    }
}

public class HttpDomainContextFactoryTests
{
    [Fact]
    public void Constructor_ThrowsOnNullOptions()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpDomainContextFactory(null!));
    }

    [Fact]
    public void CreateContext_ThrowsOnNullHttpContext()
    {
        var options = new DomainValidationOptions();
        var factory = new HttpDomainContextFactory(options);

        Assert.Throws<ArgumentNullException>(() => factory.CreateContext(null!));
    }

    [Fact]
    public void CreateContext_SetsPropertiesFromHttpContext()
    {
        var options = new DomainValidationOptions();
        var factory = new HttpDomainContextFactory(options);
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace-id";
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/api/test";
        httpContext.Request.Headers.UserAgent = "TestAgent/1.0";

        var context = factory.CreateContext(httpContext);

        Assert.Equal("test-trace-id", context.CorrelationId);
        Assert.Equal("POST", context.Properties["HttpMethod"]);
        Assert.Equal("/api/test", context.Properties["Path"]);
        Assert.Contains("TestAgent", context.Properties["UserAgent"]?.ToString());
    }

    [Fact]
    public void CreateContext_UsesCustomFactoryWhenProvided()
    {
        var options = new DomainValidationOptions
        {
            DomainContextFactory = ctx => new DomainContext
            {
                CorrelationId = "custom-id",
                Actor = "custom-actor"
            }
        };
        var factory = new HttpDomainContextFactory(options);
        var httpContext = new DefaultHttpContext();

        var context = factory.CreateContext(httpContext);

        Assert.Equal("custom-id", context.CorrelationId);
        Assert.Equal("custom-actor", context.Actor);
    }

    [Fact]
    public void CreateContext_IncludesAdditionalContext()
    {
        var options = new DomainValidationOptions();
        options.AdditionalContext["CustomKey"] = "CustomValue";
        var factory = new HttpDomainContextFactory(options);
        var httpContext = new DefaultHttpContext();

        var context = factory.CreateContext(httpContext);

        Assert.Equal("CustomValue", context.Properties["CustomKey"]);
    }

    [Fact]
    public void CreateContext_SetsActorFromUser()
    {
        var options = new DomainValidationOptions();
        var factory = new HttpDomainContextFactory(options);
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        }, "TestAuth"));

        var context = factory.CreateContext(httpContext);

        Assert.Equal("testuser", context.Actor);
    }
}

public class DomainValidationMiddlewareTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        var options = new DomainValidationOptions();
        var factory = new ValidationProblemDetailsFactory();
        var logger = new LoggerFactory().CreateLogger<DomainValidationMiddleware>();
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new DomainValidationMiddleware(next, options, factory, logger);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_DomainValidationException_HandleExceptionsTrue_WritesResponse()
    {
        var options = new DomainValidationOptions { HandleExceptionsGlobally = true };
        var factory = new ValidationProblemDetailsFactory();
        var logger = new LoggerFactory().CreateLogger<DomainValidationMiddleware>();
        var errors = new List<DomainError>
        {
            new() { Code = "TEST", Message = "Test error" }
        }.AsReadOnly();
        RequestDelegate next = _ => throw new DomainValidationException(errors);

        var middleware = new DomainValidationMiddleware(next, options, factory, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DomainValidationException_HandleExceptionsFalse_Rethrows()
    {
        var options = new DomainValidationOptions { HandleExceptionsGlobally = false };
        var factory = new ValidationProblemDetailsFactory();
        var logger = new LoggerFactory().CreateLogger<DomainValidationMiddleware>();
        var errors = new List<DomainError>
        {
            new() { Code = "TEST", Message = "Test error" }
        }.AsReadOnly();
        var exception = new DomainValidationException(errors);
        RequestDelegate next = _ => throw exception;

        var middleware = new DomainValidationMiddleware(next, options, factory, logger);
        var context = new DefaultHttpContext();

        await Assert.ThrowsAsync<DomainValidationException>(() => middleware.InvokeAsync(context));
    }

    [Fact]
    public void Constructor_ThrowsOnNullArguments()
    {
        var options = new DomainValidationOptions();
        var factory = new ValidationProblemDetailsFactory();
        var logger = new LoggerFactory().CreateLogger<DomainValidationMiddleware>();
        RequestDelegate next = _ => Task.CompletedTask;

        Assert.Throws<ArgumentNullException>(() => new DomainValidationMiddleware(null!, options, factory, logger));
        Assert.Throws<ArgumentNullException>(() => new DomainValidationMiddleware(next, null!, factory, logger));
        Assert.Throws<ArgumentNullException>(() => new DomainValidationMiddleware(next, options, null!, logger));
        Assert.Throws<ArgumentNullException>(() => new DomainValidationMiddleware(next, options, factory, null!));
    }
}

public class DomainExceptionHandlerTests
{
    [Fact]
    public void Constructor_ThrowsOnNullArguments()
    {
        var options = new DomainValidationOptions();
        var factory = new ValidationProblemDetailsFactory();
        var logger = new LoggerFactory().CreateLogger<DomainExceptionHandler>();

        Assert.Throws<ArgumentNullException>(() => new DomainExceptionHandler(null!, factory, logger));
        Assert.Throws<ArgumentNullException>(() => new DomainExceptionHandler(options, null!, logger));
        Assert.Throws<ArgumentNullException>(() => new DomainExceptionHandler(options, factory, null!));
    }

    [Fact]
    public async Task TryHandleAsync_NotDomainException_ReturnsFalse()
    {
        var options = new DomainValidationOptions();
        var factory = new ValidationProblemDetailsFactory();
        var logger = new LoggerFactory().CreateLogger<DomainExceptionHandler>();
        var handler = new DomainExceptionHandler(options, factory, logger);
        var context = new DefaultHttpContext();
        var exception = new InvalidOperationException("test");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task TryHandleAsync_DomainValidationException_ReturnsTrue()
    {
        var options = new DomainValidationOptions();
        var factory = new ValidationProblemDetailsFactory();
        var logger = new LoggerFactory().CreateLogger<DomainExceptionHandler>();
        var handler = new DomainExceptionHandler(options, factory, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var errors = new List<DomainError>
        {
            new() { Code = "TEST", Message = "Test error" }
        }.AsReadOnly();
        var exception = new DomainValidationException(errors);

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(400, context.Response.StatusCode);
    }
}

public class DomainValidationServiceExtensionsTests
{
    private class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void AddDomainValidation_RegistersRequiredServices()
    {
        var services = new ServiceCollection();

        services.AddDomainValidation();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<DomainValidationOptions>());
        Assert.NotNull(provider.GetService<ValidationProblemDetailsFactory>());
        Assert.NotNull(provider.GetService<IDomainContextFactory>());
    }

    [Fact]
    public void AddDomainValidation_WithConfigureAction_AppliesConfiguration()
    {
        var services = new ServiceCollection();

        services.AddDomainValidation(options =>
        {
            options.DefaultRuleSet = "TestRuleSet";
            options.StopOnFirstError = true;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DomainValidationOptions>();
        Assert.Equal("TestRuleSet", options.DefaultRuleSet);
        Assert.True(options.StopOnFirstError);
    }

    [Fact]
    public void AddDomainValidation_WithManifest_RegistersEngine()
    {
        var services = new ServiceCollection();
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<TestEntity>()
            .BuildManifest();

        services.AddDomainValidation(manifest);

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IDomainEngine>());
    }

    [Fact]
    public void AddDomainValidation_WithNullManifest_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddDomainValidation((DomainManifest)null!));
    }

    [Fact]
    public void AddDomainValidation_WithManifestFactory_RegistersEngine()
    {
        var services = new ServiceCollection();

        services.AddDomainValidation(_ => JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<TestEntity>()
            .BuildManifest());

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IDomainEngine>());
    }

    [Fact]
    public void AddDomainValidation_WithNullManifestFactory_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddDomainValidation((Func<IServiceProvider, DomainManifest>)null!));
    }

    [Fact]
    public void AddDomainValidation_WithEngine_RegistersEngine()
    {
        var services = new ServiceCollection();
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<TestEntity>()
            .BuildManifest();
        var engine = DomainRuntime.CreateEngine(manifest);

        services.AddDomainValidation(engine);

        var provider = services.BuildServiceProvider();
        var registeredEngine = provider.GetRequiredService<IDomainEngine>();
        Assert.Same(engine, registeredEngine);
    }

    [Fact]
    public void AddDomainValidation_WithNullEngine_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddDomainValidation((IDomainEngine)null!));
    }
}

public class DomainValidationAttributeTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void DomainValidationAttribute_PropertyDefaults()
    {
        var attribute = new DomainValidationAttribute();

        Assert.Null(attribute.ValidationType);
        Assert.Null(attribute.RuleSet);
        Assert.False(attribute.StopOnFirstError);
    }

    [Fact]
    public void DomainValidationAttribute_CanSetProperties()
    {
        var attribute = new DomainValidationAttribute
        {
            ValidationType = typeof(TestModel),
            RuleSet = "Create",
            StopOnFirstError = true
        };

        Assert.Equal(typeof(TestModel), attribute.ValidationType);
        Assert.Equal("Create", attribute.RuleSet);
        Assert.True(attribute.StopOnFirstError);
    }
}

public class MinimalApiExtensionsTests
{
    private class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void WithDomainValidation_Generic_ReturnsBuilder()
    {
        var services = new ServiceCollection();
        services.AddDomainValidation();
        var app = WebApplication.Create();

        var builder = app.MapPost("/test", (TestEntity entity) => Results.Ok(entity));
        var result = builder.WithDomainValidation<TestEntity>();

        Assert.NotNull(result);
        Assert.IsAssignableFrom<RouteHandlerBuilder>(result);
    }

    [Fact]
    public void WithDomainValidation_WithRuleSet_ReturnsBuilder()
    {
        var services = new ServiceCollection();
        services.AddDomainValidation();
        var app = WebApplication.Create();

        var builder = app.MapPost("/test", (TestEntity entity) => Results.Ok(entity));
        var result = builder.WithDomainValidation<TestEntity>("Create");

        Assert.NotNull(result);
        Assert.IsAssignableFrom<RouteHandlerBuilder>(result);
    }

    [Fact]
    public void WithDomainValidation_WithConfigureAction_ReturnsBuilder()
    {
        var services = new ServiceCollection();
        services.AddDomainValidation();
        var app = WebApplication.Create();

        var builder = app.MapPost("/test", (TestEntity entity) => Results.Ok(entity));
        var result = builder.WithDomainValidation<TestEntity>(metadata =>
        {
            metadata.WithRuleSet("Update");
            metadata.StopOnFirstError();
        });

        Assert.NotNull(result);
        Assert.IsAssignableFrom<RouteHandlerBuilder>(result);
    }
}

public class DomainValidationMiddlewareExtensionsTests
{
    [Fact]
    public void UseDomainValidation_ReturnsApplicationBuilder()
    {
        var services = new ServiceCollection();
        services.AddDomainValidation();
        var app = WebApplication.Create();

        var result = app.UseDomainValidation();

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IApplicationBuilder>(result);
    }
}
