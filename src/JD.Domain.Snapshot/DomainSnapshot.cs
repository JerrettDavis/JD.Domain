using JD.Domain.Abstractions;

namespace JD.Domain.Snapshot;

/// <summary>
/// Represents a snapshot of a domain manifest at a point in time.
/// </summary>
public sealed class DomainSnapshot
{
    /// <summary>
    /// Gets the schema version for snapshot format compatibility.
    /// </summary>
    public const string SchemaVersion = "1.0";

    /// <summary>
    /// Gets the schema URI for JSON validation.
    /// </summary>
    public const string SchemaUri = "https://jd.domain/schemas/snapshot-v1.json";

    /// <summary>
    /// Gets the domain name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the domain version.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    /// Gets the SHA256 hash of the canonical JSON representation.
    /// </summary>
    public required string Hash { get; init; }

    /// <summary>
    /// Gets the timestamp when the snapshot was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the domain manifest.
    /// </summary>
    public required DomainManifest Manifest { get; init; }

    /// <summary>
    /// Creates a new snapshot from a manifest.
    /// </summary>
    /// <param name="manifest">The domain manifest to snapshot.</param>
    /// <param name="hash">The computed hash of the canonical JSON.</param>
    /// <returns>A new snapshot instance.</returns>
    public static DomainSnapshot Create(DomainManifest manifest, string hash)
    {
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));
        if (string.IsNullOrEmpty(hash)) throw new ArgumentException("Hash cannot be null or empty.", nameof(hash));

        return new DomainSnapshot
        {
            Name = manifest.Name,
            Version = manifest.Version,
            Hash = hash,
            CreatedAt = manifest.CreatedAt,
            Manifest = manifest
        };
    }
}
