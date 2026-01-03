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
    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("JD.Domain CLI - Domain model tooling");
        rootCommand.Subcommands.Add(SnapshotCommand.Create());
        rootCommand.Subcommands.Add(DiffCommand.Create());
        rootCommand.Subcommands.Add(MigratePlanCommand.Create());

        return rootCommand.Parse(args).Invoke();
    }
}
