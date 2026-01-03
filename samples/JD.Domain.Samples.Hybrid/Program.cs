using JD.Domain.Abstractions;
using JD.Domain.Rules;
using JD.Domain.Runtime;
using JD.Domain.Diff;
using JD.Domain.Snapshot;

namespace JD.Domain.Samples.Hybrid;

/// <summary>
/// Demonstrates hybrid workflow: combining existing models with new JD definitions,
/// and using snapshot/diff for version management.
/// </summary>
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== JD.Domain Hybrid Sample ===\n");

        // Step 1: Create initial version (v1.0.0)
        Console.WriteLine("1. Creating initial domain version (v1.0.0)...");
        var v1 = CreateDomainV1();
        var writer = new SnapshotWriter();
        var snapshotV1 = writer.CreateSnapshot(v1);
        Console.WriteLine($"   Version: {snapshotV1.Version}");
        Console.WriteLine($"   Hash: {snapshotV1.Hash}");
        Console.WriteLine($"   Entities: {v1.Entities.Count}");

        // Step 2: Create updated version (v1.1.0) with new entity
        Console.WriteLine("\n2. Creating updated domain version (v1.1.0)...");
        var v1_1 = CreateDomainV1_1();
        var snapshotV1_1 = writer.CreateSnapshot(v1_1);
        Console.WriteLine($"   Version: {snapshotV1_1.Version}");
        Console.WriteLine($"   Hash: {snapshotV1_1.Hash}");
        Console.WriteLine($"   Entities: {v1_1.Entities.Count}");

        // Step 3: Generate diff between versions
        Console.WriteLine("\n3. Comparing versions...");
        var diffEngine = new DiffEngine();
        var diff = diffEngine.Compare(snapshotV1, snapshotV1_1);
        Console.WriteLine($"   Has changes: {diff.HasChanges}");
        Console.WriteLine($"   Total changes: {diff.TotalChanges}");
        Console.WriteLine($"   Breaking changes: {diff.HasBreakingChanges}");

        // Step 4: Display diff in markdown format
        Console.WriteLine("\n4. Diff details:");
        var formatter = new DiffFormatter();
        var diffMarkdown = formatter.FormatAsMarkdown(diff);
        foreach (var line in diffMarkdown.Split('\n').Take(15))
        {
            Console.WriteLine($"   {line}");
        }

        // Step 5: Generate migration plan
        Console.WriteLine("\n5. Migration plan:");
        var planGenerator = new MigrationPlanGenerator();
        var plan = planGenerator.Generate(diff);
        foreach (var line in plan.Split('\n').Take(10))
        {
            Console.WriteLine($"   {line}");
        }

        // Step 6: Define rules that span both legacy and new entities
        Console.WriteLine("\n6. Defining cross-version rules...");
        var userRules = new RuleSetBuilder<User>("Default")
            .Invariant("User.Username.Required", u => !string.IsNullOrWhiteSpace(u.Username))
            .WithMessage("Username is required")
            .Invariant("User.Username.MinLength", u => u.Username.Length >= 3)
            .WithMessage("Username must be at least 3 characters")
            .Invariant("User.Email.Required", u => !string.IsNullOrWhiteSpace(u.Email))
            .WithMessage("Email is required")
            .BuildCompiled();

        var profileRules = new RuleSetBuilder<UserProfile>("Default")
            .Invariant("Profile.DisplayName.MaxLength", p => p.DisplayName == null || p.DisplayName.Length <= 50)
            .WithMessage("Display name cannot exceed 50 characters")
            .BuildCompiled();

        // Step 7: Validate entities using compiled rules
        Console.WriteLine("\n7. Validating entities...");

        var user = new User { Id = 1, Username = "jdavis", Email = "jd@example.com" };
        var userResult = userRules.Evaluate(user);
        Console.WriteLine($"   User validation: {(userResult.IsValid ? "PASSED" : "FAILED")}");
        foreach (var error in userResult.Errors)
        {
            Console.WriteLine($"      - {error.Message}");
        }

        var profile = new UserProfile { Id = 1, UserId = 1, DisplayName = "John Doe" };
        var profileResult = profileRules.Evaluate(profile);
        Console.WriteLine($"   Profile validation: {(profileResult.IsValid ? "PASSED" : "FAILED")}");

        Console.WriteLine("\n=== Sample Complete ===");
    }

    private static DomainManifest CreateDomainV1()
    {
        return new DomainManifest
        {
            Name = "UserManagement",
            Version = new Version(1, 0, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "User",
                    TypeName = "JD.Domain.Samples.Hybrid.User",
                    Properties =
                    [
                        new PropertyManifest { Name = "Id", TypeName = "System.Int32", IsRequired = true },
                        new PropertyManifest { Name = "Username", TypeName = "System.String", IsRequired = true, MaxLength = 50 },
                        new PropertyManifest { Name = "Email", TypeName = "System.String", IsRequired = true, MaxLength = 200 }
                    ],
                    KeyProperties = ["Id"]
                }
            ]
        };
    }

    private static DomainManifest CreateDomainV1_1()
    {
        return new DomainManifest
        {
            Name = "UserManagement",
            Version = new Version(1, 1, 0),
            Entities =
            [
                new EntityManifest
                {
                    Name = "User",
                    TypeName = "JD.Domain.Samples.Hybrid.User",
                    Properties =
                    [
                        new PropertyManifest { Name = "Id", TypeName = "System.Int32", IsRequired = true },
                        new PropertyManifest { Name = "Username", TypeName = "System.String", IsRequired = true, MaxLength = 50 },
                        new PropertyManifest { Name = "Email", TypeName = "System.String", IsRequired = true, MaxLength = 200 }
                    ],
                    KeyProperties = ["Id"]
                },
                new EntityManifest
                {
                    Name = "UserProfile",
                    TypeName = "JD.Domain.Samples.Hybrid.UserProfile",
                    Properties =
                    [
                        new PropertyManifest { Name = "Id", TypeName = "System.Int32", IsRequired = true },
                        new PropertyManifest { Name = "UserId", TypeName = "System.Int32", IsRequired = true },
                        new PropertyManifest { Name = "DisplayName", TypeName = "System.String", IsRequired = false, MaxLength = 50 },
                        new PropertyManifest { Name = "Bio", TypeName = "System.String", IsRequired = false }
                    ],
                    KeyProperties = ["Id"]
                }
            ]
        };
    }
}

// Domain entities
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public UserProfile? Profile { get; set; }
}

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
}
