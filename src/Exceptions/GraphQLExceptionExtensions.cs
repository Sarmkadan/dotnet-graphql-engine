#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace GraphQLEngine.Exceptions;

/// <summary>
/// Extension methods for GraphQLException and derived exception types
/// </summary>
public static class GraphQLExceptionExtensions
{
    /// <summary>
    /// Adds multiple extensions to the exception at once
    /// </summary>
    /// <param name="exception">The GraphQL exception</param>
    /// <param name="extensions">Dictionary of extension key-value pairs</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> or <paramref name="extensions"/> is <see langword="null"/></exception>
    public static void AddExtensions(this GraphQLException exception, Dictionary<string, object> extensions)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(extensions);

        foreach (var kvp in extensions)
        {
            exception.AddExtension(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Serializes the exception extensions to JSON string
    /// </summary>
    /// <param name="exception">The GraphQL exception</param>
    /// <returns>JSON string representation of extensions, or empty string if no extensions</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/></exception>
    public static string SerializeExtensions(this GraphQLException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception.Extensions is not { Count: > 0 })
            return string.Empty;

        try
        {
            return JsonSerializer.Serialize(exception.Extensions);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the error code or returns a default value if not set
    /// </summary>
    /// <param name="exception">The GraphQL exception</param>
    /// <param name="defaultErrorCode">Default error code to return if ErrorCode is null</param>
    /// <returns>Error code or default value</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/></exception>
    public static string GetErrorCodeOrDefault(this GraphQLException exception, string defaultErrorCode = "UNKNOWN_ERROR")
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.ErrorCode ?? defaultErrorCode;
    }

    /// <summary>
    /// Creates a new GraphQLException with additional context information
    /// </summary>
    /// <param name="exception">The original exception</param>
    /// <param name="contextMessage">Additional context to include</param>
    /// <returns>New GraphQLException with combined information</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/></exception>
    public static GraphQLException WithContext(this GraphQLException exception, string contextMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (string.IsNullOrEmpty(contextMessage))
            return exception;

        var newMessage = $"{exception.Message} | Context: {contextMessage}";
        var newException = new GraphQLException(newMessage, exception.InnerException)
        {
            ErrorCode = exception.ErrorCode
        };

        foreach (var kvp in exception.Extensions)
        {
            newException.AddExtension(kvp.Key, kvp.Value);
        }

        return newException;
    }

    /// <summary>
    /// Gets a typed extension value from the exception
    /// </summary>
    /// <typeparam name="T">Type to cast the extension value to</typeparam>
    /// <param name="exception">The GraphQL exception</param>
    /// <param name="key">Extension key</param>
    /// <param name="defaultValue">Default value if key not found or cast fails</param>
    /// <returns>Typed extension value or default</returns>
    public static T GetExtension<T>(this GraphQLException exception, string key, T defaultValue = default)
    {
        if (exception is null || exception.Extensions is null)
            return defaultValue;

        return exception.Extensions.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : defaultValue;
    }

    /// <summary>
    /// Adds a formatted error code extension to help with error tracking
    /// </summary>
    /// <param name="exception">The GraphQL exception</param>
    /// <param name="errorCodePrefix">Prefix for the error code (e.g., "SCHEMA", "EXECUTION")</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorCodePrefix"/> is <see langword="null"/> or empty</exception>
    public static void AddFormattedErrorCode(this GraphQLException exception, string errorCodePrefix)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (string.IsNullOrWhiteSpace(errorCodePrefix))
            throw new ArgumentException("Error code prefix cannot be null or whitespace.", nameof(errorCodePrefix));

        var formattedCode = $"{errorCodePrefix}_{exception.GetErrorCodeOrDefault()}";
        exception.AddExtension("errorCode", formattedCode);
    }
}