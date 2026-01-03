using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JD.Domain.Abstractions;

namespace JD.Domain.Snapshot;

/// <summary>
/// Handles file system operations for domain snapshots.
/// </summary>
public sealed class SnapshotStorage
{
    private readonly SnapshotOptions _options;
    private readonly SnapshotWriter _writer;
    private readonly SnapshotReader _reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotStorage"/> class.
    /// </summary>
    /// <param name="options">The snapshot options.</param>
    public SnapshotStorage(SnapshotOptions? options = null)
    {
        _options = options ?? new SnapshotOptions();
        _writer = new SnapshotWriter(_options);
        _reader = new SnapshotReader();
    }

    /// <summary>
    /// Saves a manifest as a snapshot.
    /// </summary>
    /// <param name="manifest">The manifest to save.</param>
    /// <returns>The created snapshot.</returns>
    public DomainSnapshot Save(DomainManifest manifest)
    {
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));

        var snapshot = _writer.CreateSnapshot(manifest);
        var json = _writer.Serialize(snapshot);
        var filePath = _options.GetFilePath(snapshot.Name, snapshot.Version);

        EnsureDirectoryExists(filePath);
        File.WriteAllText(filePath, json);

        return snapshot;
    }

    /// <summary>
    /// Saves a snapshot to disk.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <returns>The file path where the snapshot was saved.</returns>
    public string SaveSnapshot(DomainSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        var json = _writer.Serialize(snapshot);
        var filePath = _options.GetFilePath(snapshot.Name, snapshot.Version);

        EnsureDirectoryExists(filePath);
        File.WriteAllText(filePath, json);

        return filePath;
    }

    /// <summary>
    /// Loads a snapshot from a file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The loaded snapshot.</returns>
    public DomainSnapshot Load(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Snapshot file not found: {filePath}", filePath);

        var json = File.ReadAllText(filePath);
        return _reader.Deserialize(json);
    }

    /// <summary>
    /// Loads a snapshot by domain name and version.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <param name="version">The domain version.</param>
    /// <returns>The loaded snapshot.</returns>
    public DomainSnapshot Load(string name, Version version)
    {
        var filePath = _options.GetFilePath(name, version);
        return Load(filePath);
    }

    /// <summary>
    /// Checks if a snapshot exists.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <param name="version">The domain version.</param>
    /// <returns>True if the snapshot exists.</returns>
    public bool Exists(string name, Version version)
    {
        var filePath = _options.GetFilePath(name, version);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Lists all snapshots for a domain.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <returns>The list of snapshot versions.</returns>
    public IReadOnlyList<Version> ListVersions(string name)
    {
        var directory = _options.OrganizeByDomainName
            ? Path.Combine(_options.OutputDirectory, name)
            : _options.OutputDirectory;

        if (!Directory.Exists(directory))
            return Array.Empty<Version>();

        var versions = new List<Version>();
        foreach (var file in Directory.GetFiles(directory, "*.json"))
        {
            try
            {
                var snapshot = Load(file);
                if (snapshot.Name == name)
                {
                    versions.Add(snapshot.Version);
                }
            }
            catch
            {
                // Skip files that can't be parsed
            }
        }

        return versions.OrderBy(v => v).ToList();
    }

    /// <summary>
    /// Gets the latest snapshot for a domain.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <returns>The latest snapshot, or null if none exist.</returns>
    public DomainSnapshot? GetLatest(string name)
    {
        var versions = ListVersions(name);
        if (versions.Count == 0)
            return null;

        var latestVersion = versions[versions.Count - 1];
        return Load(name, latestVersion);
    }

    /// <summary>
    /// Deletes a snapshot.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <param name="version">The domain version.</param>
    /// <returns>True if the snapshot was deleted.</returns>
    public bool Delete(string name, Version version)
    {
        var filePath = _options.GetFilePath(name, version);
        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);
        return true;
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
