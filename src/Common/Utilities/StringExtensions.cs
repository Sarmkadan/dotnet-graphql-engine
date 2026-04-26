#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Extension methods for string operations
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to camelCase
    /// </summary>
    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var words = input.Split('_', '-', ' ');
        if (words.Length == 0) return input;

        var result = words[0].ToLowerInvariant();
        for (int i = 1; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
                result += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLowerInvariant();
        }

        return result;
    }

    /// <summary>
    /// Converts a string to PascalCase
    /// </summary>
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var words = input.Split('_', '-', ' ');
        var result = string.Empty;

        foreach (var word in words)
        {
            if (!string.IsNullOrEmpty(word))
                result += char.ToUpper(word[0]) + word.Substring(1).ToLowerInvariant();
        }

        return result;
    }

    /// <summary>
    /// Converts a string to snake_case
    /// </summary>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = string.Empty;
        foreach (var c in input)
        {
            if (char.IsUpper(c) && result.Length > 0)
                result += "_";
            result += char.ToLowerInvariant(c);
        }

        return result;
    }

    /// <summary>
    /// Truncates a string to a maximum length
    /// </summary>
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        if (input is null || input.Length <= maxLength) return input;

        return input.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Checks if a string is a valid GraphQL name
    /// </summary>
    public static bool IsValidGraphQLName(this string input)
    {
        if (string.IsNullOrEmpty(input)) return false;

        // GraphQL names must start with a letter or underscore
        if (!char.IsLetter(input[0]) && input[0] != '_') return false;

        // Subsequent characters can be letters, digits, or underscores
        foreach (var c in input.Substring(1))
        {
            if (!char.IsLetterOrDigit(c) && c != '_') return false;
        }

        return true;
    }

    /// <summary>
    /// Normalizes whitespace in a string
    /// </summary>
    public static string NormalizeWhitespace(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Escapes GraphQL query string for safe transmission
    /// </summary>
    public static string EscapeGraphQLString(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return input
            .Replace("\"", "\\\"")
            .Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
