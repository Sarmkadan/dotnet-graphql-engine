// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.Entities;
using Microsoft.Extensions.Logging;
using ExecutionContext = GraphQLEngine.Domain.Entities.ExecutionContext;

namespace GraphQLEngine.Services.GraphQL;

/// <summary>
/// Service for executing GraphQL queries and mutations
/// </summary>
public class GraphQLExecutionService
{
    private readonly ILogger<GraphQLExecutionService> _logger;
    private readonly Dictionary<string, object> _resolvers = new();

    public GraphQLExecutionService(ILogger<GraphQLExecutionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a resolver function for a field
    /// </summary>
    public void RegisterResolver(string fieldPath, object resolver)
    {
        if (string.IsNullOrEmpty(fieldPath))
            throw new ArgumentException("Field path cannot be empty", nameof(fieldPath));

        if (resolver == null) throw new ArgumentNullException(nameof(resolver));

        _resolvers[fieldPath] = resolver;
        _logger.LogInformation("Resolver registered for field: {FieldPath}", fieldPath);
    }

    /// <summary>
    /// Executes a GraphQL query
    /// </summary>
    public async Task<ExecutionContext> ExecuteAsync(GraphQLQuery query)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));

        var context = new ExecutionContext(query.Id);
        context.RequestedFieldCount = query.SelectedFields.Count;

        try
        {
            _logger.LogInformation("Starting execution of query: {QueryId}", query.Id);

            if (!query.Validate())
            {
                foreach (var error in query.Errors)
                    context.AddError(error);
                context.Complete();
                return context;
            }

            // Parse and execute the query
            var result = await ExecuteQueryInternalAsync(query, context);

            context.Complete();
            _logger.LogInformation("Query execution completed: {QueryId}, Duration: {Duration}ms",
                query.Id, context.DurationMs);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {QueryId}", query.Id);
            context.Fail($"Execution failed: {ex.Message}");
            return context;
        }
    }

    /// <summary>
    /// Internal query execution logic
    /// </summary>
    private async Task<object?> ExecuteQueryInternalAsync(GraphQLQuery query, ExecutionContext context)
    {
        var selections = ParseSelections(query.QueryString);

        foreach (var field in selections)
        {
            try
            {
                var resolverKey = field;
                if (_resolvers.TryGetValue(resolverKey, out var resolver))
                {
                    // Simulate resolver execution
                    context.RecordResolverExecution(field);
                    await Task.Delay(10); // Simulate async work
                }
                else
                {
                    context.AddError($"No resolver found for field: {field}", field);
                }
            }
            catch (Exception ex)
            {
                context.AddError(ex.Message, field);
            }
        }

        return true;
    }

    /// <summary>
    /// Parses field selections from query string
    /// </summary>
    private List<string> ParseSelections(string queryString)
    {
        var fields = new List<string>();
        var inBraces = false;
        var currentField = string.Empty;

        foreach (var c in queryString)
        {
            if (c == '{')
            {
                inBraces = true;
                continue;
            }

            if (c == '}')
            {
                if (!string.IsNullOrWhiteSpace(currentField))
                    fields.Add(currentField.Trim());
                inBraces = false;
                currentField = string.Empty;
                continue;
            }

            if (inBraces && (c == ' ' || c == '\n' || c == '\r' || c == ','))
            {
                if (!string.IsNullOrWhiteSpace(currentField))
                    fields.Add(currentField.Trim());
                currentField = string.Empty;
            }
            else if (inBraces)
            {
                currentField += c;
            }
        }

        return fields.Distinct().ToList();
    }

    /// <summary>
    /// Gets execution statistics
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "RegisteredResolvers", _resolvers.Count },
            { "Timestamp", DateTime.UtcNow }
        };
    }
}
