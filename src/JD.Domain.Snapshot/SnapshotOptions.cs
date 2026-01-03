using System;

namespace JD.Domain.Snapshot;

/// <summary>
/// Configuration options for snapshot operations.
/// </summary>
public sealed class SnapshotOptions
{
    /// <summary>
    /// Gets or sets the base directory for storing snapshots.
    /// Default is "DomainSnapshots" in the current directory.
    /// </summary>
    public string OutputDirectory { get; set; } = "DomainSnapshots";

    /// <summary>
    /// Gets or sets whether to use indented JSON formatting.
    /// Default is true for readability.
    /// </summary>
    public bool IndentedJson { get; set; } = true;

    /// <summary>
    /// Gets or sets the file naming pattern.
    /// Supports {name} and {version} placeholders.
    /// Default is "v{version}.json".
    /// </summary>
    public string FileNamePattern { get; set; } = "v{version}.json";

    /// <summary>
    /// Gets or sets whether to organize snapshots in subdirectories by domain name.
    /// Default is true.
    /// </summary>
    public bool OrganizeByDomainName { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include the schema reference in the JSON.
    /// Default is true.
    /// </summary>
    public bool IncludeSchema { get; set; } = true;

    /// <summary>
    /// Formats the file name for a snapshot.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <param name="version">The domain version.</param>
    /// <returns>The formatted file name.</returns>
    public string FormatFileName(string name, Version version)
    {
        return FileNamePattern
            .Replace("{name}", name)
            .Replace("{version}", version.ToString());
    }

    /// <summary>
    /// Gets the full path for a snapshot file.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <param name="version">The domain version.</param>
    /// <returns>The full file path.</returns>
    public string GetFilePath(string name, Version version)
    {
        var fileName = FormatFileName(name, version);

        if (OrganizeByDomainName)
        {
            return System.IO.Path.Combine(OutputDirectory, name, fileName);
        }

        return System.IO.Path.Combine(OutputDirectory, fileName);
    }
}
