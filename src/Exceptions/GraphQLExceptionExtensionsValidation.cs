#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Globalization;

namespace GraphQLEngine.Exceptions;

/// <summary>
/// Validation helpers for GraphQLException that are specific to the extension methods
/// provided by GraphQLExceptionExtensions. This complements GraphQLExceptionValidation
/// by focusing on constraints required for the extension methods to work correctly.
/// </summary>
public static class GraphQLExceptionExtensionsValidation
{
    /// <summary>
    /// Validates the GraphQLException instance for extension method compatibility.
    /// Ensures the exception has proper ErrorCode and Extensions for extension methods
    /// like AddFormattedErrorCode, GetErrorCodeOrDefault, etc.
    /// </summary>
    /// <param name="exception">The GraphQL exception to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateExtensionCompatibility(this GraphQLException? exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var problems = new List<string>();

        // Validate ErrorCode - extension methods like GetErrorCodeOrDefault depend on this
        if (string.IsNullOrWhiteSpace(exception.ErrorCode))
        {
            problems.Add("ErrorCode must not be null or whitespace for extension method compatibility.");
        }

        // Validate Extensions - most extension methods work with extensions
        if (exception.Extensions is null)
        {
            problems.Add("Extensions dictionary must not be null for extension method compatibility.");
        }
        else
        {
            // Validate that errorCode extension exists - AddFormattedErrorCode and GetErrorCodeOrDefault expect this
            if (exception.Extensions.TryGetValue("errorCode", out var errorCodeObj) && errorCodeObj is string errorCode)
            {
                if (string.IsNullOrWhiteSpace(errorCode))
                {
                    problems.Add("The 'errorCode' extension should not be null or whitespace.");
                }
            }
            else
            {
                problems.Add("Extensions should contain an 'errorCode' string extension for proper error tracking.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the GraphQLException instance is valid for extension method usage.
    /// </summary>
    /// <param name="exception">The GraphQL exception to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValidForExtensions(this GraphQLException? exception)
    {
        return exception is not null && ValidateExtensionCompatibility(exception).Count == 0;
    }

    /// <summary>
    /// Ensures the GraphQLException instance is valid for extension method usage,
    /// throwing an exception if not.
    /// </summary>
    /// <param name="exception">The GraphQL exception to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing the list of problems</exception>
    public static void EnsureValidForExtensions(this GraphQLException? exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var problems = ValidateExtensionCompatibility(exception);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"GraphQLException validation for extension methods failed:{Environment.NewLine}- ".Replace("- ", "") +
                string.Join(Environment.NewLine + "- ", problems),
                nameof(exception));
        }
    }
}