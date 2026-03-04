#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a single field within a GraphQL type
/// </summary>
sealed public class GraphQLField
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ReturnType { get; set; } = string.Empty;
    public bool IsNullable { get; set; } = false;
    public int Complexity { get; set; } = 1;
    public bool IsDeprecated { get; set; } = false;
    public string? DeprecationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private readonly Dictionary<string, GraphQLArgument> _arguments = new();
    public IReadOnlyDictionary<string, GraphQLArgument> Arguments => _arguments.AsReadOnly();

    private readonly List<string> _directives = new();
    public IReadOnlyList<string> Directives => _directives.AsReadOnly();

    public GraphQLField()
    {
    }

    public GraphQLField(string name, string returnType)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
    }

    /// <summary>
    /// Adds an argument to this field
    /// </summary>
    public void AddArgument(GraphQLArgument argument)
    {
        if (argument is null) throw new ArgumentNullException(nameof(argument));

        if (_arguments.ContainsKey(argument.Name))
            throw new InvalidOperationException($"Argument '{argument.Name}' already exists");

        _arguments[argument.Name] = argument;
    }

    /// <summary>
    /// Removes an argument by name
    /// </summary>
    public bool RemoveArgument(string argumentName)
    {
        return _arguments.Remove(argumentName);
    }

    /// <summary>
    /// Gets an argument by name
    /// </summary>
    public GraphQLArgument? GetArgument(string argumentName)
    {
        _arguments.TryGetValue(argumentName, out var arg);
        return arg;
    }

    /// <summary>
    /// Adds a directive to this field
    /// </summary>
    public void AddDirective(string directiveName)
    {
        if (string.IsNullOrEmpty(directiveName))
            throw new ArgumentException("Directive name cannot be empty", nameof(directiveName));

        if (!_directives.Contains(directiveName))
            _directives.Add(directiveName);
    }

    /// <summary>
    /// Removes a directive by name
    /// </summary>
    public bool RemoveDirective(string directiveName)
    {
        return _directives.Remove(directiveName);
    }

    /// <summary>
    /// Marks the field as deprecated with optional reason
    /// </summary>
    public void Deprecate(string? reason = null)
    {
        IsDeprecated = true;
        DeprecationReason = reason;
    }

    /// <summary>
    /// Validates the field definition
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Field name is required");

        if (string.IsNullOrWhiteSpace(ReturnType))
            errors.Add("Field return type is required");

        if (Complexity < 0)
            errors.Add("Field complexity cannot be negative");

        foreach (var arg in _arguments.Values)
        {
            if (!arg.Validate(out var argErrors))
                errors.AddRange(argErrors);
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Gets the complete field signature as it appears in GraphQL
    /// </summary>
    public string GetSignature()
    {
        var args = _arguments.Count > 0
            ? $"({string.Join(", ", _arguments.Values.Select(a => a.GetSignature()))})"
            : string.Empty;

        return $"{Name}{args}: {ReturnType}";
    }
}

/// <summary>
/// Represents a field argument in GraphQL
/// </summary>
sealed public class GraphQLArgument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public object? DefaultValue { get; set; }
    public string? Description { get; set; }

    public GraphQLArgument()
    {
    }

    public GraphQLArgument(string name, string type, bool isRequired = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        IsRequired = isRequired;
    }

    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Argument name is required");

        if (string.IsNullOrWhiteSpace(Type))
            errors.Add("Argument type is required");

        return errors.Count == 0;
    }

    public string GetSignature() => $"{Name}: {Type}{(IsRequired ? "!" : "")}";
}
