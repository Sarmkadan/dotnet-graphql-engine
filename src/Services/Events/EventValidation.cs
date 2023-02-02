#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GraphQLEngine.Services.Events;

/// <summary>
/// Provides validation helpers for <see cref="Event"/> instances.
/// </summary>
public static class EventValidation
{
    /// <summary>
    /// Validates the supplied <see cref="Event"/> and returns a read‑only list of human‑readable problems.
    /// </summary>
    /// <param name="value">The event to validate.</param>
    /// <returns>A read‑only list of validation error messages. The list is empty when the event is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this Event value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Id must be a non‑empty GUID string.
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("Id must be a non‑empty string.");
        }

        // Timestamp must not be the default value.
        if (value.Timestamp == default)
        {
            problems.Add("Timestamp must be set to a non‑default value.");
        }

        // Source, if supplied, must not be empty/whitespace.
        if (value.Source is not null && string.IsNullOrWhiteSpace(value.Source))
        {
            problems.Add("Source, when provided, cannot be empty or whitespace.");
        }

        // Metadata dictionary must be instantiated.
        if (value.Metadata is null)
        {
            problems.Add("Metadata dictionary must not be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="Event"/> is valid.
    /// </summary>
    /// <param name="value">The event to check.</param>
    /// <returns><c>true</c> if the event has no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this Event value) => !value.Validate().Any();

    /// <summary>
    /// Ensures that the supplied <see cref="Event"/> is valid.
    /// </summary>
    /// <param name="value">The event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the event fails validation; the exception message contains a semicolon‑separated list of problems.</exception>
    public static void EnsureValid(this Event value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            var message = string.Join("; ", problems);
            throw new ArgumentException(message, nameof(value));
        }
    }
}
