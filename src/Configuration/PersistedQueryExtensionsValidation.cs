#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Validation helpers for <see cref="PersistedQueryExtensions"/> and <see cref="PersistedQueryOptions"/>.
/// </summary>
public static class PersistedQueryExtensionsValidation
{
    /// <summary>
    /// Validates the <see cref="PersistedQueryOptions"/> instance.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <returns>An enumerable of human-readable validation problems; empty when valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this PersistedQueryOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.MaxIndexSize <= 0)
            errors.Add($"MaxIndexSize must be greater than 0, but was {value.MaxIndexSize}.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="PersistedQueryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options to check.</param>
    /// <returns><c>true</c> when valid; otherwise <c>false</c>.</returns>
    public static bool IsValid(this PersistedQueryOptions? value)
        => value is not null && value.Validate() is { Count: 0 };

    /// <summary>
    /// Ensures the <see cref="PersistedQueryOptions"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the options are invalid, listing all problems.</exception>
    public static void EnsureValid(this PersistedQueryOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
            return;

        throw new ArgumentException(
            $"PersistedQueryOptions is invalid:{Environment.NewLine}- ".Replace("- ", "") +
            string.Join(Environment.NewLine + "- ", errors));
    }
}