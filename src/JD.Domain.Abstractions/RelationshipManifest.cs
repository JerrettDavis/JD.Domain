namespace JD.Domain.Abstractions;

/// <summary>
/// Represents a relationship between entities.
/// </summary>
public sealed class RelationshipManifest
{
    /// <summary>
    /// Gets the principal entity name.
    /// </summary>
    public required string PrincipalEntity { get; init; }

    /// <summary>
    /// Gets the dependent entity name.
    /// </summary>
    public required string DependentEntity { get; init; }

    /// <summary>
    /// Gets the relationship type (OneToMany, OneToOne, ManyToMany).
    /// </summary>
    public required string RelationshipType { get; init; }

    /// <summary>
    /// Gets the principal navigation property name.
    /// </summary>
    public string? PrincipalNavigation { get; init; }

    /// <summary>
    /// Gets the dependent navigation property name.
    /// </summary>
    public string? DependentNavigation { get; init; }

    /// <summary>
    /// Gets the foreign key property names.
    /// </summary>
    public IReadOnlyList<string> ForeignKeyProperties { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the relationship is required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets the delete behavior (Cascade, SetNull, Restrict, NoAction).
    /// </summary>
    public string? DeleteBehavior { get; init; }

    /// <summary>
    /// Gets the join entity name for many-to-many relationships.
    /// </summary>
    public string? JoinEntity { get; init; }

    /// <summary>
    /// Gets additional relationship metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = 
        new Dictionary<string, object?>();
}
