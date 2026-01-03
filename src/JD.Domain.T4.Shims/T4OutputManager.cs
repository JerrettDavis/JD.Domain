using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JD.Domain.T4.Shims;

/// <summary>
/// Manages T4 template output files for deterministic generation.
/// </summary>
public sealed class T4OutputManager
{
    private readonly string _outputDirectory;
    private readonly List<GeneratedFile> _files = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="T4OutputManager"/> class.
    /// </summary>
    /// <param name="outputDirectory">The output directory for generated files.</param>
    public T4OutputManager(string outputDirectory)
    {
        _outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
    }

    /// <summary>
    /// Adds a file to be generated.
    /// </summary>
    /// <param name="fileName">The file name (relative to output directory).</param>
    /// <param name="content">The file content.</param>
    public void AddFile(string fileName, string content)
    {
        _files.Add(new GeneratedFile(fileName, content));
    }

    /// <summary>
    /// Writes all files to disk.
    /// </summary>
    /// <param name="cleanDirectory">Whether to clean the directory first.</param>
    public void WriteAll(bool cleanDirectory = false)
    {
        if (cleanDirectory && Directory.Exists(_outputDirectory))
        {
            // Only delete .cs files to preserve other content
            foreach (var file in Directory.GetFiles(_outputDirectory, "*.cs", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }

        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }

        // Sort files for deterministic output
        foreach (var file in _files.OrderBy(f => f.FileName, StringComparer.Ordinal))
        {
            var fullPath = Path.Combine(_outputDirectory, file.FileName);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, file.Content);
        }
    }

    /// <summary>
    /// Gets the list of files that would be generated.
    /// </summary>
    /// <returns>The list of file names.</returns>
    public IReadOnlyList<string> GetFileNames()
    {
        return _files.Select(f => f.FileName).OrderBy(f => f, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// Clears all pending files.
    /// </summary>
    public void Clear()
    {
        _files.Clear();
    }

    private sealed class GeneratedFile
    {
        public string FileName { get; }
        public string Content { get; }

        public GeneratedFile(string fileName, string content)
        {
            FileName = fileName;
            Content = content;
        }
    }
}
