using JD.Domain.AspNetCore;
using Microsoft.AspNetCore.Http;

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
