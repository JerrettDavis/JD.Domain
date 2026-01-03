using System.CommandLine;
using JD.Domain.Cli.Commands;

namespace JD.Domain.Cli;

/// <summary>
/// Entry point for the jd-domain CLI tool.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("JD.Domain CLI - Domain model tooling")
        {
            SnapshotCommand.Create(),
            DiffCommand.Create(),
            MigratePlanCommand.Create()
        };

        return await rootCommand.InvokeAsync(args);
    }
}
