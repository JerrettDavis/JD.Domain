using JD.Domain.Cli.Commands;
using System.CommandLine;

namespace JD.Domain.Tests.Unit.Cli;

public class CliTests
{
    [Fact]
    public void SnapshotCommand_Create_ReturnsCommand()
    {
        var command = SnapshotCommand.Create();

        Assert.NotNull(command);
        Assert.Equal("snapshot", command.Name);
        Assert.Contains("snapshot", command.Description.ToLowerInvariant());
    }

    [Fact]
    public void SnapshotCommand_Create_HasManifestOption()
    {
        var command = SnapshotCommand.Create();

        var manifestOption = command.Options.FirstOrDefault(o => o.HasAlias("--manifest"));
        Assert.NotNull(manifestOption);
        Assert.True(manifestOption.IsRequired);
    }

    [Fact]
    public void SnapshotCommand_Create_HasOutputOption()
    {
        var command = SnapshotCommand.Create();

        var outputOption = command.Options.FirstOrDefault(o => o.HasAlias("--output"));
        Assert.NotNull(outputOption);
        Assert.True(outputOption.IsRequired);
    }

    [Fact]
    public void SnapshotCommand_Create_HasVersionOption()
    {
        var command = SnapshotCommand.Create();

        var versionOption = command.Options.FirstOrDefault(o => o.HasAlias("--version"));
        Assert.NotNull(versionOption);
        Assert.False(versionOption.IsRequired);
    }

    [Fact]
    public void SnapshotCommand_Create_HasShortAliases()
    {
        var command = SnapshotCommand.Create();

        Assert.Contains(command.Options, o => o.HasAlias("-m"));
        Assert.Contains(command.Options, o => o.HasAlias("-o"));
        Assert.Contains(command.Options, o => o.HasAlias("-v"));
    }

    [Fact]
    public void DiffCommand_Create_ReturnsCommand()
    {
        var command = DiffCommand.Create();

        Assert.NotNull(command);
        Assert.Equal("diff", command.Name);
        Assert.Contains("compare", command.Description.ToLowerInvariant());
    }

    [Fact]
    public void DiffCommand_Create_HasBeforeArgument()
    {
        var command = DiffCommand.Create();

        var beforeArg = command.Arguments.FirstOrDefault(a => a.Name == "before");
        Assert.NotNull(beforeArg);
    }

    [Fact]
    public void DiffCommand_Create_HasAfterArgument()
    {
        var command = DiffCommand.Create();

        var afterArg = command.Arguments.FirstOrDefault(a => a.Name == "after");
        Assert.NotNull(afterArg);
    }

    [Fact]
    public void DiffCommand_Create_HasFormatOption()
    {
        var command = DiffCommand.Create();

        var formatOption = command.Options.FirstOrDefault(o => o.HasAlias("--format"));
        Assert.NotNull(formatOption);
        Assert.False(formatOption.IsRequired);
    }

    [Fact]
    public void DiffCommand_Create_HasShortAliases()
    {
        var command = DiffCommand.Create();

        Assert.Contains(command.Options, o => o.HasAlias("-f"));
        Assert.Contains(command.Options, o => o.HasAlias("-o"));
    }

    [Fact]
    public void MigratePlanCommand_Create_ReturnsCommand()
    {
        var command = MigratePlanCommand.Create();

        Assert.NotNull(command);
        Assert.Equal("migrate-plan", command.Name);
        Assert.Contains("migration", command.Description.ToLowerInvariant());
    }

    [Fact]
    public void MigratePlanCommand_Create_HasBeforeArgument()
    {
        var command = MigratePlanCommand.Create();

        var beforeArg = command.Arguments.FirstOrDefault(a => a.Name == "before");
        Assert.NotNull(beforeArg);
    }

    [Fact]
    public void MigratePlanCommand_Create_HasAfterArgument()
    {
        var command = MigratePlanCommand.Create();

        var afterArg = command.Arguments.FirstOrDefault(a => a.Name == "after");
        Assert.NotNull(afterArg);
    }

    [Fact]
    public void MigratePlanCommand_Create_HasOutputOption()
    {
        var command = MigratePlanCommand.Create();

        var outputOption = command.Options.FirstOrDefault(o => o.HasAlias("--output"));
        Assert.NotNull(outputOption);
        Assert.False(outputOption.IsRequired);
    }

    [Fact]
    public void MigratePlanCommand_Create_HasShortAlias()
    {
        var command = MigratePlanCommand.Create();

        Assert.Contains(command.Options, o => o.HasAlias("-o"));
    }
}
