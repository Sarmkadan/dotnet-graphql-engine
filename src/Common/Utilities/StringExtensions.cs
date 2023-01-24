#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Extension methods for string operations
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to camelCase
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToCamelCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split('_', '-', ' ');
        if (words.Length == 0)
            return input;

        var result = words[0].ToLowerInvariant();
        for (int i = 1; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
                result += char.ToUpperInvariant(words[i][0]) + words[i].Substring(1).ToLowerInvariant();
        }

        return result;
    }

    /// <summary>
    /// Converts a string to PascalCase
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToPascalCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split('_', '-', ' ');
        var result = string.Empty;

        foreach (var word in words)
        {
            if (!string.IsNullOrEmpty(word))
                result += char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant();
        }

        return result;
    }

    /// <summary>
    /// Converts a string to snake_case
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToSnakeCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrEmpty(input))
            return input;

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
    /// <param name="input">The input string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="suffix">The suffix to append when truncating. Defaults to "...".</param>
    /// <returns>The truncated string, or the original string if it's shorter than maxLength.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suffix"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 0.</exception>
    public static string? Truncate(this string? input, int maxLength, string suffix = "...")
    {
        ArgumentNullException.ThrowIfNull(suffix);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (input is null || input.Length <= maxLength)
            return input;

        if (maxLength < suffix.Length)
            return suffix.Substring(0, maxLength);

        return input.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Checks if a string is a valid GraphQL name
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static bool IsValidGraphQLName(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrEmpty(input))
            return false;

        // GraphQL names must start with a letter or underscore
        if (!char.IsLetter(input[0]) && input[0] != '_')
            return false;

        // Subsequent characters can be letters, digits, or underscores
        for (int i = 1; i < input.Length; i++)
        {
            if (!char.IsLetterOrDigit(input[i]) && input[i] != '_')
                return false;
        }

        return true;
    }

    /// <summary>
    /// Normalizes whitespace in a string
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string NormalizeWhitespace(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrEmpty(input))
            return input;

        return System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Escapes GraphQL query string for safe transmission
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string EscapeGraphQLString(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrEmpty(input))
            return input;

        return input
            .Replace("\"", "\\\"")
            .Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}