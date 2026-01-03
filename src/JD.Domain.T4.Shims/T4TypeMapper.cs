namespace JD.Domain.T4.Shims;

/// <summary>
/// Maps CLR type names to C# keywords and EF Core conventions.
/// </summary>
public static class T4TypeMapper
{
    private static readonly Dictionary<string, string> ClrToCSharp = new(StringComparer.Ordinal)
    {
        ["System.Boolean"] = "bool",
        ["System.Byte"] = "byte",
        ["System.SByte"] = "sbyte",
        ["System.Char"] = "char",
        ["System.Decimal"] = "decimal",
        ["System.Double"] = "double",
        ["System.Single"] = "float",
        ["System.Int32"] = "int",
        ["System.UInt32"] = "uint",
        ["System.Int64"] = "long",
        ["System.UInt64"] = "ulong",
        ["System.Int16"] = "short",
        ["System.UInt16"] = "ushort",
        ["System.String"] = "string",
        ["System.Object"] = "object",
        ["System.Void"] = "void"
    };

    private static readonly Dictionary<string, string> SqlServerTypes = new(StringComparer.Ordinal)
    {
        ["System.Boolean"] = "bit",
        ["System.Byte"] = "tinyint",
        ["System.Int16"] = "smallint",
        ["System.Int32"] = "int",
        ["System.Int64"] = "bigint",
        ["System.Decimal"] = "decimal",
        ["System.Double"] = "float",
        ["System.Single"] = "real",
        ["System.String"] = "nvarchar",
        ["System.DateTime"] = "datetime2",
        ["System.DateTimeOffset"] = "datetimeoffset",
        ["System.DateOnly"] = "date",
        ["System.TimeOnly"] = "time",
        ["System.TimeSpan"] = "time",
        ["System.Guid"] = "uniqueidentifier",
        ["System.Byte[]"] = "varbinary"
    };

    /// <summary>
    /// Converts a CLR type name to a C# keyword where applicable.
    /// </summary>
    /// <param name="clrTypeName">The CLR type name.</param>
    /// <returns>The C# type name.</returns>
    public static string ToCSharpType(string clrTypeName)
    {
        if (string.IsNullOrEmpty(clrTypeName))
            return clrTypeName;

        // Handle nullable types
        var isNullable = clrTypeName.EndsWith("?");
        var baseType = isNullable ? clrTypeName.TrimEnd('?') : clrTypeName;

        // Handle Nullable<T> syntax
        if (baseType.StartsWith("System.Nullable`1["))
        {
            var innerType = baseType.Substring(18, baseType.Length - 19);
            return ToCSharpType(innerType) + "?";
        }

        if (ClrToCSharp.TryGetValue(baseType, out var csharpType))
        {
            return isNullable ? csharpType + "?" : csharpType;
        }

        // Return simple name for non-system types
        var lastDot = baseType.LastIndexOf('.');
        var simpleName = lastDot >= 0 ? baseType.Substring(lastDot + 1) : baseType;
        return isNullable ? simpleName + "?" : simpleName;
    }

    /// <summary>
    /// Gets the SQL Server type for a CLR type.
    /// </summary>
    /// <param name="clrTypeName">The CLR type name.</param>
    /// <param name="maxLength">Optional max length for string types.</param>
    /// <returns>The SQL Server type name.</returns>
    public static string ToSqlServerType(string clrTypeName, int? maxLength = null)
    {
        if (string.IsNullOrEmpty(clrTypeName))
            return "nvarchar(max)";

        var baseType = clrTypeName.TrimEnd('?');

        if (SqlServerTypes.TryGetValue(baseType, out var sqlType))
        {
            if (sqlType == "nvarchar" || sqlType == "varbinary")
            {
                var length = maxLength.HasValue ? maxLength.Value.ToString() : "max";
                return $"{sqlType}({length})";
            }
            return sqlType;
        }

        return "nvarchar(max)";
    }

    /// <summary>
    /// Determines if a type is a primitive or simple type.
    /// </summary>
    /// <param name="clrTypeName">The CLR type name.</param>
    /// <returns>True if the type is primitive or simple.</returns>
    public static bool IsPrimitiveOrSimple(string clrTypeName)
    {
        if (string.IsNullOrEmpty(clrTypeName))
            return false;

        var baseType = clrTypeName.TrimEnd('?');
        return ClrToCSharp.ContainsKey(baseType) ||
               baseType == "System.DateTime" ||
               baseType == "System.DateTimeOffset" ||
               baseType == "System.DateOnly" ||
               baseType == "System.TimeOnly" ||
               baseType == "System.TimeSpan" ||
               baseType == "System.Guid" ||
               baseType == "System.Byte[]";
    }

    /// <summary>
    /// Determines if a type is a collection type.
    /// </summary>
    /// <param name="clrTypeName">The CLR type name.</param>
    /// <returns>True if the type is a collection.</returns>
    public static bool IsCollection(string clrTypeName)
    {
        if (string.IsNullOrEmpty(clrTypeName))
            return false;

        return clrTypeName.Contains("ICollection") ||
               clrTypeName.Contains("IList") ||
               clrTypeName.Contains("IEnumerable") ||
               clrTypeName.Contains("List`1") ||
               clrTypeName.Contains("HashSet`1") ||
               clrTypeName.Contains("[]");
    }
}
