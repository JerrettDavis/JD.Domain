using JD.Domain.Abstractions;
using JD.Domain.Cli;
using JD.Domain.Cli.Commands;
using JD.Domain.Snapshot;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace JD.Domain.Tests.Unit.Cli;

public sealed class CliCommandExecutionTests
{
    [Fact]
    public void SnapshotCommand_Execute_WritesSnapshot()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var manifest = CreateManifest("TestDomain", new Version(1, 0, 0));
            var manifestPath = WriteManifestFile(tempDir, manifest);
            var outputDir = Path.Combine(tempDir, "snapshots");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = SnapshotCommand.Create();
            var exitCode = Invoke(command, new[] { "--manifest", manifestPath, "--output", outputDir, "--version", "2.0.0" });

            var expectedPath = Path.Combine(outputDir, "TestDomain", "v2.0.0.json");
            Assert.Equal(0, exitCode);
            Assert.Equal(0, Environment.ExitCode);
            Assert.True(File.Exists(expectedPath));
            Assert.Contains("Snapshot created", output.ToString());
            Assert.Empty(error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void SnapshotCommand_Execute_MissingManifest_SetsExitCode()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = SnapshotCommand.Create();
            Invoke(command, new[] { "--manifest", Path.Combine(tempDir, "missing.json"), "--output", tempDir });

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("Manifest file not found", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void DiffCommand_Execute_WritesOutputFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var beforePath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "before.json");
            var afterPath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(2, 0, 0)), "after.json");
            var outputPath = Path.Combine(tempDir, "diff.json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = DiffCommand.Create();
            var exitCode = Invoke(command, new[] { beforePath, afterPath, "--format", "json", "--output", outputPath });

            Assert.Equal(0, exitCode);
            Assert.Equal(0, Environment.ExitCode);
            Assert.True(File.Exists(outputPath));
            Assert.Contains("\"domain\":", File.ReadAllText(outputPath));
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void DiffCommand_Execute_WritesToConsole_WhenNoOutput()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var beforePath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "before.json");
            var afterPath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "after.json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = DiffCommand.Create();
            Invoke(command, new[] { beforePath, afterPath });

            Assert.Equal(0, Environment.ExitCode);
            Assert.Contains("Domain Diff", output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void DiffCommand_Execute_MissingAfter_SetsExitCode()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var beforePath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "before.json");
            var missingAfter = Path.Combine(tempDir, "missing.json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = DiffCommand.Create();
            Invoke(command, new[] { beforePath, missingAfter });

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("After snapshot not found", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void DiffCommand_Execute_MissingBefore_SetsExitCode()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var missingBefore = Path.Combine(tempDir, "missing.json");
            var afterPath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "after.json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = DiffCommand.Create();
            Invoke(command, new[] { missingBefore, afterPath });

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("Before snapshot not found", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void DiffCommand_Execute_InvalidJson_SetsExitCode()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var beforePath = Path.Combine(tempDir, "before.json");
            var afterPath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "after.json");
            File.WriteAllText(beforePath, "invalid-json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = DiffCommand.Create();
            Invoke(command, new[] { beforePath, afterPath });

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("Error:", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void MigratePlanCommand_Execute_WritesOutputFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var beforePath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "before.json");
            var afterPath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(2, 0, 0)), "after.json");
            var outputPath = Path.Combine(tempDir, "plan.md");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = MigratePlanCommand.Create();
            Invoke(command, new[] { beforePath, afterPath, "--output", outputPath });

            Assert.Equal(0, Environment.ExitCode);
            Assert.True(File.Exists(outputPath));
            Assert.Contains("Migration Plan", File.ReadAllText(outputPath));
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void MigratePlanCommand_Execute_WritesToConsole_WhenNoOutput()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var beforePath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "before.json");
            var afterPath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "after.json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = MigratePlanCommand.Create();
            Invoke(command, new[] { beforePath, afterPath });

            Assert.Equal(0, Environment.ExitCode);
            Assert.Contains("Migration Plan", output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void MigratePlanCommand_Execute_MissingBefore_SetsExitCode()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var missingBefore = Path.Combine(tempDir, "missing.json");
            var afterPath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "after.json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = MigratePlanCommand.Create();
            Invoke(command, new[] { missingBefore, afterPath });

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("Before snapshot not found", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void MigratePlanCommand_Execute_MissingAfter_SetsExitCode()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var beforePath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "before.json");
            var missingAfter = Path.Combine(tempDir, "missing.json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = MigratePlanCommand.Create();
            Invoke(command, new[] { beforePath, missingAfter });

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("After snapshot not found", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void MigratePlanCommand_Execute_InvalidJson_SetsExitCode()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var beforePath = Path.Combine(tempDir, "before.json");
            var afterPath = WriteSnapshotFile(tempDir, CreateManifest("TestDomain", new Version(1, 0, 0)), "after.json");
            File.WriteAllText(beforePath, "invalid-json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = MigratePlanCommand.Create();
            Invoke(command, new[] { beforePath, afterPath });

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("Error:", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void SnapshotCommand_Execute_InvalidJson_SetsExitCode()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"JD.Domain.Tests.{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        Environment.ExitCode = 0;

        try
        {
            var manifestPath = Path.Combine(tempDir, "manifest.json");
            File.WriteAllText(manifestPath, "invalid-json");

            using var output = new StringWriter();
            using var error = new StringWriter();
            Console.SetOut(output);
            Console.SetError(error);

            var command = SnapshotCommand.Create();
            Invoke(command, new[] { "--manifest", manifestPath, "--output", tempDir });

            Assert.Equal(1, Environment.ExitCode);
            Assert.Contains("Error:", error.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Environment.ExitCode = 0;
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void Program_Main_ReturnsZero_ForHelp()
    {
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            var exitCode = Program.Main(new[] { "--help" });

            Assert.Equal(0, exitCode);
            Assert.Contains("JD.Domain CLI", output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private static DomainManifest CreateManifest(string name, Version version)
    {
        return new DomainManifest
        {
            Name = name,
            Version = version,
            Entities =
            [
                new EntityManifest
                {
                    Name = "Customer",
                    TypeName = $"{name}.Customer",
                    Properties =
                    [
                        new PropertyManifest { Name = "Id", TypeName = "System.Guid", IsRequired = true }
                    ],
                    KeyProperties = ["Id"]
                }
            ]
        };
    }

    private static string WriteManifestFile(string directory, DomainManifest manifest)
    {
        var writer = new SnapshotWriter();
        var json = writer.SerializeManifest(manifest);
        var path = Path.Combine(directory, "manifest.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static string WriteSnapshotFile(string directory, DomainManifest manifest, string fileName)
    {
        var writer = new SnapshotWriter();
        var snapshot = writer.CreateSnapshot(manifest);
        var json = writer.Serialize(snapshot);
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, json);
        return path;
    }

    private static int Invoke(Command command, string[] args)
    {
        return command.Parse(args).Invoke();
    }
}
