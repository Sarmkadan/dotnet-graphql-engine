#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a selected field in a GraphQL query, including its nested selections.
/// This is used for query execution and complexity analysis.
/// </summary>
public class QueryField
{
    public string Name { get; }
    public string? Alias { get; }
    public string? TypeCondition { get; } // For inline fragments (e.g., "... on User")
    public IReadOnlyList<QueryArgument> Arguments { get; }
    public IReadOnlyList<QueryField> Fields { get; } // Nested selections

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
    public string Name { get; }
    public object? Value { get; } // Parsed value of the argument

    public QueryArgument(string name, object? value)
    {
        Name = name;
        Value = value;
    }
}
