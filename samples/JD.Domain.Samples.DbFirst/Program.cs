using JD.Domain.Abstractions;
using JD.Domain.Rules;
using JD.Domain.Runtime;
using JD.Domain.Snapshot;

namespace JD.Domain.Samples.DbFirst;

/// <summary>
/// Demonstrates database-first workflow: existing EF entities + JD rules as partials.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== JD.Domain Database-First Sample ===\n");

        // Step 1: Simulate loading manifest from scaffolded EF entities
        Console.WriteLine("1. Loading manifest from existing EF entities...");
        var manifest = CreateManifestFromScaffoldedEntities();
        Console.WriteLine($"   Loaded: {manifest.Name} v{manifest.Version}");
        Console.WriteLine($"   Entities: {manifest.Entities.Count}");

        // Step 2: Add JD rules as partial classes (rules defined separately)
        Console.WriteLine("\n2. Defining rules for existing entities...");
        var blogRules = new RuleSetBuilder<Blog>("Default")
            .Invariant("Blog.Url.Required", b => !string.IsNullOrWhiteSpace(b.Url))
            .WithMessage("Blog must have a valid URL")
            .Invariant("Blog.Url.Protocol", b => b.Url.StartsWith("http"))
            .WithMessage("Blog URL must start with http:// or https://")
            .Build();

        var postRules = new RuleSetBuilder<Post>("Default")
            .Invariant("Post.Title.Required", p => !string.IsNullOrWhiteSpace(p.Title))
            .WithMessage("Post must have a title")
            .Invariant("Post.Title.MaxLength", p => p.Title.Length <= 200)
            .WithMessage("Post title cannot exceed 200 characters")
            .Build();

        Console.WriteLine($"   Blog rules: {blogRules.Rules.Count}");
        Console.WriteLine($"   Post rules: {postRules.Rules.Count}");

        // Step 3: Create snapshot for versioning
        Console.WriteLine("\n3. Creating snapshot for version control...");
        var writer = new SnapshotWriter();
        var snapshot = writer.CreateSnapshot(manifest);
        Console.WriteLine($"   Snapshot hash: {snapshot.Hash}");
        Console.WriteLine($"   Created at: {snapshot.CreatedAt:O}");

        // Step 4: Validate entities at runtime
        Console.WriteLine("\n4. Validating sample entities...");
        var engine = DomainRuntime.CreateEngine(manifest);

        // Valid blog
        var validBlog = new Blog { BlogId = 1, Url = "https://example.com/blog" };
        var blogResult = engine.Evaluate(validBlog, blogRules);
        Console.WriteLine($"   Valid blog: {(blogResult.IsValid ? "PASSED" : "FAILED")}");

        // Invalid blog
        var invalidBlog = new Blog { BlogId = 2, Url = "" };
        var invalidBlogResult = engine.Evaluate(invalidBlog, blogRules);
        Console.WriteLine($"   Invalid blog: {(invalidBlogResult.IsValid ? "PASSED" : "FAILED")}");
        foreach (var error in invalidBlogResult.Errors)
        {
            Console.WriteLine($"      - {error.Message}");
        }

        Console.WriteLine("\n=== Sample Complete ===");
    }

    /// <summary>
    /// Simulates creating a manifest from EF Core scaffolded entities.
    /// </summary>
    private static DomainManifest CreateManifestFromScaffoldedEntities()
    {
        return new DomainManifest
        {
            Name = "BloggingDb",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "Blog",
                    TypeName = "JD.Domain.Samples.DbFirst.Blog",
                    TableName = "Blogs",
                    Properties =
                    [
                        new PropertyManifest { Name = "BlogId", TypeName = "System.Int32", IsRequired = true },
                        new PropertyManifest { Name = "Url", TypeName = "System.String", IsRequired = true, MaxLength = 500 }
                    ],
                    KeyProperties = ["BlogId"]
                },
                new EntityManifest
                {
                    Name = "Post",
                    TypeName = "JD.Domain.Samples.DbFirst.Post",
                    TableName = "Posts",
                    Properties =
                    [
                        new PropertyManifest { Name = "PostId", TypeName = "System.Int32", IsRequired = true },
                        new PropertyManifest { Name = "Title", TypeName = "System.String", IsRequired = true, MaxLength = 200 },
                        new PropertyManifest { Name = "Content", TypeName = "System.String", IsRequired = false },
                        new PropertyManifest { Name = "BlogId", TypeName = "System.Int32", IsRequired = true }
                    ],
                    KeyProperties = ["PostId"]
                }
            ]
        };
    }
}

// These represent EF Core scaffolded entities
public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; } = "";
    public List<Post> Posts { get; set; } = new();
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; } = "";
    public string? Content { get; set; }
    public int BlogId { get; set; }
    public Blog Blog { get; set; } = null!;
}
