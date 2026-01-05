using JD.Domain.Abstractions;

namespace JD.Domain.Tests.Unit.Abstractions;

public sealed class AbstractionsAdditionalTests
{
    [Fact]
    public void DomainContext_Empty_ReturnsDefaults()
    {
        var context = DomainContext.Empty();

        Assert.NotNull(context.Properties);
        Assert.Empty(context.Properties);
        Assert.True(context.Timestamp <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void DomainContext_WithCorrelationId_SetsValue()
    {
        var context = DomainContext.WithCorrelationId("trace-123");

        Assert.Equal("trace-123", context.CorrelationId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DomainContext_WithCorrelationId_ThrowsOnInvalid(string? value)
    {
        Assert.Throws<ArgumentException>(() => DomainContext.WithCorrelationId(value!));
    }

    [Fact]
    public void DomainCreateOptions_Default_HasExpectedValues()
    {
        var options = DomainCreateOptions.Default;

        Assert.Null(options.RuleSet);
        Assert.Null(options.Context);
        Assert.False(options.ThrowOnFailure);
        Assert.NotNull(options.Properties);
        Assert.Empty(options.Properties);
    }

    [Fact]
    public void RuleEvaluationResult_Success_ReturnsValidResult()
    {
        var result = RuleEvaluationResult.Success();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void RuleEvaluationResult_Failure_ThrowsOnNullErrors()
    {
        Assert.Throws<ArgumentNullException>(() => RuleEvaluationResult.Failure((IReadOnlyList<DomainError>)null!));
        Assert.Throws<ArgumentNullException>(() => RuleEvaluationResult.Failure((DomainError[])null!));
    }

    [Fact]
    public void RuleEvaluationResult_Failure_PreservesErrors()
    {
        var errors = new[] { DomainError.Create("E1", "Message") };

        var result = RuleEvaluationResult.Failure(errors);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void RuleEvaluationResult_Failure_WithList_PreservesMetadata()
    {
        var errors = new List<DomainError> { DomainError.Create("E1", "Message") };

        var result = RuleEvaluationResult.Failure((IReadOnlyList<DomainError>)errors);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);

        var withMetadata = new RuleEvaluationResult
        {
            Metadata = new Dictionary<string, object?> { ["Source"] = "UnitTest" }
        };

        Assert.Equal("UnitTest", withMetadata.Metadata["Source"]);
    }

    [Fact]
    public void RuleEvaluationOptions_SetsPropertyNameAndContext()
    {
        var options = new RuleEvaluationOptions
        {
            PropertyName = "Name",
            Context = new Dictionary<string, object?> { ["TraceId"] = "trace-1" }
        };

        Assert.Equal("Name", options.PropertyName);
        Assert.Equal("trace-1", options.Context["TraceId"]);
    }

    [Fact]
    public void DomainValidationException_FormatsMessageForEmptyErrors()
    {
        var exception = new DomainValidationException(Array.Empty<DomainError>());

        Assert.Equal("Domain validation failed.", exception.Message);
        Assert.Empty(exception.Errors);
    }

    [Fact]
    public void DomainValidationException_FormatsMessageForSingleError()
    {
        var error = DomainError.Create("E1", "Single error");
        var exception = new DomainValidationException(new[] { error });

        Assert.Equal("Single error", exception.Message);
        Assert.Single(exception.Errors);
    }

    [Fact]
    public void DomainValidationException_FromError_SetsMessage()
    {
        var error = DomainError.Create("E1", "Single error");
        var exception = new DomainValidationException(error);

        Assert.Equal("Single error", exception.Message);
        Assert.Single(exception.Errors);
    }

    [Fact]
    public void DomainValidationException_FormatsMessageForMultipleErrors()
    {
        var errors = new[]
        {
            DomainError.Create("E1", "First error"),
            DomainError.Create("E2", "Second error")
        };

        var exception = new DomainValidationException(errors);

        Assert.Contains("2 errors", exception.Message);
        Assert.StartsWith("Domain validation failed", exception.Message);
    }

    [Fact]
    public void DomainValidationException_ThrowsOnNullErrors()
    {
        Assert.Throws<ArgumentNullException>(() => new DomainValidationException((IReadOnlyList<DomainError>)null!));
        Assert.Throws<ArgumentNullException>(() => new DomainValidationException((DomainError)null!));
    }

    [Fact]
    public void DomainValidationException_MessageConstructor_SetsErrors()
    {
        var exception = new DomainValidationException("Validation failed");

        Assert.Single(exception.Errors);
        Assert.Equal("ValidationError", exception.Errors[0].Code);
    }

    [Fact]
    public void DomainValidationException_MessageAndInner_SetsInnerException()
    {
        var inner = new InvalidOperationException("boom");
        var exception = new DomainValidationException("Validation failed", inner);

        Assert.Same(inner, exception.InnerException);
        Assert.Single(exception.Errors);
    }
}
