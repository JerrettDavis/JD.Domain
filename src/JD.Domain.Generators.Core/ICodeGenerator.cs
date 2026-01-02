using System.Collections.Generic;
using System.Threading;

namespace JD.Domain.Generators.Core;

/// <summary>
/// Interface for code generators that produce generated files from domain manifests.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Gets the name of this generator.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Generates code files from the given context.
    /// </summary>
    /// <param name="context">The generator context containing manifest and compilation information.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A collection of generated files.</returns>
    IEnumerable<GeneratedFile> Generate(GeneratorContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if this generator can process the given manifest.
    /// </summary>
    /// <param name="context">The generator context to check.</param>
    /// <returns>True if this generator can process the manifest, false otherwise.</returns>
    bool CanGenerate(GeneratorContext context);
}
