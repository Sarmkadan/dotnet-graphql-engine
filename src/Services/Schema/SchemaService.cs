// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Exceptions;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Services.Schema;

/// <summary>
/// Service for managing GraphQL schemas
/// </summary>
public class SchemaService
{
    private readonly ILogger<SchemaService> _logger;
    private readonly Dictionary<string, GraphQLSchema> _schemas = new();

    public SchemaService(ILogger<SchemaService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates and registers a new schema
    /// </summary>
    public GraphQLSchema CreateSchema(string schemaName)
    {
        if (string.IsNullOrEmpty(schemaName))
            throw new ArgumentException("Schema name cannot be empty", nameof(schemaName));

        if (_schemas.ContainsKey(schemaName))
            throw new SchemaException($"Schema '{schemaName}' already exists");

        var schema = new GraphQLSchema(schemaName);
        _schemas[schemaName] = schema;

        _logger.LogInformation("Schema created: {SchemaName}", schemaName);
        return schema;
    }

    /// <summary>
    /// Retrieves a schema by name
    /// </summary>
    public GraphQLSchema? GetSchema(string schemaName)
    {
        _schemas.TryGetValue(schemaName, out var schema);
        return schema;
    }

    /// <summary>
    /// Adds a type to the schema
    /// </summary>
    public void AddType(string schemaName, GraphQLType type)
    {
        var schema = GetSchema(schemaName) ??
            throw new SchemaException($"Schema '{schemaName}' not found");

        schema.AddType(type);
        _logger.LogInformation("Type added to schema {SchemaName}: {TypeName}",
            schemaName, type.Name);
    }

    /// <summary>
    /// Adds a field to a type
    /// </summary>
    public void AddField(string schemaName, string typeName, GraphQLField field)
    {
        var schema = GetSchema(schemaName) ??
            throw new SchemaException($"Schema '{schemaName}' not found");

        var type = schema.GetType(typeName) ??
            throw new SchemaException($"Type '{typeName}' not found in schema '{schemaName}'");

        type.AddField(field);
        schema.IncrementVersion();

        _logger.LogInformation("Field added to type {TypeName} in schema {SchemaName}: {FieldName}",
            typeName, schemaName, field.Name);
    }

    /// <summary>
    /// Removes a type from the schema
    /// </summary>
    public bool RemoveType(string schemaName, string typeName)
    {
        var schema = GetSchema(schemaName) ??
            throw new SchemaException($"Schema '{schemaName}' not found");

        var result = schema.RemoveType(typeName);
        if (result)
            schema.IncrementVersion();

        return result;
    }

    /// <summary>
    /// Validates a schema
    /// </summary>
    public bool ValidateSchema(string schemaName, out List<string> errors)
    {
        var schema = GetSchema(schemaName) ??
            throw new SchemaException($"Schema '{schemaName}' not found");

        var isValid = schema.Validate(out errors);
        _logger.LogInformation("Schema validation result for {SchemaName}: {IsValid}",
            schemaName, isValid);

        return isValid;
    }

    /// <summary>
    /// Gets all registered schemas
    /// </summary>
    public IEnumerable<GraphQLSchema> GetAllSchemas()
    {
        return _schemas.Values.ToList();
    }

    /// <summary>
    /// Deletes a schema
    /// </summary>
    public bool DeleteSchema(string schemaName)
    {
        var removed = _schemas.Remove(schemaName);
        if (removed)
            _logger.LogInformation("Schema deleted: {SchemaName}", schemaName);

        return removed;
    }

    /// <summary>
    /// Exports a schema as SDL (Schema Definition Language)
    /// </summary>
    public string ExportAsSDL(string schemaName)
    {
        var schema = GetSchema(schemaName) ??
            throw new SchemaException($"Schema '{schemaName}' not found");

        var sdl = new System.Text.StringBuilder();
        sdl.AppendLine($"\"\"\"");
        sdl.AppendLine($"{schema.Description ?? "GraphQL Schema"}");
        sdl.AppendLine($"\"\"\"");
        sdl.AppendLine();

        // Export Query type
        if (schema.QueryType != null)
        {
            sdl.AppendLine($"type {schema.QueryType.Name} {{");
            foreach (var field in schema.QueryType.Fields)
                sdl.AppendLine($"  {field.GetSignature()}");
            sdl.AppendLine("}");
            sdl.AppendLine();
        }

        // Export Mutation type
        if (schema.MutationType != null)
        {
            sdl.AppendLine($"type {schema.MutationType.Name} {{");
            foreach (var field in schema.MutationType.Fields)
                sdl.AppendLine($"  {field.GetSignature()}");
            sdl.AppendLine("}");
            sdl.AppendLine();
        }

        // Export all types
        foreach (var type in schema.GetObjectTypes())
        {
            sdl.AppendLine($"type {type.Name} {{");
            foreach (var field in type.Fields)
                sdl.AppendLine($"  {field.GetSignature()}");
            sdl.AppendLine("}");
            sdl.AppendLine();
        }

        return sdl.ToString();
    }

    /// <summary>
    /// Gets schema introspection data
    /// </summary>
    public Dictionary<string, object> GetIntrospection(string schemaName)
    {
        var schema = GetSchema(schemaName) ??
            throw new SchemaException($"Schema '{schemaName}' not found");

        return new Dictionary<string, object>
        {
            { "__schema", new {
                schema.Name,
                schema.Description,
                types = schema.Types.Keys.ToList(),
                queryType = schema.QueryType?.Name,
                mutationType = schema.MutationType?.Name,
                subscriptionType = schema.SubscriptionType?.Name,
                directives = schema.Directives.ToList(),
                schema.Version
            } }
        };
    }
}
