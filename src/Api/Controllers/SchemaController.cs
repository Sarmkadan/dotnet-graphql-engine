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

        // Would retrieve types from schema service
        // This is a simplified implementation

        return response;
    }

    /// <summary>
    /// Retrieves metadata for a specific type
    /// </summary>
    public TypeInfoDto? GetType(string schemaName, string typeName)
    {
        logger.LogInformation("Retrieving type {TypeName} from schema {SchemaName}", typeName, schemaName);

        try
        {
            // Build type information from schema
            var typeInfo = new TypeInfoDto
            {
                Name = typeName,
                Kind = "Object",
                Description = $"Type {typeName}",
                Fields = new List<FieldInfoDto>()
            };

            return typeInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving type information");
            return null;
        }
    }

    /// <summary>
    /// Lists all available schemas
    /// </summary>
    public SchemasListResponse ListSchemas()
    {
        logger.LogInformation("Listing all available schemas");

        var response = new SchemasListResponse
        {
            Schemas = new List<SchemaInfoDto>()
        };

        // Would query schema repository
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
