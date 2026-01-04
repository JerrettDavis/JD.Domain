using JD.Domain.Abstractions;

namespace JD.Domain.Tests.Unit.Abstractions;

/// <summary>
/// Tests for the Result&lt;T&gt; type.
/// </summary>
public sealed class ResultTests
{
    [Fact]
    public void Success_WithValidValue_CreatesSuccessfulResult()
    {
        // Arrange
        var value = "test-value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(value, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Success_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullValue = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Result<string>.Success(nullValue!));
    }

    [Fact]
    public void Failure_WithSingleError_CreatesFailureResult()
    {
        // Arrange
        var error = DomainError.Create("TEST001", "Test error message");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(error.Code, result.Errors[0].Code);
        Assert.Equal(error.Message, result.Errors[0].Message);
    }

    [Fact]
    public void Failure_WithMultipleErrors_CreatesFailureResult()
    {
        // Arrange
        var errors = new[]
        {
            DomainError.Create("TEST001", "First error"),
            DomainError.Create("TEST002", "Second error")
        };

        // Act
        var result = Result<string>.Failure(errors[0], errors[1]);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void Value_OnFailureResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<string>.Failure(DomainError.Create("TEST001", "Test error"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Match_WithSuccessResult_ExecutesSuccessFunction()
    {
        // Arrange
        var result = Result<string>.Success("test-value");

        // Act
        var matchResult = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: _ => "Failure");

        // Assert
        Assert.Equal("Success: test-value", matchResult);
    }

    [Fact]
    public void Match_WithFailureResult_ExecutesFailureFunction()
    {
        // Arrange
        var result = Result<string>.Failure(DomainError.Create("TEST001", "Test error"));

        // Act
        var matchResult = result.Match(
            onSuccess: _ => "Success",
            onFailure: errors => $"Failure: {errors.Count} errors");

        // Assert
        Assert.Equal("Failure: 1 errors", matchResult);
    }

    [Fact]
    public void Map_WithSuccessResult_MapsValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var mapped = result.Map(value => $"Value: {value}");

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal("Value: 42", mapped.Value);
    }

    [Fact]
    public void Map_WithFailureResult_PreservesErrors()
    {
        // Arrange
        var result = Result<int>.Failure(DomainError.Create("TEST001", "Test error"));

        // Act
        var mapped = result.Map(value => $"Value: {value}");

        // Assert
        Assert.True(mapped.IsFailure);
        Assert.Single(mapped.Errors);
        Assert.Equal("TEST001", mapped.Errors[0].Code);
    }

    [Fact]
    public void Bind_WithSuccessResult_ExecutesBindFunction()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var bound = result.Bind(value => Result<string>.Success($"Value: {value}"));

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal("Value: 42", bound.Value);
    }

    [Fact]
    public void Bind_WithFailureResult_PreservesErrors()
    {
        // Arrange
        var result = Result<int>.Failure(DomainError.Create("TEST001", "Test error"));

        // Act
        var bound = result.Bind(value => Result<string>.Success($"Value: {value}"));

        // Assert
        Assert.True(bound.IsFailure);
        Assert.Single(bound.Errors);
        Assert.Equal("TEST001", bound.Errors[0].Code);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        // Arrange
        var value = "test-value";

        // Act
        Result<string> result = value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailureResult()
    {
        // Arrange
        var error = DomainError.Create("TEST001", "Test error");

        // Act
        Result<string> result = error;

        // Assert
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
        Assert.Equal(error.Code, result.Errors[0].Code);
    }
}
