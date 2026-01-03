using System.CommandLine;
using JD.Domain.Diff;
using JD.Domain.Snapshot;

namespace JD.Domain.Cli.Commands;

/// <summary>
/// Command for comparing domain snapshots.
/// </summary>
public static class DiffCommand
{
    /// <summary>
    /// Creates the diff command.
    /// </summary>
    public static Command Create()
    {
        var beforeArg = new Argument<FileInfo>(
            name: "before",
            description: "Path to the 'before' snapshot file");

        var afterArg = new Argument<FileInfo>(
            name: "after",
            description: "Path to the 'after' snapshot file");

        var formatOption = new Option<string>(
            aliases: ["--format", "-f"],
            getDefaultValue: () => "md",
            description: "Output format: md (Markdown) or json");

        var outputOption = new Option<FileInfo?>(
            aliases: ["--output", "-o"],
            description: "Output file (defaults to stdout)");

        var command = new Command("diff", "Compare two domain snapshots")
        {
            beforeArg,
            afterArg,
            formatOption,
            outputOption
        };

        command.SetHandler(async (before, after, format, output) =>
        {
            await ExecuteAsync(before, after, format, output);
        }, beforeArg, afterArg, formatOption, outputOption);

        return command;
    }

    private static async Task ExecuteAsync(FileInfo beforeFile, FileInfo afterFile, string format, FileInfo? outputFile)
    {
        if (!beforeFile.Exists)
        {
            Console.Error.WriteLine($"Error: Before snapshot not found: {beforeFile.FullName}");
            Environment.ExitCode = 1;
            return;
        }

        if (!afterFile.Exists)
        {
            Console.Error.WriteLine($"Error: After snapshot not found: {afterFile.FullName}");
            Environment.ExitCode = 1;
            return;
        }

        try
        {
            var reader = new SnapshotReader();
            var beforeSnapshot = reader.Deserialize(await File.ReadAllTextAsync(beforeFile.FullName));
            var afterSnapshot = reader.Deserialize(await File.ReadAllTextAsync(afterFile.FullName));

            var engine = new DiffEngine();
            var diff = engine.Compare(beforeSnapshot, afterSnapshot);

            var formatter = new DiffFormatter();
            var result = format.ToLowerInvariant() switch
            {
                "json" => formatter.FormatAsJson(diff),
                _ => formatter.FormatAsMarkdown(diff)
            };

            if (outputFile != null)
            {
                outputFile.Directory?.Create();
                await File.WriteAllTextAsync(outputFile.FullName, result);
                Console.WriteLine($"Diff written to: {outputFile.FullName}");
            }
            else
            {
                Console.WriteLine(result);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
