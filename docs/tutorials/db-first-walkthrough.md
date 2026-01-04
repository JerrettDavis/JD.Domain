# Database-First Walkthrough

In this tutorial, you'll learn how to add JD.Domain business rules and validation to existing EF Core scaffolded entities without modifying the generated code. This approach is perfect for legacy databases, existing projects, or teams that prefer database-driven development.

**Time:** 30-45 minutes | **Level:** Beginner

## What You'll Build

By the end of this tutorial, you'll have:

- ✅ Scaffolded EF Core entities from an existing database
- ✅ Domain manifest describing existing entities
- ✅ Business rules attached to scaffolded entities
- ✅ FluentValidation validators (auto-generated)
- ✅ ASP.NET Core API with automatic validation

## Prerequisites

- .NET 10.0 SDK or later
- Basic understanding of C# and Entity Framework Core
- SQL Server LocalDB or another database
- An existing database (or we'll create one for this tutorial)

## Step 1: Create the Database

For this tutorial, we'll create a simple blogging database.

Create a SQL script `setup.sql`:

```sql
CREATE DATABASE BloggingDb;
GO

USE BloggingDb;
GO

CREATE TABLE Blogs (
    BlogId INT PRIMARY KEY IDENTITY(1,1),
    Url NVARCHAR(500) NOT NULL,
    Rating INT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Posts (
    PostId INT PRIMARY KEY IDENTITY(1,1),
    BlogId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX) NULL,
    PublishedDate DATETIME2 NULL,
    CONSTRAINT FK_Posts_Blogs FOREIGN KEY (BlogId) REFERENCES Blogs(BlogId)
);

CREATE UNIQUE INDEX IX_Blogs_Url ON Blogs(Url);
CREATE INDEX IX_Posts_BlogId ON Posts(BlogId);
GO

-- Insert sample data
INSERT INTO Blogs (Url, Rating) VALUES
    ('https://devblogs.microsoft.com', 5),
    ('https://blog.cleancoder.com', 5),
    ('https://martinfowler.com', 5);

INSERT INTO Posts (BlogId, Title, Content, PublishedDate) VALUES
    (1, 'Announcing .NET 10', 'Today we are excited to announce...', GETUTCDATE()),
    (1, 'EF Core 10 Released', 'Entity Framework Core 10 is now...', GETUTCDATE()),
    (2, 'Clean Code Principles', 'Writing clean code is essential...', GETUTCDATE());
GO
```

Execute the script:

```bash
sqlcmd -S "(localdb)\mssqllocaldb" -i setup.sql
```

## Step 2: Create the Project

Create a new web API project:

```bash
mkdir JD.Domain.Tutorial.DbFirst
cd JD.Domain.Tutorial.DbFirst
dotnet new webapi
```

## Step 3: Install Required Packages

Install EF Core and JD.Domain packages:

```bash
# EF Core scaffolding
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design

# JD.Domain packages
dotnet add package JD.Domain.Abstractions
dotnet add package JD.Domain.ManifestGeneration
dotnet add package JD.Domain.ManifestGeneration.Generator
dotnet add package JD.Domain.Rules
dotnet add package JD.Domain.Runtime
dotnet add package JD.Domain.AspNetCore
dotnet add package JD.Domain.FluentValidation.Generator
dotnet add package FluentValidation.AspNetCore
```

## Step 4: Scaffold Entities from Database

Scaffold the database into EF Core entities:

```bash
dotnet ef dbcontext scaffold "Server=(localdb)\mssqllocaldb;Database=BloggingDb;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer -o Data/Entities --context-dir Data --context BloggingDbContext --force
```

This generates:

**Data/Entities/Blog.cs:**
```csharp
public partial class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; } = null!;
    public int? Rating { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
```

**Data/Entities/Post.cs:**
```csharp
public partial class Post
{
    public int PostId { get; set; }
    public int BlogId { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public DateTime? PublishedDate { get; set; }
    public virtual Blog Blog { get; set; } = null!;
}
```

### Using Partial Classes for Annotations

The scaffolded entities are marked `partial`, which allows us to extend them in separate files without modifying the generated code. We'll use this to add JD.Domain attributes and data annotations.

## Step 5: Add Domain Annotations (Automatic Manifest Generation)

Instead of manually writing manifests with strings, we'll use **automatic manifest generation** via source generators. This respects the scaffolded entities as the source of truth and eliminates manual string writing.

### 5a. Configure Assembly-Level Manifest

Create `Properties/AssemblyInfo.cs`:

```csharp
using JD.Domain.ManifestGeneration;

[assembly: GenerateManifest("Blogging", Version = "1.0.0")]
```

### 5b. Extend Scaffolded Entities with Attributes

Since scaffolded entities are `partial`, we can add attributes in separate files without touching the generated code.

Create `Data/Entities/Blog.Annotations.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;

namespace JD.Domain.Tutorial.DbFirst.Data.Entities;

[DomainEntity(TableName = "Blogs")]
public partial class Blog
{
    // Properties are automatically discovered from the main partial class
    // We just need to add data annotations for metadata extraction
}

// Extension class for adding data annotations without modifying scaffolded code
public static class BlogAnnotations
{
    // Metadata will be extracted from the actual Blog class properties
    // Data annotations on the scaffolded class will be auto-detected
}
```

Create `Data/Entities/Post.Annotations.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using JD.Domain.ManifestGeneration;

namespace JD.Domain.Tutorial.DbFirst.Data.Entities;

[DomainEntity(TableName = "Posts")]
public partial class Post
{
    // Properties are automatically discovered from the main partial class
}
```

### Explanation

**NO MANUAL STRING WRITING REQUIRED!** The manifest source generator will:

- Automatically discover all properties from the scaffolded entities
- Extract property types, names, and nullability from the actual code
- Read `[Key]` attributes from EF scaffolding
- Detect required vs optional based on nullable reference types
- Infer `MaxLength` from string property configurations if present

The scaffolded entities remain **completely unchanged**, while JD.Domain attributes live in separate partial class files.

## Step 6: Define Business Rules

Now add business rules to the scaffolded entities.

Create `Domain/BlogRules.cs`:

```csharp
using JD.Domain.Rules;
using JD.Domain.Tutorial.DbFirst.Data.Entities;

namespace JD.Domain.Tutorial.DbFirst.Domain;

public static class BlogRules
{
    public static RuleSetManifest Default()
    {
        return new RuleSetBuilder<Blog>("Default")
            // URL validation
            .Invariant("Url.Required", b => !string.IsNullOrWhiteSpace(b.Url))
            .WithMessage("Blog URL is required")

            .Invariant("Url.ValidProtocol", b => b.Url.StartsWith("http://") || b.Url.StartsWith("https://"))
            .WithMessage("Blog URL must start with http:// or https://")

            .Invariant("Url.MaxLength", b => b.Url.Length <= 500)
            .WithMessage("Blog URL cannot exceed 500 characters")

            // Rating validation
            .Invariant("Rating.Range", b => !b.Rating.HasValue || (b.Rating.Value >= 1 && b.Rating.Value <= 5))
            .WithMessage("Blog rating must be between 1 and 5")

            // Created date validation
            .Invariant("CreatedDate.NotFuture", b => b.CreatedDate <= DateTime.UtcNow)
            .WithMessage("Blog creation date cannot be in the future")

            .Build();
    }
}
```

Create `Domain/PostRules.cs`:

```csharp
using JD.Domain.Rules;
using JD.Domain.Tutorial.DbFirst.Data.Entities;

namespace JD.Domain.Tutorial.DbFirst.Domain;

public static class PostRules
{
    public static RuleSetManifest Default()
    {
        return new RuleSetBuilder<Post>("Default")
            // Title validation
            .Invariant("Title.Required", p => !string.IsNullOrWhiteSpace(p.Title))
            .WithMessage("Post title is required")

            .Invariant("Title.MinLength", p => p.Title.Length >= 5)
            .WithMessage("Post title must be at least 5 characters")

            .Invariant("Title.MaxLength", p => p.Title.Length <= 200)
            .WithMessage("Post title cannot exceed 200 characters")

            // Content validation (if provided)
            .Invariant("Content.MinLength", p => string.IsNullOrEmpty(p.Content) || p.Content.Length >= 10)
            .WithMessage("Post content must be at least 10 characters if provided")

            // Blog ID validation
            .Invariant("BlogId.Positive", p => p.BlogId > 0)
            .WithMessage("Post must be associated with a valid blog")

            // Published date validation
            .Invariant("PublishedDate.NotFuture", p => !p.PublishedDate.HasValue || p.PublishedDate.Value <= DateTime.UtcNow)
            .WithMessage("Post published date cannot be in the future")

            .Build();
    }
}
```

### Explanation

Rules are defined **externally** from the scaffolded entities. The entities remain unchanged, but we can still enforce business rules at runtime or during API requests.

## Step 7: Configure ASP.NET Core Validation

Update `Program.cs` to add domain validation:

```csharp
using FluentValidation;
using JD.Domain.AspNetCore;
using JD.Domain.Tutorial.DbFirst.Data;
using JD.Domain.Tutorial.DbFirst.Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core
builder.Services.AddDbContext<BloggingDbContext>(options =>
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BloggingDb;Trusted_Connection=True;TrustServerCertificate=True"));

// Add domain validation with auto-generated manifest
builder.Services.AddDomainValidation(options =>
{
    // Use the auto-generated manifest (BloggingManifest class is created by the source generator)
    options.AddManifest(BloggingManifest.GeneratedManifest);
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use domain validation middleware
app.UseDomainValidation();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Step 8: Create API Endpoints

Create `Controllers/BlogsController.cs`:

```csharp
using JD.Domain.AspNetCore;
using JD.Domain.Runtime;
using JD.Domain.Tutorial.DbFirst.Data;
using JD.Domain.Tutorial.DbFirst.Data.Entities;
using JD.Domain.Tutorial.DbFirst.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JD.Domain.Tutorial.DbFirst.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly BloggingDbContext _context;
    private readonly IDomainEngine _engine;

    public BlogsController(BloggingDbContext context, IDomainEngine engine)
    {
        _context = context;
        _engine = engine;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Blog>>> GetBlogs()
    {
        return await _context.Blogs.Include(b => b.Posts).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Blog>> GetBlog(int id)
    {
        var blog = await _context.Blogs
            .Include(b => b.Posts)
            .FirstOrDefaultAsync(b => b.BlogId == id);

        if (blog == null)
            return NotFound();

        return blog;
    }

    [HttpPost]
    [DomainValidation] // Automatic validation using MVC filter
    public async Task<ActionResult<Blog>> CreateBlog(Blog blog)
    {
        // Validation happens automatically via [DomainValidation] attribute
        // If validation fails, middleware returns 400 Bad Request with ProblemDetails

        _context.Blogs.Add(blog);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBlog), new { id = blog.BlogId }, blog);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBlog(int id, Blog blog)
    {
        if (id != blog.BlogId)
            return BadRequest();

        // Manual validation for demonstration
        var validationResult = _engine.Evaluate(blog, BlogRules.Default());
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.Message));
        }

        _context.Entry(blog).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Blogs.AnyAsync(b => b.BlogId == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBlog(int id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null)
            return NotFound();

        _context.Blogs.Remove(blog);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
```

### Explanation

- **`[DomainValidation]`** attribute automatically validates requests
- Manual validation using `_engine.Evaluate()` is also possible
- Validation errors return RFC 9457 ProblemDetails responses

## Step 9: Test the API

Run the application:

```bash
dotnet run
```

Open Swagger UI at `https://localhost:5001/swagger` and test:

### Test 1: Create Valid Blog

POST to `/api/blogs`:
```json
{
  "url": "https://newblog.com",
  "rating": 4
}
```

**Expected:** `201 Created` with the new blog

### Test 2: Create Invalid Blog (Bad URL)

POST to `/api/blogs`:
```json
{
  "url": "not-a-url",
  "rating": 4
}
```

**Expected:** `400 Bad Request` with validation errors:
```json
{
  "type": "https://tools.ietf.org/html/rfc9457",
  "title": "Validation Failed",
  "status": 400,
  "errors": [
    {
      "property": "Url",
      "message": "Blog URL must start with http:// or https://"
    }
  ]
}
```

### Test 3: Create Invalid Blog (Rating out of range)

POST to `/api/blogs`:
```json
{
  "url": "https://blog.com",
  "rating": 10
}
```

**Expected:** `400 Bad Request` with:
```json
{
  "errors": [
    {
      "property": "Rating",
      "message": "Blog rating must be between 1 and 5"
    }
  ]
}
```

## Step 10: Explore Generated Validators

The `JD.Domain.FluentValidation.Generator` automatically generated FluentValidation validators.

Check `obj/` for `BlogValidator.g.cs` and `PostValidator.g.cs`.

You can use these validators in your API:

```csharp
using FluentValidation;

public class CreateBlogRequest
{
    public string Url { get; set; } = string.Empty;
    public int? Rating { get; set; }
}

// The generator creates BlogValidator that you can inject and use
public class BlogsController : ControllerBase
{
    private readonly IValidator<Blog> _validator;

    public BlogsController(IValidator<Blog> validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Blog blog)
    {
        var validationResult = await _validator.ValidateAsync(blog);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // Save blog...
    }
}
```

## What You've Learned

In this tutorial, you:

✅ Scaffolded EF Core entities from an existing database
✅ Created a domain manifest for existing entities
✅ Added business rules without modifying generated code
✅ Integrated JD.Domain with ASP.NET Core
✅ Used automatic validation middleware
✅ Generated FluentValidation validators
✅ Tested API endpoints with Swagger

## Key Concepts

### 1. Non-Invasive Rules

JD.Domain rules are completely external to your entities. Scaffolded code remains untouched and can be regenerated without losing your business logic.

### 2. Partial Classes

The scaffolded entities use `partial` classes, so you *could* extend them if needed, but it's not required for JD.Domain.

### 3. Separation of Concerns

- **Database** → Source of truth for schema
- **Scaffolded Entities** → Data access layer (unchanged)
- **Domain Rules** → Business logic layer (JD.Domain)
- **API** → Application layer (ASP.NET Core)

### 4. Multiple Validation Points

You can validate:
- Automatically in API middleware (`[DomainValidation]`)
- Manually with `IDomainEngine.Evaluate()`
- Using generated FluentValidation validators

## Next Steps

### Add More Rules

- Create context-dependent validators
- Add authorization policies
- Define derivation rules for computed properties

### Generate Rich Domain Types

Wrap scaffolded entities in rich types:

```bash
dotnet add package JD.Domain.DomainModel.Generator
```

This generates `DomainBlog` and `DomainPost` wrappers with construction safety.

### Migrate to Hybrid Workflow

As you gain confidence, gradually move some entities to code-first:

- Keep critical legacy tables as database-first
- Define new features as code-first
- Use snapshots to track evolution

See [Hybrid Workflow Tutorial](hybrid-workflow.md).

### Track Schema Changes

Use snapshots to detect database schema drift:

```bash
dotnet tool install -g JD.Domain.Cli
jd-domain snapshot --manifest blogging-v1.json --output ./snapshots
```

See [Version Management Tutorial](version-management.md).

## Troubleshooting

### Scaffolding Fails

Ensure connection string is correct and database exists:
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT name FROM sys.databases"
```

### Validators Not Generated

Check that `JD.Domain.FluentValidation.Generator` is installed:
```bash
dotnet list package
```

Rebuild the project:
```bash
dotnet clean
dotnet build
```

### Validation Not Working

Ensure you called `AddDomainValidation()` in `Program.cs` and `UseDomainValidation()` middleware is registered.

## Additional Resources

- **[ASP.NET Core Integration](aspnet-core-integration.md)** - Deep dive into middleware
- **[Business Rules Tutorial](business-rules.md)** - Advanced rule patterns
- **[Hybrid Workflow](hybrid-workflow.md)** - Mix database-first and code-first
- **[Sample Code](../../samples/JD.Domain.Samples.DbFirst/)** - Complete working example

## Get Help

- **Questions?** Open a [GitHub Issue](https://github.com/JerrettDavis/JD.Domain/issues)
- **Found a bug?** Report it on [GitHub](https://github.com/JerrettDavis/JD.Domain/issues)

Congratulations on completing the Database-First walkthrough! You've successfully added rich domain validation to existing scaffolded code without modifying a single generated file.
