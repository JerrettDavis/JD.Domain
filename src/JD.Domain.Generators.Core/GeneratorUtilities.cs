using System.Security.Cryptography;
using System.Text;

namespace JD.Domain.Generators.Core;

/// <summary>
/// Utility methods for code generation.
/// </summary>
public static class GeneratorUtilities
{
    /// <summary>
    /// Computes a stable hash of the given content for change detection.
    /// </summary>
    public static string ComputeHash(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>
    /// Generates a valid C# identifier from the given name.
    /// </summary>
    public static string ToIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        var sb = new StringBuilder();
        var first = true;

        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                if (first && char.IsDigit(c))
                {
                    sb.Append('_');
                }
                sb.Append(c);
                first = false;
            }
            else if (!first)
            {
                sb.Append('_');
            }
        }

        var result = sb.ToString();
        return string.IsNullOrEmpty(result) ? "_" : result;
    }

    /// <summary>
    /// Sorts a collection of items deterministically for stable output.
    /// </summary>
    public static IEnumerable<T> SortDeterministically<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        return source.OrderBy(keySelector);
    }

    /// <summary>
    /// Formats a type name for code generation.
    /// </summary>
    public static string FormatTypeName(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
        var genericArgs = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
        return $"{genericTypeName}<{genericArgs}>";
    }

    /// <summary>
    /// Escapes a string for use in C# string literals.
    /// </summary>
    public static string EscapeStringLiteral(string value)
    {
        if (value == null)
        {
            return "null";
        }

        return "\"" + value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            + "\"";
    }

    /// <summary>
    /// Generates a deterministic file name from entity or type name.
    /// </summary>
    public static string GenerateFileName(string baseName, string suffix, string extension = ".g.cs")
    {
        var identifier = ToIdentifier(baseName);
        if (string.IsNullOrEmpty(suffix))
        {
            return $"{identifier}{extension}";
        }
        return $"{identifier}.{suffix}{extension}";
    }

    /// <summary>
    /// Normalizes line endings for consistent output across platforms.
    /// </summary>
    public static string NormalizeLineEndings(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        // Normalize all line endings to \n for consistent source generation output
        return content.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}
