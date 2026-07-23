#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Provides extension methods for <see cref="QueryArgument"/> instances.
/// </summary>
public static class QueryArgumentExtensions
{
    /// <summary>
    /// Creates a new QueryArgument with the same name but a different value.
    /// </summary>
    /// <param name="argument">The original QueryArgument.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>A new QueryArgument instance with the updated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    public static QueryArgument WithValue(this QueryArgument argument, object? newValue)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return new QueryArgument(argument.Name, newValue);
    }

    /// <summary>
    /// Creates a new QueryArgument with a different name.
    /// </summary>
    /// <param name="argument">The original QueryArgument.</param>
    /// <param name="newName">The new name to set.</param>
    /// <returns>A new QueryArgument instance with the updated name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="newName"/> is null or empty.</exception>
    public static QueryArgument WithName(this QueryArgument argument, string newName)
    {
        ArgumentNullException.ThrowIfNull(argument);
        ArgumentException.ThrowIfNullOrEmpty(newName);
        return new QueryArgument(newName, argument.Value);
    }

    /// <summary>
    /// Checks if two QueryArgument instances are equal based on their name and value.
    /// </summary>
    /// <param name="argument">The first QueryArgument.</param>
    /// <param name="other">The second QueryArgument to compare with.</param>
    /// <returns>True if both name and value are equal; otherwise false.</returns>
    public static bool Equals(this QueryArgument argument, QueryArgument? other)
    {
        if (argument is null && other is null)
            return true;
        if (argument is null || other is null)
            return false;

        return string.Equals(argument.Name, other.Name, StringComparison.Ordinal)
            && Equals(argument.Value, other.Value);
    }

    /// <summary>
    /// Gets the string representation of the argument value.
    /// </summary>
    /// <param name="argument">The QueryArgument.</param>
    /// <returns>A string representation of the value, or "null" if the value is null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    public static string GetValueAsString(this QueryArgument argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return argument.Value?.ToString() ?? "null";
    }

    /// <summary>
    /// Determines whether the argument has a non-null value.
    /// </summary>
    /// <param name="argument">The QueryArgument.</param>
    /// <returns>True if the value is not null; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    public static bool HasValue(this QueryArgument argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return argument.Value != null;
    }

    /// <summary>
    /// Determines whether the argument has a null value.
    /// </summary>
    /// <param name="argument">The QueryArgument.</param>
    /// <returns>True if the value is null; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    public static bool IsNullValue(this QueryArgument argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return argument.Value == null;
    }

    /// <summary>
    /// Creates a deep copy of the QueryArgument.
    /// </summary>
    /// <param name="argument">The QueryArgument to copy.</param>
    /// <returns>A new QueryArgument instance with the same name and value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    public static QueryArgument DeepCopy(this QueryArgument argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return new QueryArgument(argument.Name, argument.Value);
    }

    /// <summary>
    /// Finds an argument by name in a collection of QueryArguments.
    /// </summary>
    /// <param name="arguments">The collection of QueryArguments to search.</param>
    /// <param name="name">The name of the argument to find.</param>
    /// <returns>The found QueryArgument, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public static QueryArgument? FindByName(this IEnumerable<QueryArgument> arguments, string name)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentException.ThrowIfNullOrEmpty(name);

        return arguments.FirstOrDefault(arg => string.Equals(arg.Name, name, StringComparison.Ordinal));
    }

    /// <summary>
    /// Determines whether a collection of QueryArguments contains an argument with the specified name.
    /// </summary>
    /// <param name="arguments">The collection of QueryArguments to search.</param>
    /// <param name="name">The name of the argument to check for.</param>
    /// <returns>True if an argument with the specified name exists; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public static bool ContainsName(this IEnumerable<QueryArgument> arguments, string name)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentException.ThrowIfNullOrEmpty(name);

        return arguments.Any(arg => string.Equals(arg.Name, name, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets the value of an argument as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="argument">The QueryArgument.</param>
    /// <returns>The value cast to type T, or default(T) if the value is null or of a different type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    public static T? GetValue<T>(this QueryArgument argument)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return argument.Value is T value ? value : default;
    }

    /// <summary>
    /// Gets the value of an argument as a specific type, with a default value if null or of wrong type.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="argument">The QueryArgument.</param>
    /// <param name="defaultValue">The default value to return if the value is null or of a different type.</param>
    /// <returns>The value cast to type T, or the default value if conversion fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    public static T GetValueOrDefault<T>(this QueryArgument argument, T defaultValue = default!)
    {
        ArgumentNullException.ThrowIfNull(argument);
        return argument.Value is T value ? value : defaultValue;
    }
}