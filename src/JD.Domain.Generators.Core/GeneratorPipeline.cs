namespace JD.Domain.Generators.Core;

/// <summary>
/// Pipeline for executing multiple code generators in sequence.
/// </summary>
public sealed class GeneratorPipeline
{
    private readonly List<ICodeGenerator> _generators = [];

    /// <summary>
    /// Adds a generator to the pipeline.
    /// </summary>
    public GeneratorPipeline Add(ICodeGenerator generator)
    {
        if (generator == null)
        {
            throw new ArgumentNullException(nameof(generator));
        }

        _generators.Add(generator);
        return this;
    }

    /// <summary>
    /// Executes all generators in the pipeline.
    /// </summary>
    public IEnumerable<GeneratedFile> Execute(GeneratorContext context, CancellationToken cancellationToken = default)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var results = new List<GeneratedFile>();

        foreach (var generator in _generators)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (generator.CanGenerate(context))
            {
                var files = generator.Generate(context, cancellationToken);
                results.AddRange(files);
            }
        }

        return results;
    }

    /// <summary>
    /// Gets all generators in the pipeline.
    /// </summary>
    public IReadOnlyList<ICodeGenerator> Generators => _generators.AsReadOnly();
}
