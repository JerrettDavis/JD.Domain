using JD.Domain.Abstractions;
using JD.Domain.Rules;
using JD.Domain.Runtime;

namespace JD.Domain.Tests.Unit.Runtime;

/// <summary>
/// Tests for the Runtime evaluation engine.
/// </summary>
public sealed class DomainEngineTests
{
    private class Blog
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PostCount { get; set; }
    }

    [Fact]
    public void CreateEngine_WithValidManifest_CreatesEngine()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .BuildManifest();

        // Act
        var engine = DomainRuntime.CreateEngine(manifest);

        // Assert
        Assert.NotNull(engine);
    }

    [Fact]
    public void Create_WithOptions_CreatesEngine()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .BuildManifest();

        // Act
        var engine = DomainRuntime.Create(options => options.AddManifest(manifest));

        // Assert
        Assert.NotNull(engine);
    }

    [Fact]
    public void Evaluate_WithNoRules_ReturnsSuccess()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .BuildManifest();

        var engine = DomainRuntime.CreateEngine(manifest);
        var blog = new Blog { Name = "Test Blog" };

        // Act
        var result = engine.Evaluate(blog);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Evaluate_WithRulesHavingMessages_IncludesErrors()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
                    .WithMessage("Name is required"))
            .BuildManifest();

        var engine = DomainRuntime.CreateEngine(manifest);
        var blog = new Blog { Name = "" };

        // Act
        var result = engine.Evaluate(blog);

        // Assert
        // Note: Current implementation creates errors for rules with messages
        // In a full implementation, this would evaluate the expression
        Assert.Single(result.Errors);
        Assert.Equal("NameRequired", result.Errors[0].Code);
        Assert.Equal("Name is required", result.Errors[0].Message);
    }

    [Fact]
    public async Task EvaluateAsync_WithValidBlog_ReturnsResult()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
                    .WithMessage("Name is required"))
            .BuildManifest();

        var engine = DomainRuntime.CreateEngine(manifest);
        var blog = new Blog { Name = "Test" };

        // Act
        var result = await engine.EvaluateAsync(blog);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.RulesEvaluated);
    }

    [Fact]
    public void Evaluate_WithNamedRuleSet_EvaluatesOnlyThatRuleSet()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .Rules<Blog>("Create", r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
                    .WithMessage("Name is required"))
            .Rules<Blog>("Update", r => r
                .Validator("PostCountValid", b => b.PostCount >= 0)
                    .WithMessage("Post count must be non-negative"))
            .BuildManifest();

        var engine = DomainRuntime.CreateEngine(manifest);
        var blog = new Blog();

        // Act
        var result = engine.Evaluate(blog, new RuleEvaluationOptions
        {
            RuleSet = "Create"
        });

        // Assert
        Assert.Contains("Create", result.RuleSetsEvaluated);
        Assert.DoesNotContain("Update", result.RuleSetsEvaluated);
    }

    [Fact]
    public void Evaluate_WithWarnings_IncludesWarnings()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .Invariant("NameLength", b => b.Name.Length <= 100)
                    .WithMessage("Name is quite long")
                    .WithSeverity(RuleSeverity.Warning))
            .BuildManifest();

        var engine = DomainRuntime.CreateEngine(manifest);
        var blog = new Blog { Name = "Test" };

        // Act
        var result = engine.Evaluate(blog);

        // Assert
        Assert.Single(result.Warnings);
        Assert.True(result.IsValid); // Warnings don't make it invalid
    }

    [Fact]
    public void Evaluate_WithMultipleRuleSets_EvaluatesAll()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .Rules<Blog>("Create", r => r
                .Invariant("Rule1", b => true)
                    .WithMessage("Message 1"))
            .Rules<Blog>("Update", r => r
                .Invariant("Rule2", b => true)
                    .WithMessage("Message 2"))
            .BuildManifest();

        var engine = DomainRuntime.CreateEngine(manifest);
        var blog = new Blog();

        // Act
        var result = engine.Evaluate(blog);

        // Assert
        Assert.Equal(2, result.RulesEvaluated);
        Assert.Equal(2, result.RuleSetsEvaluated.Count);
    }

    #region CompiledRuleSet Tests - Actual Expression Evaluation

    [Fact]
    public void CompiledRuleSet_WhenRulePasses_ReturnsValid()
    {
        // Arrange
        var ruleSet = new RuleSetBuilder<Blog>("Default")
            .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
            .WithMessage("Name is required")
            .BuildCompiled();

        var blog = new Blog { Name = "Valid Name" };

        // Act
        var result = ruleSet.Evaluate(blog);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Equal(1, result.RulesEvaluated);
    }

    [Fact]
    public void CompiledRuleSet_WhenRuleFails_ReturnsInvalid()
    {
        // Arrange
        var ruleSet = new RuleSetBuilder<Blog>("Default")
            .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
            .WithMessage("Name is required")
            .BuildCompiled();

        var blog = new Blog { Name = "" };

        // Act
        var result = ruleSet.Evaluate(blog);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("NameRequired", result.Errors[0].Code);
        Assert.Equal("Name is required", result.Errors[0].Message);
    }

    [Fact]
    public void CompiledRuleSet_WithMultipleRules_EvaluatesAll()
    {
        // Arrange
        var ruleSet = new RuleSetBuilder<Blog>("Default")
            .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
            .WithMessage("Name is required")
            .Invariant("NameLength", b => b.Name.Length <= 100)
            .WithMessage("Name too long")
            .Invariant("PostCountValid", b => b.PostCount >= 0)
            .WithMessage("Post count must be non-negative")
            .BuildCompiled();

        var blog = new Blog { Name = "Valid", PostCount = 5 };

        // Act
        var result = ruleSet.Evaluate(blog);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(3, result.RulesEvaluated);
    }

    [Fact]
    public void CompiledRuleSet_WithMultipleFailures_ReturnsAllErrors()
    {
        // Arrange
        var ruleSet = new RuleSetBuilder<Blog>("Default")
            .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
            .WithMessage("Name is required")
            .Invariant("PostCountValid", b => b.PostCount >= 0)
            .WithMessage("Post count must be non-negative")
            .BuildCompiled();

        var blog = new Blog { Name = "", PostCount = -5 };

        // Act
        var result = ruleSet.Evaluate(blog);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void CompiledRuleSet_WithStopOnFirstError_StopsAfterFirstError()
    {
        // Arrange
        var ruleSet = new RuleSetBuilder<Blog>("Default")
            .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
            .WithMessage("Name is required")
            .Invariant("PostCountValid", b => b.PostCount >= 0)
            .WithMessage("Post count must be non-negative")
            .BuildCompiled();

        var blog = new Blog { Name = "", PostCount = -5 };
        var options = new RuleEvaluationOptions { StopOnFirstError = true };

        // Act
        var result = ruleSet.Evaluate(blog, options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("NameRequired", result.Errors[0].Code);
    }

    [Fact]
    public void CompiledRuleSet_WithWarning_IncludesWarningButStillValid()
    {
        // Arrange
        var ruleSet = new RuleSetBuilder<Blog>("Default")
            .Invariant("NameLength", b => b.Name.Length <= 10)
            .WithMessage("Name is quite long")
            .WithSeverity(RuleSeverity.Warning)
            .BuildCompiled();

        var blog = new Blog { Name = "This is a very long name that exceeds the limit" };

        // Act
        var result = ruleSet.Evaluate(blog);

        // Assert
        Assert.True(result.IsValid); // Warnings don't make it invalid
        Assert.Single(result.Warnings);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DomainEngine_EvaluateWithCompiledRuleSet_ActuallyEvaluatesPredicates()
    {
        // Arrange
        var manifest = JD.Domain.Modeling.Domain.Create("TestDomain")
            .Entity<Blog>()
            .BuildManifest();

        var engine = (DomainEngine)DomainRuntime.CreateEngine(manifest);
        var ruleSet = new RuleSetBuilder<Blog>("Default")
            .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
            .WithMessage("Name is required")
            .BuildCompiled();

        var invalidBlog = new Blog { Name = "" };
        var validBlog = new Blog { Name = "Valid" };

        // Act
        var invalidResult = engine.Evaluate(invalidBlog, ruleSet);
        var validResult = engine.Evaluate(validBlog, ruleSet);

        // Assert
        Assert.False(invalidResult.IsValid);
        Assert.Single(invalidResult.Errors);

        Assert.True(validResult.IsValid);
        Assert.Empty(validResult.Errors);
    }

    #endregion
}
