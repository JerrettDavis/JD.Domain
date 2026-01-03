using JD.Domain.Abstractions;
using Microsoft.CodeAnalysis;

namespace JD.Domain.Generators.Core;

/// <summary>
/// Context for generator execution containing manifest and compilation information.
/// </summary>
public sealed class GeneratorContext
{
    /// <summary>
    /// Gets the domain manifest being processed.
    /// </summary>
    public required DomainManifest Manifest { get; init; }

    /// <summary>
    /// Gets the compilation context.
    /// </summary>
    public required Compilation Compilation { get; init; }

    /// <summary>
    /// Gets the cancellation token for the generator execution.
    /// </summary>
    public required System.Threading.CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Gets custom properties for generator configuration.
    /// </summary>
    public Dictionary<string, string> Properties { get; init; } = new();
}
