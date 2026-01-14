using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;
using JD.Domain.Rules;
using JD.Domain.Snapshot;

namespace JD.Domain.Samples.DbFirst;

/// <summary>
/// Demonstrates database-first workflow: existing EF entities + JD rules as partials.
/// Manifests are now automatically generated from entity attributes - NO manual string writing!
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== JD.Domain Database-First Sample (Automatic Manifest Generation) ===\n");

        // Step 1: Access auto-generated manifest from scaffolded EF entities
        Console.WriteLine("1. Using auto-generated manifest from EF entities...");
        var manifest = BloggingDbManifest.GeneratedManifest;
        Console.WriteLine($"   Domain: {manifest.Name} v{manifest.Version}");
        Console.WriteLine($"   Entities: {manifest.Entities.Count}");
        Console.WriteLine($"   Source: {(manifest.Sources.Count > 0 ? manifest.Sources[0].Type : "Unknown")}");
        Console.WriteLine($"   NO MANUAL STRING WRITING REQUIRED!");

        // Step 2: Add JD rules as partial classes (rules defined separately)
        Console.WriteLine("\n2. Defining rules for existing entities...");
        var blogRules = new RuleSetBuilder<Blog>("Default")
            .Invariant("Blog.Url.Required", b => !string.IsNullOrWhiteSpace(b.Url))
            .WithMessage("Blog must have a valid URL")
            .Invariant("Blog.Url.Protocol", b => b.Url.StartsWith("http"))
            .WithMessage("Blog URL must start with http:// or https://")
            .BuildCompiled();

        var postRules = new RuleSetBuilder<Post>("Default")
            .Invariant("Post.Title.Required", p => !string.IsNullOrWhiteSpace(p.Title))
            .WithMessage("Post must have a title")
            .Invariant("Post.Title.MaxLength", p => p.Title.Length <= 200)
            .WithMessage("Post title cannot exceed 200 characters")
            .BuildCompiled();

        Console.WriteLine($"   Blog rules: {blogRules.Rules.Count}");
        Console.WriteLine($"   Post rules: {postRules.Rules.Count}");

        // Step 3: Create snapshot for versioning
        Console.WriteLine("\n3. Creating snapshot for version control...");
        var writer = new SnapshotWriter();
        var snapshot = writer.CreateSnapshot(manifest);
        Console.WriteLine($"   Snapshot hash: {snapshot.Hash}");
        Console.WriteLine($"   Created at: {snapshot.CreatedAt:O}");

        // Step 4: Validate entities using compiled rules
        Console.WriteLine("\n4. Validating sample entities...");

        // Valid blog
        var validBlog = new Blog { BlogId = 1, Url = "https://example.com/blog" };
        var blogResult = blogRules.Evaluate(validBlog);
        Console.WriteLine($"   Valid blog: {(blogResult.IsValid ? "PASSED" : "FAILED")}");

        // Invalid blog (empty URL triggers rule)
        var invalidBlog = new Blog { BlogId = 2, Url = "" };
        var invalidBlogResult = blogRules.Evaluate(invalidBlog);
        Console.WriteLine($"   Invalid blog: {(invalidBlogResult.IsValid ? "PASSED" : "FAILED")}");
        foreach (var error in invalidBlogResult.Errors)
        {
            Console.WriteLine($"      - {error.Message}");
        }

        Console.WriteLine("\n=== Sample Complete ===");
        Console.WriteLine("Manifest was generated automatically from entity attributes!");
    }
}

// These represent EF Core scaffolded entities with [DomainEntity] attributes added
// This demonstrates how you can add JD.Domain attributes to existing scaffolded entities
[DomainEntity(TableName = "Blogs")]
public class Blog
{
    [Key]
    public int BlogId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = "";

    [ExcludeFromManifest]
    public List<Post> Posts { get; set; } = new();
}

[DomainEntity(TableName = "Posts")]
public class Post
{
    [Key]
    public int PostId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    public string? Content { get; set; }

    [Required]
    public int BlogId { get; set; }

    [ExcludeFromManifest]
    public Blog Blog { get; set; } = null!;
}
