#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.Schema;
using GraphQLEngine.Formatters;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GraphQLEngine.Api.Controllers;

/// <summary>
/// Main GraphQL query and mutation endpoint controller
/// Handles incoming GraphQL requests and returns formatted responses
/// </summary>
sealed public class GraphQLController
{
    private readonly GraphQLExecutionService _executionService;
    private readonly SchemaService _schemaService;
    private readonly ILogger<GraphQLController> _logger;
    private readonly JsonOutputFormatter _jsonFormatter;

    public GraphQLController(
        GraphQLExecutionService executionService,
        SchemaService schemaService,
        ILogger<GraphQLController> logger,
        JsonOutputFormatter jsonFormatter)
    {
        _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
        _schemaService = schemaService ?? throw new ArgumentNullException(nameof(schemaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonFormatter = jsonFormatter ?? throw new ArgumentNullException(nameof(jsonFormatter));
    }

    /// <summary>
    /// Executes a GraphQL query and returns the result
    /// </summary>
    public async Task<GraphQLResponse> ExecuteQueryAsync(GraphQLRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            ValidateRequest(request);

            _logger.LogInformation(
                "Executing GraphQL query {OperationName} from schema {SchemaName}",
                request.OperationName ?? "Anonymous",
                request.SchemaName);

            // Parse and build query
            var query = new GraphQLQuery(request.Query);

            if (!string.IsNullOrEmpty(request.OperationName))
                query.OperationName = request.OperationName;

            // Add variables if provided
            if (request.Variables is not null)
            {
                foreach (var variable in request.Variables)
                    query.SetVariable(variable.Key, variable.Value);
            }

            // Execute the query
            var executionContext = await _executionService.ExecuteAsync(query);
            stopwatch.Stop();

            _logger.LogInformation(
                "Query execution completed in {DurationMs}ms with {ErrorCount} errors",
                stopwatch.ElapsedMilliseconds,
                executionContext.Errors.Count);

            // Format the response
            return new GraphQLResponse
            {
                Data = null,
                Errors = executionContext.Errors.Select(e => new GraphQLError
                {
                    Message = e.Message
                }).ToList(),
                Extensions = new Dictionary<string, object>
                {
                    { "durationMs", stopwatch.ElapsedMilliseconds },
                    { "executedAt", DateTime.UtcNow },
                    { "queryId", query.Id }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing GraphQL query");
            stopwatch.Stop();

            return new GraphQLResponse
            {
                Errors = new List<GraphQLError>
                {
                    new GraphQLError
                    {
                        Message = "Internal server error",
                        Code = "INTERNAL_ERROR"
                    }
                },
                Extensions = new Dictionary<string, object>
                {
                    { "durationMs", stopwatch.ElapsedMilliseconds }
                }
            };
        }
    }

    /// <summary>
    /// Executes a batch of GraphQL queries for improved performance
    /// </summary>
    public async Task<List<GraphQLResponse>> ExecuteBatchAsync(List<GraphQLRequest> requests)
    {
        if (requests is null || requests.Count == 0)
            throw new ArgumentException("Requests batch cannot be empty", nameof(requests));

        if (requests.Count > 10)
            throw new ArgumentException("Batch size cannot exceed 10 requests", nameof(requests));

        _logger.LogInformation("Executing batch of {Count} GraphQL queries", requests.Count);

        var responses = new List<GraphQLResponse>();
        var tasks = requests.Select(req => ExecuteQueryAsync(req)).ToList();

        var results = await Task.WhenAll(tasks);
        responses.AddRange(results);

        return responses;
    }

    /// <summary>
    /// Validates the incoming GraphQL request for basic constraints
    /// </summary>
    private void ValidateRequest(GraphQLRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("Query cannot be empty", nameof(request.Query));

        if (request.Query.Length > 100000)
            throw new ArgumentException("Query size exceeds maximum allowed (100KB)", nameof(request.Query));

        if (!string.IsNullOrEmpty(request.SchemaName) && !Regex.IsMatch(request.SchemaName, @"^[a-zA-Z0-9_-]+$"))
            throw new ArgumentException("Invalid schema name format", nameof(request.SchemaName));
    }
}

/// <summary>
/// Represents an incoming GraphQL request
/// </summary>
sealed public class GraphQLRequest
{
    public string Query { get; set; } = string.Empty;
    public string? OperationName { get; set; }
    public string SchemaName { get; set; } = "default";
    public Dictionary<string, object>? Variables { get; set; }
}

/// <summary>
/// Represents a GraphQL execution response
/// </summary>
sealed public class GraphQLResponse
{
    public object? Data { get; set; }
    public List<GraphQLError> Errors { get; set; } = new();
    public Dictionary<string, object>? Extensions { get; set; }

    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this);
}

/// <summary>
/// Represents an error in GraphQL execution
/// </summary>
sealed public class GraphQLError
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }
    public List<string>? Path { get; set; }
}
