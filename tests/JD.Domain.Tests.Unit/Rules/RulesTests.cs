using JD.Domain.Abstractions;
using JD.Domain.Modeling;
using JD.Domain.Rules;

namespace JD.Domain.Tests.Unit.Rules;

/// <summary>
/// Tests for the Rules DSL.
/// </summary>
public sealed class RulesTests
{
    private class Blog
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PostCount { get; set; }
    }

    [Fact]
    public void Rules_WithInvariant_AddsRuleToManifest()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name)))
            .BuildManifest();

        // Assert
        Assert.Single(manifest.RuleSets);
        var ruleSet = manifest.RuleSets[0];
        Assert.Equal("Default", ruleSet.Name);
        Assert.Single(ruleSet.Rules);
        Assert.Equal("NameRequired", ruleSet.Rules[0].Id);
        Assert.Equal("Invariant", ruleSet.Rules[0].Category);
    }

    [Fact]
    public void Rules_WithNamedRuleSet_CreatesNamedRuleSet()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>("Create", r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name)))
            .BuildManifest();

        // Assert
        Assert.Single(manifest.RuleSets);
        Assert.Equal("Create", manifest.RuleSets[0].Name);
    }

    [Fact]
    public void Rules_WithMultipleInvariants_AddsAllRules()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>(r =>
            {
                r.Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name));
                r.Invariant("NameMaxLength", b => b.Name.Length <= 200);
            })
            .BuildManifest();

        // Assert
        var ruleSet = manifest.RuleSets[0];
        Assert.Equal(2, ruleSet.Rules.Count);
        Assert.Contains(ruleSet.Rules, rule => rule.Id == "NameRequired");
        Assert.Contains(ruleSet.Rules, rule => rule.Id == "NameMaxLength");
    }

    [Fact]
    public void Rules_WithValidator_AddsValidatorRule()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .Validator("ValidPostCount", b => b.PostCount >= 0))
            .BuildManifest();

        // Assert
        var rule = manifest.RuleSets[0].Rules[0];
        Assert.Equal("Validator", rule.Category);
        Assert.Equal("ValidPostCount", rule.Id);
    }

    [Fact]
    public void Rules_WithMessage_SetsMessage()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
                .WithMessage("Blog name is required."))
            .BuildManifest();

        // Assert
        var rule = manifest.RuleSets[0].Rules[0];
        Assert.Equal("Blog name is required.", rule.Message);
    }

    [Fact]
    public void Rules_WithSeverity_SetsSeverity()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
                .WithSeverity(RuleSeverity.Critical))
            .BuildManifest();

        // Assert
        var rule = manifest.RuleSets[0].Rules[0];
        Assert.Equal(RuleSeverity.Critical, rule.Severity);
    }

    [Fact]
    public void Rules_WithTag_AddsTag()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name))
                .WithTag("DataQuality"))
            .BuildManifest();

        // Assert
        var rule = manifest.RuleSets[0].Rules[0];
        Assert.Contains("DataQuality", rule.Tags);
    }

    [Fact]
    public void Rules_WithInclude_AddsInclude()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>("Create", r => r
                .Include("Default")
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name)))
            .BuildManifest();

        // Assert
        var ruleSet = manifest.RuleSets[0];
        Assert.Contains("Default", ruleSet.Includes);
    }

    [Fact]
    public void Rules_WithMultipleRuleSets_AddsAllRuleSets()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>("Create", r => r
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name)))
            .Rules<Blog>("Update", r => r
                .Invariant("NameMaxLength", b => b.Name.Length <= 200))
            .BuildManifest();

        // Assert
        Assert.Equal(2, manifest.RuleSets.Count);
        Assert.Contains(manifest.RuleSets, rs => rs.Name == "Create");
        Assert.Contains(manifest.RuleSets, rs => rs.Name == "Update");
    }

    [Fact]
    public void Rules_WithMetadata_AddsMetadata()
    {
        // Arrange & Act
        var manifest = JD.Domain.Modeling.Domain.Create("BlogDomain")
            .Entity<Blog>()
            .Rules<Blog>(r => r
                .WithMetadata("Author", "Test")
                .Invariant("NameRequired", b => !string.IsNullOrWhiteSpace(b.Name)))
            .BuildManifest();

        // Assert
        var ruleSet = manifest.RuleSets[0];
        Assert.True(ruleSet.Metadata.ContainsKey("Author"));
        Assert.Equal("Test", ruleSet.Metadata["Author"]);
    }
}
