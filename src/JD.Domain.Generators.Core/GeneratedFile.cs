namespace JD.Domain.Generators.Core;

/// <summary>
/// Represents a generated file with name and content.
/// </summary>
public sealed class GeneratedFile
{
    /// <summary>
    /// Gets or initializes the file name (including extension).
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or initializes the file content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets or initializes the hint name for the source generator.
    /// </summary>
    public string HintName => FileName;

    /// <summary>
    /// Gets or initializes the timestamp when this file was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or initializes the hash of the content for change detection.
    /// </summary>
    public string? ContentHash { get; init; }
}
