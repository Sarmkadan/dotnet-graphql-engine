// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a parsed GraphQL query ready for execution
/// </summary>
public class GraphQLQuery
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string QueryString { get; set; } = string.Empty;
    public string? OperationName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long ExecutionTimeMs { get; set; } = 0;
    public bool IsValid { get; set; } = true;

    private readonly Dictionary<string, object?> _variables = new();
    public IReadOnlyDictionary<string, object?> Variables => _variables.AsReadOnly();

    private readonly List<string> _errors = new();
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    private readonly List<string> _selectedFields = new();
    public IReadOnlyList<string> SelectedFields => _selectedFields.AsReadOnly();

    public GraphQLQuery()
    {
    }

    public GraphQLQuery(string queryString)
    {
        QueryString = queryString ?? throw new ArgumentNullException(nameof(queryString));
    }

    /// <summary>
    /// Sets a variable for query execution
    /// </summary>
    public void SetVariable(string name, object? value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be empty", nameof(name));

        _variables[name] = value;
    }

    /// <summary>
    /// Gets a variable value
    /// </summary>
    public object? GetVariable(string name)
    {
        _variables.TryGetValue(name, out var value);
        return value;
    }

    /// <summary>
    /// Removes a variable
    /// </summary>
    public bool RemoveVariable(string name)
    {
        return _variables.Remove(name);
    }

    /// <summary>
    /// Clears all variables
    /// </summary>
    public void ClearVariables()
    {
        _variables.Clear();
    }

    /// <summary>
    /// Adds a parsing/validation error
    /// </summary>
    public void AddError(string error)
    {
        if (string.IsNullOrEmpty(error)) return;

        _errors.Add(error);
        IsValid = false;
    }

    /// <summary>
    /// Clears all errors
    /// </summary>
    public void ClearErrors()
    {
        _errors.Clear();
        IsValid = true;
    }

    /// <summary>
    /// Adds a selected field name to the execution plan
    /// </summary>
    public void AddSelectedField(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return;

        if (!_selectedFields.Contains(fieldName))
            _selectedFields.Add(fieldName);
    }

    /// <summary>
    /// Checks if the query is well-formed
    /// </summary>
    public bool Validate()
    {
        _errors.Clear();

        if (string.IsNullOrWhiteSpace(QueryString))
            _errors.Add("Query string cannot be empty");

        if (QueryString.Length > 100000)
            _errors.Add("Query exceeds maximum length of 100000 characters");

        // Basic syntax validation
        var openBraces = QueryString.Count(c => c == '{');
        var closeBraces = QueryString.Count(c => c == '}');

        if (openBraces != closeBraces)
            _errors.Add("Mismatched braces in query");

        IsValid = _errors.Count == 0;
        return IsValid;
    }

    /// <summary>
    /// Gets the query type (Query, Mutation, Subscription)
    /// </summary>
    public string GetQueryType()
    {
        var trimmed = QueryString.Trim();
        if (trimmed.StartsWith("mutation", StringComparison.OrdinalIgnoreCase)) return "mutation";
        if (trimmed.StartsWith("subscription", StringComparison.OrdinalIgnoreCase)) return "subscription";
        return "query";
    }

    /// <summary>
    /// Gets the depth of nested selections in the query
    /// </summary>
    public int GetQueryDepth()
    {
        var depth = 0;
        var maxDepth = 0;
        var inString = false;
        var escapeNext = false;

        foreach (var c in QueryString)
        {
            if (escapeNext)
            {
                escapeNext = false;
                continue;
            }

            if (c == '\\')
            {
                escapeNext = true;
                continue;
            }

            if (c == '"' && !escapeNext)
                inString = !inString;

            if (!inString)
            {
                if (c == '{')
                {
                    depth++;
                    maxDepth = Math.Max(maxDepth, depth);
                }
                else if (c == '}')
                    depth--;
            }
        }

        return maxDepth;
    }
}
