#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Common.Constants;
using System.Text.RegularExpressions;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Helper utilities for validation
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates a GraphQL query string
    /// </summary>
    public static bool ValidateQueryString(string query, out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(query))
        {
            errors.Add("Query string cannot be empty");
            return false;
        }

        if (query.Length > GraphQLConstants.DefaultMaxQueryLength)
        {
            errors.Add($"Query exceeds maximum length of {GraphQLConstants.DefaultMaxQueryLength}");
            return false;
        }

        // Check for balanced braces
        var openBraces = query.Count(c => c == '{');
        var closeBraces = query.Count(c => c == '}');

        if (openBraces != closeBraces)
        {
            errors.Add("Mismatched braces in query");
            return false;
        }

        // Check for balanced parentheses
        var openParens = query.Count(c => c == '(');
        var closeParens = query.Count(c => c == ')');

        if (openParens != closeParens)
        {
            errors.Add("Mismatched parentheses in query");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates a GraphQL type name
    /// </summary>
    public static bool ValidateTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return false;

        // Type names must start with a letter or underscore
        if (!char.IsLetter(typeName[0]) && typeName[0] != '_') return false;

        // Subsequent characters can be letters, digits, or underscores
        foreach (var c in typeName.Substring(1))
        {
            if (!char.IsLetterOrDigit(c) && c != '_') return false;
        }

        return true;
    }

    /// <summary>
    /// Validates a GraphQL field name
    /// </summary>
    public static bool ValidateFieldName(string fieldName)
    {
        return ValidateTypeName(fieldName);
    }

    /// <summary>
    /// Validates an email address
    /// </summary>
    public static bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;

        try
        {
            var regex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a URL
    /// </summary>
    public static bool ValidateUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return false;

        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    /// <summary>
    /// Validates an object ID format
    /// </summary>
    public static bool ValidateId(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        // Accept UUIDs, numeric IDs, or alphanumeric strings
        var guidPattern = new Regex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase);
        var numericPattern = new Regex(@"^\d+$");
        var alphanumericPattern = new Regex(@"^[a-zA-Z0-9_-]+$");

        return guidPattern.IsMatch(id) || numericPattern.IsMatch(id) || alphanumericPattern.IsMatch(id);
    }

    /// <summary>
    /// Validates a complexity score
    /// </summary>
    public static bool ValidateComplexityScore(int score, int maxScore)
    {
        return score >= 0 && score <= maxScore;
    }

    /// <summary>
    /// Validates a depth value
    /// </summary>
    public static bool ValidateDepth(int depth, int maxDepth)
    {
        return depth >= 0 && depth <= maxDepth;
    }
}
