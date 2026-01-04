using JD.Domain.Abstractions;
using JD.Domain.Validation;

namespace JD.Domain.Tests.Unit.Validation;

public class ValidationProblemDetailsTests
{
    [Fact]
    public void ValidationProblemDetails_HasDefaultValues()
    {
        var details = new ValidationProblemDetails();

        Assert.NotNull(details.Errors);
        Assert.Empty(details.Errors);
        Assert.NotNull(details.DomainErrors);
        Assert.Empty(details.DomainErrors);
        Assert.Null(details.CorrelationId);
        Assert.Empty(details.RuleSetsEvaluated);
    }

    [Fact]
    public void ValidationProblemDetails_TypePrefix_IsCorrect()
    {
        Assert.Equal("https://jd.domain/validation-errors/", ValidationProblemDetails.TypePrefix);
    }
}

public class DomainValidationErrorTests
{
    [Fact]
    public void FromDomainError_MapsAllProperties()
    {
        var domainError = DomainError.Create("TestCode", "Test message", "TestProperty");

        var validationError = DomainValidationError.FromDomainError(domainError);

        Assert.Equal("TestCode", validationError.Code);
        Assert.Equal("Test message", validationError.Message);
        Assert.Equal("TestProperty", validationError.Target);
        Assert.Equal("Error", validationError.Severity);
    }

    [Fact]
    public void FromDomainError_MapsMetadata_WhenPresent()
    {
        var metadata = new Dictionary<string, object?> { ["key"] = "value" };
        var domainError = new DomainError
        {
            Code = "TestCode",
            Message = "Test message",
            Metadata = metadata
        };

        var validationError = DomainValidationError.FromDomainError(domainError);

        Assert.NotNull(validationError.Metadata);
        Assert.Equal("value", validationError.Metadata["key"]);
    }

    [Fact]
    public void FromDomainError_OmitsMetadata_WhenEmpty()
    {
        var domainError = DomainError.Create("TestCode", "Test message");

        var validationError = DomainValidationError.FromDomainError(domainError);

        Assert.Null(validationError.Metadata);
    }

    [Fact]
    public void FromDomainError_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DomainValidationError.FromDomainError(null!));
    }
}

public class ProblemDetailsBuilderTests
{
    [Fact]
    public void Create_ReturnsNewBuilder()
    {
        var builder = ProblemDetailsBuilder.Create();

        Assert.NotNull(builder);
    }

    [Fact]
    public void Build_ReturnsValidProblemDetails()
    {
        var details = ProblemDetailsBuilder.Create().Build();

        Assert.NotNull(details);
        Assert.Equal(400, details.Status);
        Assert.Equal("One or more validation errors occurred.", details.Title);
        Assert.Equal("https://jd.domain/validation-errors/validation-failed", details.Type);
    }

    [Fact]
    public void WithTitle_SetsTitle()
    {
        var details = ProblemDetailsBuilder.Create()
            .WithTitle("Custom Title")
            .Build();

        Assert.Equal("Custom Title", details.Title);
    }

    [Fact]
    public void WithStatus_SetsStatus()
    {
        var details = ProblemDetailsBuilder.Create()
            .WithStatus(422)
            .Build();

        Assert.Equal(422, details.Status);
    }

    [Fact]
    public void WithDetail_SetsDetail()
    {
        var details = ProblemDetailsBuilder.Create()
            .WithDetail("Detailed message")
            .Build();

        Assert.Equal("Detailed message", details.Detail);
    }

    [Fact]
    public void WithInstance_SetsInstance()
    {
        var details = ProblemDetailsBuilder.Create()
            .WithInstance("/api/test")
            .Build();

        Assert.Equal("/api/test", details.Instance);
    }

    [Fact]
    public void WithCorrelationId_SetsCorrelationId()
    {
        var details = ProblemDetailsBuilder.Create()
            .WithCorrelationId("test-correlation-id")
            .Build();

        Assert.Equal("test-correlation-id", details.CorrelationId);
    }

