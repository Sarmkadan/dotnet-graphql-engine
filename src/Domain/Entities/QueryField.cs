#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Text.Json.Serialization;
using GraphQLEngine.Common.Utilities;

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a selected field in a GraphQL query, including its nested selections.
/// This is used for query execution and complexity analysis.
/// </summary>
public class QueryField
{
    public string Name { get; private set; }
    public string? Alias { get; private set; }
    public string? TypeCondition { get; private set; } // For inline fragments (e.g., "... on User")
    public IReadOnlyList<QueryArgument> Arguments { get; private set; }
    public IReadOnlyList<QueryField> Fields { get; private set; } // Nested selections

    [JsonConstructor]
public QueryField(
        string name,
        string? alias = null,
        string? typeCondition = null,
        IEnumerable<QueryArgument>? arguments = null,
        IEnumerable<QueryField>? fields = null)
    {
        Name = name;
        Alias = alias;
        TypeCondition = typeCondition;
        Arguments = arguments?.ToList().AsReadOnly() ?? (IReadOnlyList<QueryArgument>)Array.Empty<QueryArgument>();
        Fields = fields?.ToList().AsReadOnly() ?? (IReadOnlyList<QueryField>)Array.Empty<QueryField>();
    }
}

/// <summary>
/// Represents an argument for a selected field in a GraphQL query.
/// </summary>
public class QueryArgument
{
    /// <summary>
    /// Gets the name of the argument. Must be a valid GraphQL name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the value of the argument.
    /// </summary>
    public object? Value { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryArgument"/> class.
    /// </summary>
    /// <param name="name">The name of the argument (must be a valid GraphQL name).</param>
    /// <param name="value">The value of the argument.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or contains invalid characters.</exception>
    public QueryArgument(string name, object? value)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Argument name cannot be null, empty, or whitespace.", nameof(name));
        }

        if (name.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("Argument name cannot contain whitespace characters.", nameof(name));
        }

        if (!name.IsValidGraphQLName())
        {
            throw new ArgumentException(
                "Argument name must be a valid GraphQL name (start with letter/underscore, followed by letters/digits/underscores).",
                nameof(name));
        }

        Name = name;
        Value = value;
    }
}
