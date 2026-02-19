// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a GraphQL type definition with metadata and field information
/// </summary>
public class GraphQLType
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GraphQLTypeKind Kind { get; set; }
    public bool IsNullable { get; set; } = false;
    public bool IsArray { get; set; } = false;
    public string? BaseTypeName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private readonly List<GraphQLField> _fields = new();
    public IReadOnlyList<GraphQLField> Fields => _fields.AsReadOnly();

    private readonly List<string> _implements = new();
    public IReadOnlyList<string> Implements => _implements.AsReadOnly();

    public GraphQLType()
    {
    }

    public GraphQLType(string name, GraphQLTypeKind kind)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Kind = kind;
    }

    /// <summary>
    /// Adds a field to the GraphQL type definition
    /// </summary>
    public void AddField(GraphQLField field)
    {
        if (field == null) throw new ArgumentNullException(nameof(field));

        if (_fields.Any(f => f.Name == field.Name))
            throw new InvalidOperationException($"Field '{field.Name}' already exists in type '{Name}'");

        _fields.Add(field);
    }

    /// <summary>
    /// Removes a field by name
    /// </summary>
    public bool RemoveField(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return false;

        var field = _fields.FirstOrDefault(f => f.Name == fieldName);
        if (field == null) return false;

        _fields.Remove(field);
        return true;
    }

    /// <summary>
    /// Gets a field by name
    /// </summary>
    public GraphQLField? GetField(string fieldName)
    {
        return _fields.FirstOrDefault(f => f.Name == fieldName);
    }

    /// <summary>
    /// Adds an interface implementation
    /// </summary>
    public void AddInterface(string interfaceName)
    {
        if (string.IsNullOrEmpty(interfaceName))
            throw new ArgumentException("Interface name cannot be empty", nameof(interfaceName));

        if (!_implements.Contains(interfaceName))
            _implements.Add(interfaceName);
    }

    /// <summary>
    /// Validates the GraphQL type definition for completeness
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Type name is required");

        if (Kind == GraphQLTypeKind.Object && _fields.Count == 0)
            errors.Add($"Object type '{Name}' must have at least one field");

        foreach (var field in _fields)
        {
            if (!field.Validate(out var fieldErrors))
                errors.AddRange(fieldErrors);
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Gets the fully qualified type name with modifiers
    /// </summary>
    public string GetFullyQualifiedName()
    {
        var name = Name;
        if (IsArray) name += "[]";
        if (!IsNullable) name += "!";
        return name;
    }
}

/// <summary>
/// Enumeration of GraphQL type kinds
/// </summary>
public enum GraphQLTypeKind
{
    Scalar = 0,
    Object = 1,
    Interface = 2,
    Union = 3,
    Enum = 4,
    InputObject = 5,
    List = 6
}