    [Fact]
    public void FromEvaluationResult_MapsErrors()
    {
        var result = new RuleEvaluationResult
        {
            IsValid = false,
            Errors = new[]
            {
                DomainError.Create("Error1", "Error message 1", "Property1"),
                DomainError.Create("Error2", "Error message 2", "Property1")
            },
            RuleSetsEvaluated = new[] { "Create" }
        };

        var details = ProblemDetailsBuilder.Create()
            .FromEvaluationResult(result)
            .Build();

        Assert.Equal(2, details.DomainErrors.Count);
        Assert.Contains("Property1", details.Errors.Keys);
        Assert.Equal(2, details.Errors["Property1"].Length);
        Assert.Single(details.RuleSetsEvaluated);
        Assert.Equal("Create", details.RuleSetsEvaluated[0]);
    }

    [Fact]
    public void FromEvaluationResult_SingleError_SetsDetailToMessage()
    {
        var result = new RuleEvaluationResult
        {
            IsValid = false,
            Errors = new[] { DomainError.Create("Error1", "Single error message") }
        };

        var details = ProblemDetailsBuilder.Create()
            .FromEvaluationResult(result)
            .Build();

        Assert.Equal("Single error message", details.Detail);
    }

    [Fact]
    public void FromEvaluationResult_MultipleErrors_SetsDetailToCount()
    {
        var result = new RuleEvaluationResult
        {
            IsValid = false,
            Errors = new[]
            {
                DomainError.Create("Error1", "Error 1"),
                DomainError.Create("Error2", "Error 2"),
                DomainError.Create("Error3", "Error 3")
            }
        };

        var details = ProblemDetailsBuilder.Create()
            .FromEvaluationResult(result)
            .Build();

        Assert.Equal("Validation failed with 3 errors.", details.Detail);
    }

    [Fact]
    public void FromException_MapsErrors()
    {
        var errors = new[]
        {
            DomainError.Create("Error1", "Error message 1", "Property1")
        };
        var exception = new DomainValidationException(errors);

        var details = ProblemDetailsBuilder.Create()
            .FromException(exception)
            .Build();

        Assert.Single(details.DomainErrors);
        Assert.Equal("Error1", details.DomainErrors[0].Code);
    }

    [Fact]
    public void WithExtension_AddsExtension()
    {
        var details = ProblemDetailsBuilder.Create()
            .WithExtension("customKey", "customValue")
            .Build();

        Assert.True(details.Extensions.ContainsKey("customKey"));
        Assert.Equal("customValue", details.Extensions["customKey"]);
    }
}

public class ValidationProblemDetailsFactoryTests
{
    private readonly ValidationProblemDetailsFactory _factory = new();

    [Fact]
    public void CreateFromResult_CreatesProblemDetails()
    {
        var result = new RuleEvaluationResult
        {
            IsValid = false,
            Errors = new[] { DomainError.Create("TestError", "Test message") }
        };

        var details = _factory.CreateFromResult(result);

        Assert.NotNull(details);
        Assert.Single(details.DomainErrors);
    }

    [Fact]
    public void CreateFromResult_WithStatusCode_OverridesDefault()
    {
        var result = new RuleEvaluationResult
        {
            IsValid = false,
            Errors = new[] { DomainError.Create("TestError", "Test message") }
        };

        var details = _factory.CreateFromResult(result, statusCode: 422);

        Assert.Equal(422, details.Status);
    }

    [Fact]
    public void CreateFromException_CreatesProblemDetails()
    {
        var exception = new DomainValidationException("Test error");

        var details = _factory.CreateFromException(exception);

        Assert.NotNull(details);
        Assert.Single(details.DomainErrors);
    }

    [Fact]
    public void CreateFromErrors_CreatesProblemDetails()
    {
        var errors = new[]
        {
            DomainError.Create("Error1", "Message 1"),
            DomainError.Create("Error2", "Message 2")
        };

        var details = _factory.CreateFromErrors(errors);

        Assert.NotNull(details);
        Assert.Equal(2, details.DomainErrors.Count);
        Assert.Equal("Validation failed with 2 errors.", details.Detail);
    }
}
