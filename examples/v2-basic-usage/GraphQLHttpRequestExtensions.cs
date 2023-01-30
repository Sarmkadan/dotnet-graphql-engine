using System;
using System.Collections.Generic;

namespace GraphQLEngine.Hosting;

/// <summary>
/// Provides extension methods for <see cref="GraphQLHttpRequest"/> to simplify common operations.
/// </summary>
public static class GraphQLHttpRequestExtensions
{
    /// <summary>
    /// Determines whether the request contains a non-empty query string.
    /// </summary>
    /// <param name="request">The GraphQL HTTP request to check. Cannot be null.</param>
    /// <returns>True if the request has a non-empty query; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static bool IsQuery(this GraphQLHttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return !string.IsNullOrEmpty(request.Query);
    }

    /// <summary>
    /// Safely gets the variables dictionary from the request, returning an empty dictionary if null.
    /// </summary>
    /// <param name="request">The GraphQL HTTP request containing the variables. Cannot be null.</param>
    /// <returns>A dictionary containing the variables. Never null - returns empty dictionary if Variables is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static Dictionary<string, object?> GetVariablesAsDictionary(this GraphQLHttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Variables ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Gets the operation name from the request, or returns the specified default value if null or empty.
    /// </summary>
    /// <param name="request">The GraphQL HTTP request containing the operation name. Cannot be null.</param>
    /// <param name="defaultValue">The value to return if the operation name is null or empty. Defaults to empty string.</param>
    /// <returns>The operation name if present; otherwise, the default value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static string GetOperationNameOrDefault(this GraphQLHttpRequest request, string defaultValue = "")
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.OperationName ?? defaultValue;
    }
}
