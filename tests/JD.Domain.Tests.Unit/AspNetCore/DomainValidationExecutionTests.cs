using JD.Domain.Abstractions;
using JD.Domain.AspNetCore;
using JD.Domain.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JD.Domain.Tests.Unit.AspNetCore;

public sealed class DomainValidationExecutionTests
{
    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task DomainValidationAttribute_SkipsWhenNoEngine()
    {
        var services = new ServiceCollection()
            .AddSingleton(new DomainValidationOptions())
            .AddSingleton(new ValidationProblemDetailsFactory());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var actionArguments = new Dictionary<string, object?> { ["model"] = new TestModel() };
        var filters = new List<IFilterMetadata>();
        var controller = new object();
        var executingContext = new ActionExecutingContext(actionContext, filters, actionArguments, controller);

        var attribute = new DomainValidationAttribute();
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, filters, controller));
        };

        await attribute.OnActionExecutionAsync(executingContext, next);

        Assert.True(nextCalled);
        Assert.Null(executingContext.Result);
    }

    [Fact]
    public async Task DomainValidationAttribute_ValidatesSpecificTypeAndStopsOnFailure()
    {
        var engine = new StubDomainEngine
        {
            Result = RuleEvaluationResult.Failure(DomainError.Create("E1", "Invalid"))
        };

        var options = new DomainValidationOptions
        {
            DefaultRuleSet = "Default",
            StopOnFirstError = false,
            IncludeInfo = true,
            ValidationFailureStatusCode = StatusCodes.Status422UnprocessableEntity
        };

        var services = new ServiceCollection()
            .AddSingleton<IDomainEngine>(engine)
            .AddSingleton(options)
            .AddSingleton(new ValidationProblemDetailsFactory());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var actionArguments = new Dictionary<string, object?> { ["model"] = new TestModel() };
        var filters = new List<IFilterMetadata>();
        var controller = new object();
        var executingContext = new ActionExecutingContext(actionContext, filters, actionArguments, controller);

        var attribute = new DomainValidationAttribute
        {
            ValidationType = typeof(TestModel),
            RuleSet = "Create",
            StopOnFirstError = true
        };

        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, filters, controller));
        };

        await attribute.OnActionExecutionAsync(executingContext, next);

        Assert.False(nextCalled);
        Assert.NotNull(executingContext.Result);
        Assert.Single(engine.Options);
        Assert.Equal("Create", engine.Options[0]?.RuleSet);
        Assert.True(engine.Options[0]!.StopOnFirstError);
    }

    [Fact]
    public async Task DomainValidationAttribute_ValidatesClassArguments_WhenNoValidationType()
    {
        var engine = new StubDomainEngine();
        var services = new ServiceCollection()
            .AddSingleton<IDomainEngine>(engine)
            .AddSingleton(new DomainValidationOptions())
            .AddSingleton(new ValidationProblemDetailsFactory());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var actionArguments = new Dictionary<string, object?>
        {
            ["model"] = new TestModel(),
            ["name"] = "not validated"
        };
        var filters = new List<IFilterMetadata>();
        var controller = new object();
        var executingContext = new ActionExecutingContext(actionContext, filters, actionArguments, controller);

        var attribute = new DomainValidationAttribute();
        ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(actionContext, filters, controller));

        await attribute.OnActionExecutionAsync(executingContext, next);

        Assert.Single(engine.Options);
    }

    [Fact]
    public async Task DomainValidationEndpointFilter_UsesMetadataOverrides()
    {
        var engine = new StubDomainEngine
        {
            Result = RuleEvaluationResult.Failure(DomainError.Create("E1", "Invalid"))
        };

        var services = new ServiceCollection()
            .AddSingleton<IDomainEngine>(engine)
            .AddSingleton(new DomainValidationOptions())
            .AddSingleton(new ValidationProblemDetailsFactory());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var metadata = new DomainValidationMetadata(typeof(TestModel), "Create", true);
        httpContext.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(metadata), "test"));

        var context = new TestEndpointFilterContext(httpContext, new object?[] { new TestModel() });
        var filter = new DomainValidationEndpointFilter<TestModel>();

        var nextCalled = false;
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("next");
        };

        var result = await filter.InvokeAsync(context, next);

        Assert.False(nextCalled);
        Assert.NotNull(result);
        Assert.Single(engine.Options);
        Assert.Equal("Create", engine.Options[0]?.RuleSet);
        Assert.True(engine.Options[0]!.StopOnFirstError);
    }

    [Fact]
    public async Task DomainValidationEndpointFilter_SkipsWhenNoEngineOrArgument()
    {
        var services = new ServiceCollection()
            .AddSingleton(new DomainValidationOptions())
            .AddSingleton(new ValidationProblemDetailsFactory());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var context = new TestEndpointFilterContext(httpContext, new object?[] { "not a model" });
        var filter = new DomainValidationEndpointFilter<TestModel>();

        var nextCalled = false;
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("next");
        };

        var result = await filter.InvokeAsync(context, next);

        Assert.True(nextCalled);
        Assert.Equal("next", result);
    }

    [Fact]
    public async Task DomainValidationEndpointFilter_SkipsWhenArgumentMissing()
    {
        var engine = new StubDomainEngine();
        var services = new ServiceCollection()
            .AddSingleton<IDomainEngine>(engine)
            .AddSingleton(new DomainValidationOptions())
            .AddSingleton(new ValidationProblemDetailsFactory());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var context = new TestEndpointFilterContext(httpContext, new object?[] { "not a model" });
        var filter = new DomainValidationEndpointFilter<TestModel>();

        var nextCalled = false;
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("next");
        };

        var result = await filter.InvokeAsync(context, next);

        Assert.True(nextCalled);
        Assert.Equal("next", result);
        Assert.Empty(engine.Options);
    }

    [Fact]
    public async Task DomainValidationEndpointFilter_ReturnsNext_WhenValid()
    {
        var engine = new StubDomainEngine
        {
            Result = RuleEvaluationResult.Success()
        };

        var services = new ServiceCollection()
            .AddSingleton<IDomainEngine>(engine)
            .AddSingleton(new DomainValidationOptions())
            .AddSingleton(new ValidationProblemDetailsFactory());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var context = new TestEndpointFilterContext(httpContext, new object?[] { new TestModel() });
        var filter = new DomainValidationEndpointFilter<TestModel>();

        var nextCalled = false;
        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return ValueTask.FromResult<object?>("next");
        };

        var result = await filter.InvokeAsync(context, next);

        Assert.True(nextCalled);
        Assert.Equal("next", result);
    }

    private sealed class StubDomainEngine : IDomainEngine
    {
        public RuleEvaluationResult Result { get; set; } = RuleEvaluationResult.Success();
        public List<RuleEvaluationOptions?> Options { get; } = new();

        public ValueTask<RuleEvaluationResult> EvaluateAsync<T>(T instance, RuleEvaluationOptions? options = null, CancellationToken cancellationToken = default) where T : class
        {
            Options.Add(options);
            return ValueTask.FromResult(Result);
        }

        public RuleEvaluationResult Evaluate<T>(T instance, RuleEvaluationOptions? options = null) where T : class
        {
            Options.Add(options);
            return Result;
        }

        public RuleEvaluationResult Evaluate<T>(T instance, RuleSetManifest ruleSet) where T : class
        {
            return Result;
        }
    }

    private sealed class TestEndpointFilterContext : EndpointFilterInvocationContext
    {
        private readonly IList<object?> _arguments;

        public TestEndpointFilterContext(HttpContext httpContext, IList<object?> arguments)
        {
            HttpContext = httpContext;
            _arguments = arguments;
        }

        public override HttpContext HttpContext { get; }
        public override IList<object?> Arguments => _arguments;

        public override T GetArgument<T>(int index)
        {
            return (T)_arguments[index]!;
        }
    }
}
