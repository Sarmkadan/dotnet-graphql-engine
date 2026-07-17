using System;
using System.Collections.Generic;
using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides validation helpers for string operations.
/// </summary>
public static class StringExtensionsValidation
{
	/// <summary>
	/// Validates a string based on rules relevant to the operations in <see cref="StringExtensions"/>.
	/// </summary>
	/// <param name="value">The string to validate.</param>
	/// <returns>A list of problems found, or an empty list if valid.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = new List<string>();

		if (string.IsNullOrWhiteSpace(value))
		{
			problems.Add("String cannot be null, empty, or whitespace.");
		}

		return problems;
	}

	/// <summary>
	/// Determines if a string is valid based on rules relevant to the operations in <see cref="StringExtensions"/>.
	/// </summary>
	/// <param name="value">The string to validate.</param>
	/// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static bool IsValid(this string value) => value.Validate().Count == 0;

	/// <summary>
	/// Ensures that the string is valid, throwing an exception if it is not.
	/// </summary>
	/// <param name="value">The string to validate.</param>
	/// <exception cref="ArgumentException">Thrown when the string is invalid.</exception>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static void EnsureValid(this string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = value.Validate();
		if (problems.Count > 0)
		{
			throw new ArgumentException(string.Join("; ", problems), nameof(value));
		}
	}
}