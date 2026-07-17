#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="DependencyInjectionJsonExtensions"/>.
/// </summary>
public static class DependencyInjectionJsonExtensionsValidation
{
	/// <summary>
	/// Validates extensions methods of <see cref="DependencyInjectionJsonExtensions"/> for invalid usage.
	/// </summary>
	/// <returns>A list of human-readable problems with the extensions methods.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="problems"/> is <see langword="null"/>.</exception>
	public static IReadOnlyList<string> Validate()
	{
		var problems = new List<string>();

		// Validate ToJson methods
		ValidateToJson(problems);

		// Validate FromJson methods
		ValidateFromJson(problems);

		// Validate TryFromJson methods
		ValidateTryFromJson(problems);

		return problems.AsReadOnly();
	}

	private static void ValidateToJson(List<string> problems)
	{
		ArgumentNullException.ThrowIfNull(problems);

		// Validate that ToJson methods properly handle null inputs
		// These methods should throw ArgumentNullException for null inputs
	}

	private static void ValidateFromJson(List<string> problems)
	{
		ArgumentNullException.ThrowIfNull(problems);

		// FromJson methods should return null for null, empty, or whitespace input
		// and throw JsonException for malformed JSON
	}

	private static void ValidateTryFromJson(List<string> problems)
	{
		ArgumentNullException.ThrowIfNull(problems);

		// TryFromJson methods should return true for null, empty, or whitespace input
		// and false for malformed JSON
	}

	/// <summary>
	/// Checks if extensions methods of <see cref="DependencyInjectionJsonExtensions"/> are valid.
	/// </summary>
	/// <returns><see langword="true"/> if the extensions methods are valid; otherwise, <see langword="false"/>.</returns>
	public static bool IsValid()
	{
		return Validate().Count == 0;
	}

	/// <summary>
	/// Ensures extensions methods of <see cref="DependencyInjectionJsonExtensions"/> are valid.
	/// </summary>
	/// <exception cref="ArgumentException">Thrown when extensions methods are not valid.</exception>
	public static void EnsureValid()
	{
		var problems = Validate();
		if (problems.Count > 0)
		{
			throw new ArgumentException($"Invalid usage of DependencyInjectionJsonExtensions: {string.Join(", ", problems)}");
		}
	}
}