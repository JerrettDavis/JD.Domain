using System.CommandLine;
using JD.Domain.Abstractions;
using JD.Domain.Snapshot;

namespace JD.Domain.Cli.Commands;

/// <summary>
/// Command for creating domain snapshots.
/// </summary>
public static class SnapshotCommand
{
    /// <summary>
    /// Creates the snapshot command.
    /// </summary>
    public static Command Create()
    {
        var manifestOption = new Option<FileInfo>(
            aliases: ["--manifest", "-m"],
            description: "Path to the domain manifest JSON file")
        {
            IsRequired = true
        };

        var outputOption = new Option<DirectoryInfo>(
            aliases: ["--output", "-o"],
            description: "Output directory for snapshots")
        {
            IsRequired = true
        };

        var versionOption = new Option<string?>(
            aliases: ["--version", "-v"],
            description: "Version override (defaults to manifest version)");

        var command = new Command("snapshot", "Create a snapshot of a domain manifest")
        {
            manifestOption,
            outputOption,
            versionOption
        };

        command.SetHandler(async (manifest, output, version) =>
        {
            await ExecuteAsync(manifest, output, version);
        }, manifestOption, outputOption, versionOption);

        return command;
    }

    private static async Task ExecuteAsync(FileInfo manifestFile, DirectoryInfo outputDir, string? versionOverride)
    {
        if (!manifestFile.Exists)
        {
            Console.Error.WriteLine($"Error: Manifest file not found: {manifestFile.FullName}");
            Environment.ExitCode = 1;
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(manifestFile.FullName);
            var reader = new SnapshotReader();
            var manifest = reader.DeserializeManifest(json);

            // Apply version override if provided
            if (!string.IsNullOrEmpty(versionOverride) && Version.TryParse(versionOverride, out var v))
            {
                manifest = new DomainManifest
                {
                    Name = manifest.Name,
                    Version = v,
                    Hash = manifest.Hash,
                    CreatedAt = manifest.CreatedAt,
                    Entities = manifest.Entities,
                    ValueObjects = manifest.ValueObjects,
                    Enums = manifest.Enums,
                    RuleSets = manifest.RuleSets,
                    Configurations = manifest.Configurations,
                    Sources = manifest.Sources,
                    Metadata = manifest.Metadata
                };
            }

            var options = new SnapshotOptions
            {
                OutputDirectory = outputDir.FullName
            };

            var writer = new SnapshotWriter(options);
            var storage = new SnapshotStorage(options);

            var snapshot = writer.CreateSnapshot(manifest);

            if (!outputDir.Exists)
            {
                outputDir.Create();
            }

            var outputPath = storage.SaveSnapshot(snapshot);

            Console.WriteLine($"Snapshot created: {outputPath}");
            Console.WriteLine($"  Name: {snapshot.Name}");
            Console.WriteLine($"  Version: {snapshot.Version}");
            Console.WriteLine($"  Hash: {snapshot.Hash}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
