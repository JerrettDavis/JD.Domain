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
        var manifestOption = new Option<FileInfo>("--manifest")
        {
            Description = "Path to the domain manifest JSON file",
            Required = true
        };
        manifestOption.Aliases.Add("--manifest");
        manifestOption.Aliases.Add("-m");

        var outputOption = new Option<DirectoryInfo>("--output")
        {
            Description = "Output directory for snapshots",
            Required = true
        };
        outputOption.Aliases.Add("--output");
        outputOption.Aliases.Add("-o");

        var versionOption = new Option<string?>("--version")
        {
            Description = "Version override (defaults to manifest version)"
        };
        versionOption.Aliases.Add("--version");
        versionOption.Aliases.Add("-v");

        var command = new Command("snapshot", "Create a snapshot of a domain manifest");
        command.Options.Add(manifestOption);
        command.Options.Add(outputOption);
        command.Options.Add(versionOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var manifest = parseResult.GetValue(manifestOption);
            var output = parseResult.GetValue(outputOption);
            var version = parseResult.GetValue(versionOption);

            await ExecuteAsync(manifest!, output!, version);
        });

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
