#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a complete GraphQL schema with root types and definitions
/// </summary>
sealed public class GraphQLSchema
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GraphQLType? QueryType { get; set; }
    public GraphQLType? MutationType { get; set; }
    public GraphQLType? SubscriptionType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;

    private readonly Dictionary<string, GraphQLType> _types = new();
    public IReadOnlyDictionary<string, GraphQLType> Types => _types.AsReadOnly();

    private readonly List<string> _directives = new();
    public IReadOnlyList<string> Directives => _directives.AsReadOnly();

    public GraphQLSchema()
    {
    }

    public GraphQLSchema(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Registers a type in the schema
    /// </summary>
    public void AddType(GraphQLType type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(type.Name))
            throw new ArgumentException("Type name is required", nameof(type));

        _types[type.Name] = type;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a type from the schema
    /// </summary>
    public bool RemoveType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return false;

        var removed = _types.Remove(typeName);
        if (removed) UpdatedAt = DateTime.UtcNow;
        return removed;
    }

    /// <summary>
    /// Gets a type by name
    /// </summary>
    public GraphQLType? GetType(string typeName)
    {
        _types.TryGetValue(typeName, out var type);
        return type;
    }

    /// <summary>
    /// Checks if a type exists in the schema
    /// </summary>
    public bool HasType(string typeName)
    {
        return _types.ContainsKey(typeName);
    }

    /// <summary>
    /// Gets all scalar types in the schema
    /// </summary>
    public IEnumerable<GraphQLType> GetScalarTypes()
    {
        return _types.Values.Where(t => t.Kind == GraphQLTypeKind.Scalar);
    }

    /// <summary>
    /// Gets all object types in the schema
    /// </summary>
    public IEnumerable<GraphQLType> GetObjectTypes()
    {
        return _types.Values.Where(t => t.Kind == GraphQLTypeKind.Object);
    }

    /// <summary>
    /// Gets all interface types in the schema
    /// </summary>
    public IEnumerable<GraphQLType> GetInterfaceTypes()
    {
        return _types.Values.Where(t => t.Kind == GraphQLTypeKind.Interface);
    }

    /// <summary>
    /// Gets all enum types in the schema
    /// </summary>
    public IEnumerable<GraphQLType> GetEnumTypes()
    {
        return _types.Values.Where(t => t.Kind == GraphQLTypeKind.Enum);
    }

    /// <summary>
    /// Adds a custom directive to the schema
    /// </summary>
    public void AddDirective(string directiveName)
    {
        if (string.IsNullOrEmpty(directiveName))
            throw new ArgumentException("Directive name cannot be empty", nameof(directiveName));

        if (!_directives.Contains(directiveName))
            _directives.Add(directiveName);
    }

    /// <summary>
    /// Validates the entire schema
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Schema name is required");

        if (QueryType is null)
            errors.Add("Schema must have a Query root type");

        if (QueryType is not null && !QueryType.Validate(out var queryErrors))
            errors.AddRange(queryErrors.Select(e => $"Query type error: {e}"));

        if (MutationType is not null && !MutationType.Validate(out var mutationErrors))
            errors.AddRange(mutationErrors.Select(e => $"Mutation type error: {e}"));

        if (SubscriptionType is not null && !SubscriptionType.Validate(out var subscriptionErrors))
            errors.AddRange(subscriptionErrors.Select(e => $"Subscription type error: {e}"));

        foreach (var type in _types.Values)
        {
            if (!type.Validate(out var typeErrors))
                errors.AddRange(typeErrors.Select(e => $"Type '{type.Name}' error: {e}"));
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Increments the schema version
    /// </summary>
    public void IncrementVersion()
    {
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a summary of the schema
    /// </summary>
    public string GetSummary()
    {
        return $"Schema '{Name}' v{Version}: {_types.Count} types, " +
               $"{GetObjectTypes().Count()} objects, " +
               $"{GetScalarTypes().Count()} scalars, " +
               $"{GetInterfaceTypes().Count()} interfaces, " +
               $"{GetEnumTypes().Count()} enums";
    }
}
