#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Services.Schema;
using GraphQLEngine.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Api.Controllers;

/// <summary>
/// Schema management and introspection endpoint
/// Allows inspection of schema structure and modifications
/// </summary>
sealed public class SchemaController
{
    private readonly SchemaService _schemaService;
    private readonly ILogger<SchemaController> logger;

    public SchemaController(SchemaService schemaService, ILogger<SchemaController> logger)
    {
        _schemaService = schemaService ?? throw new ArgumentNullException(nameof(schemaService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the schema definition in SDL format
    /// </summary>
    public string GetSchemaAsSDL(string schemaName)
    {
        logger.LogInformation("Retrieving SDL for schema {SchemaName}", schemaName);

        try
        {
            var sdl = _schemaService.ExportAsSDL(schemaName);
            return sdl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving schema SDL");
            throw;
        }
    }

    /// <summary>
    /// Retrieves all types defined in the schema
    /// </summary>
    public SchemaTypesResponse GetAllTypes(string schemaName)
    {
        logger.LogInformation("Retrieving all types for schema {SchemaName}", schemaName);

        var response = new SchemaTypesResponse
        {
            SchemaName = schemaName,
            Types = new List<TypeInfoDto>()
        };

        var schema = _schemaService.GetSchema(schemaName);
        if (schema is null)
        {
            logger.LogWarning("Schema {SchemaName} not found", schemaName);
            return response;
        }

        response.Types.AddRange(schema.Types.Values.Select(MapType));
        return response;
    }

    /// <summary>
    /// Retrieves metadata for a specific type
    /// </summary>
    public TypeInfoDto? GetType(string schemaName, string typeName)
    {
        logger.LogInformation("Retrieving type {TypeName} from schema {SchemaName}", typeName, schemaName);

        var schema = _schemaService.GetSchema(schemaName);
        var type = schema?.GetType(typeName);
        if (type is null)
        {
            logger.LogWarning("Type {TypeName} not found in schema {SchemaName}", typeName, schemaName);
            return null;
        }

        return MapType(type);
    }

    /// <summary>
    /// Maps a domain type to its transfer object representation
    /// </summary>
    private static TypeInfoDto MapType(Domain.Entities.GraphQLType type)
    {
        return new TypeInfoDto
        {
            Name = type.Name,
            Kind = type.Kind.ToString(),
            Description = type.Description,
            PossibleTypes = type.Implements.ToList(),
            Fields = type.Fields.Select(f => new FieldInfoDto
            {
                Name = f.Name,
                Type = f.ReturnType,
                Description = f.Description,
                IsDeprecated = f.IsDeprecated,
                DeprecationReason = f.DeprecationReason,
                Arguments = f.Arguments.Values.Select(a => new ArgumentInfoDto
                {
                    Name = a.Name,
                    Type = a.Type,
                    DefaultValue = a.DefaultValue?.ToString()
                }).ToList()
            }).ToList()
        };
    }

    /// <summary>
    /// Lists all available schemas
    /// </summary>
    public SchemasListResponse ListSchemas()
    {
        logger.LogInformation("Listing all available schemas");

        var response = new SchemasListResponse
        {
            Schemas = _schemaService.GetAllSchemas().Select(s => new SchemaInfoDto
            {
                Name = s.Name,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                ModifiedAt = s.UpdatedAt,
                TypeCount = s.Types.Count
            }).ToList()
        };

        logger.LogInformation("Found {Count} schemas", response.Schemas.Count);

        return response;
    }

    /// <summary>
    /// Validates a GraphQL query against the schema
    /// </summary>
    public QueryValidationResponse ValidateQuery(string schemaName, string query)
    {
        logger.LogInformation("Validating query against schema {SchemaName}", schemaName);

        var response = new QueryValidationResponse
        {
            IsValid = true,
            Query = query,
            Errors = new List<string>()
        };

        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                response.IsValid = false;
                response.Errors.Add("Query cannot be empty");
                return response;
            }

            if (_schemaService.GetSchema(schemaName) is null)
            {
                response.IsValid = false;
                response.Errors.Add($"Schema '{schemaName}' does not exist");
            }

            var braceDepth = 0;
            var parenDepth = 0;
            foreach (var c in query)
            {
                switch (c)
                {
                    case '{': braceDepth++; break;
                    case '}': braceDepth--; break;
                    case '(': parenDepth++; break;
                    case ')': parenDepth--; break;
                }

                if (braceDepth < 0 || parenDepth < 0)
                    break;
            }

            if (braceDepth != 0)
            {
                response.IsValid = false;
                response.Errors.Add("Unbalanced braces in query");
            }

            if (parenDepth != 0)
            {
                response.IsValid = false;
                response.Errors.Add("Unbalanced parentheses in query");
            }

            if (!query.Contains('{'))
            {
                response.IsValid = false;
                response.Errors.Add("Query must contain a selection set");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating query");
            response.IsValid = false;
            response.Errors.Add($"Validation error: {ex.Message}");
        }

        return response;
    }
}

/// <summary>
/// Response model for schema types listing
/// </summary>
sealed public class SchemaTypesResponse
{
    public string SchemaName { get; set; } = string.Empty;
    public List<TypeInfoDto> Types { get; set; } = new();
}

/// <summary>
/// Type information data transfer object
/// </summary>
sealed public class TypeInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<FieldInfoDto> Fields { get; set; } = new();
    public List<string> PossibleTypes { get; set; } = new();
}

/// <summary>
/// Field information data transfer object
/// </summary>
sealed public class FieldInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeprecated { get; set; }
    public string? DeprecationReason { get; set; }
    public List<ArgumentInfoDto> Arguments { get; set; } = new();
}

/// <summary>
/// Argument information data transfer object
/// </summary>
sealed public class ArgumentInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Schemas listing response
/// </summary>
sealed public class SchemasListResponse
{
    public List<SchemaInfoDto> Schemas { get; set; } = new();
}

/// <summary>
/// Schema information data transfer object
/// </summary>
sealed public class SchemaInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int TypeCount { get; set; }
}

/// <summary>
/// Query validation response
/// </summary>
sealed public class QueryValidationResponse
{
    public bool IsValid { get; set; }
    public string Query { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public int ErrorCount => Errors.Count;
}
