using System.CommandLine;
using JD.Domain.Diff;
using JD.Domain.Snapshot;

namespace JD.Domain.Cli.Commands;

/// <summary>
/// Command for generating migration plans.
/// </summary>
public static class MigratePlanCommand
{
    /// <summary>
    /// Creates the migrate-plan command.
    /// </summary>
    public static Command Create()
    {
        var beforeArg = new Argument<FileInfo>("before")
        {
            Description = "Path to the 'before' snapshot file"
        };

        var afterArg = new Argument<FileInfo>("after")
        {
            Description = "Path to the 'after' snapshot file"
        };

        var outputOption = new Option<FileInfo?>("--output", "-o")
        {
            Description = "Output file (defaults to stdout)"
        };

        var command = new Command("migrate-plan", "Generate a migration plan between snapshots");
        command.Arguments.Add(beforeArg);
        command.Arguments.Add(afterArg);
        command.Options.Add(outputOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var before = parseResult.GetValue(beforeArg);
            var after = parseResult.GetValue(afterArg);
            var output = parseResult.GetValue(outputOption);

            await ExecuteAsync(before!, after!, output);
        });

        return command;
    }

    private static async Task ExecuteAsync(FileInfo beforeFile, FileInfo afterFile, FileInfo? outputFile)
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

            var generator = new MigrationPlanGenerator();
            var plan = generator.Generate(diff);

            if (outputFile != null)
            {
                outputFile.Directory?.Create();
                await File.WriteAllTextAsync(outputFile.FullName, plan);
                Console.WriteLine($"Migration plan written to: {outputFile.FullName}");
            }
            else
            {
                Console.WriteLine(plan);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }
}
