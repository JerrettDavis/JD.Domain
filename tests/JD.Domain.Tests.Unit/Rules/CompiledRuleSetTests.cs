using JD.Domain.Abstractions;
using JD.Domain.Rules;

namespace JD.Domain.Tests.Unit.Rules;

public sealed class CompiledRuleSetTests
{
    private sealed class TestEntity
    {
        public int Count { get; set; }
    }

    [Fact]
    public void Constructor_ThrowsOnInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => new CompiledRuleSet<TestEntity>(" ", Array.Empty<CompiledRule<TestEntity>>()));
        Assert.Throws<ArgumentNullException>(() => new CompiledRuleSet<TestEntity>("Default", null!));
    }

    [Fact]
    public void Evaluate_ThrowsOnNullInstance()
    {
        var ruleSet = new CompiledRuleSet<TestEntity>("Default", Array.Empty<CompiledRule<TestEntity>>());

        Assert.Throws<ArgumentNullException>(() => ruleSet.Evaluate(null!));
    }

    [Fact]
    public void Evaluate_IncludesWarningsAndInfo()
    {
        var builder = new RuleSetBuilder<TestEntity>("Default");
        builder.Invariant("InfoRule", _ => false).WithSeverity(RuleSeverity.Info);
        builder.Invariant("WarnRule", _ => false).WithSeverity(RuleSeverity.Warning);

        var compiled = builder.BuildCompiled();
        var result = compiled.Evaluate(new TestEntity(), new RuleEvaluationOptions
        {
            IncludeInfo = true
        });

        Assert.True(result.IsValid);
        Assert.Single(result.Info);
        Assert.Single(result.Warnings);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Evaluate_StopsOnFirstError()
    {
        var builder = new RuleSetBuilder<TestEntity>("Default");
        builder.Invariant("ErrorRule1", _ => false).WithSeverity(RuleSeverity.Error);
        builder.Invariant("ErrorRule2", _ => false).WithSeverity(RuleSeverity.Error);

        var compiled = builder.BuildCompiled();
        var result = compiled.Evaluate(new TestEntity(), new RuleEvaluationOptions
        {
            StopOnFirstError = true
        });

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(1, result.RulesEvaluated);
    }

    [Fact]
    public void Evaluate_StopsOnFirstError_WhenExceptionThrown()
    {
        var builder = new RuleSetBuilder<TestEntity>("Default");
        builder.Invariant("ThrowRule", x => Throws(x));
        builder.Invariant("ErrorRule", _ => false).WithSeverity(RuleSeverity.Error);

        var compiled = builder.BuildCompiled();
        var result = compiled.Evaluate(new TestEntity(), new RuleEvaluationOptions
        {
            StopOnFirstError = true
        });

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(1, result.RulesEvaluated);
    }

    [Fact]
    public void TargetType_ReturnsTypeName()
    {
        var ruleSet = new CompiledRuleSet<TestEntity>("Default", Array.Empty<CompiledRule<TestEntity>>());

        Assert.Contains(nameof(TestEntity), ruleSet.TargetType);
    }

    [Fact]
    public void Evaluate_HandlesRuleExceptions()
    {
        var builder = new RuleSetBuilder<TestEntity>("Default");
        builder.Invariant("ThrowRule", x => Throws(x)).WithSeverity(RuleSeverity.Error);

        var compiled = builder.BuildCompiled();
        var result = compiled.Evaluate(new TestEntity());

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Rule evaluation failed", result.Errors[0].Message);
    }

    private static bool Throws(TestEntity _)
    {
        throw new InvalidOperationException("boom");
    }
}
