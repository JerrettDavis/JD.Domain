using System;
using System.IO;
using JD.Domain.Abstractions;
using JD.Domain.Snapshot;

namespace JD.Domain.T4.Shims;

/// <summary>
/// Loads domain manifests for use in T4 templates.
/// </summary>
public static class T4ManifestLoader
{
    /// <summary>
    /// Loads a domain manifest from a JSON file.
    /// </summary>
    /// <param name="path">The path to the manifest JSON file.</param>
    /// <returns>The loaded manifest.</returns>
    public static DomainManifest LoadManifest(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Manifest file not found: {path}", path);

        var json = File.ReadAllText(path);
        var reader = new SnapshotReader();
        return reader.DeserializeManifest(json);
    }

    /// <summary>
    /// Loads a domain snapshot from a JSON file.
    /// </summary>
    /// <param name="path">The path to the snapshot JSON file.</param>
    /// <returns>The loaded snapshot.</returns>
    public static DomainSnapshot LoadSnapshot(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException($"Snapshot file not found: {path}", path);

        var json = File.ReadAllText(path);
        var reader = new SnapshotReader();
        return reader.Deserialize(json);
    }

    /// <summary>
    /// Tries to load a manifest from a path, returning null if not found.
    /// </summary>
    /// <param name="path">The path to the manifest JSON file.</param>
    /// <returns>The loaded manifest, or null if not found.</returns>
    public static DomainManifest? TryLoadManifest(string path)
    {
        try
        {
            return LoadManifest(path);
        }
        catch
        {
            return null;
        }
    }
}
