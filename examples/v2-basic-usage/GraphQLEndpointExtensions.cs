#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GraphQLEngine.Hosting;

/// <summary>
/// Request payload for the POST /graphql endpoint.
/// </summary>
public sealed class GraphQLHttpRequest
{
    public string Query { get; set; } = string.Empty;
    public string? OperationName { get; set; }
    public Dictionary<string, object?>? Variables { get; set; }
}

/// <summary>
/// Minimal-API endpoint mapping extensions that wire the GraphQL engine
/// services (execution, schema, health) to HTTP endpoints.
/// </summary>
public static class GraphQLEndpointExtensions
{
    /// <summary>
    /// Maps POST /graphql to the GraphQL execution pipeline.
    /// </summary>
    public static IEndpointRouteBuilder MapGraphQL(this IEndpointRouteBuilder app, string pattern = "/graphql")
    {
        app.MapPost(pattern, async (GraphQLHttpRequest request, GraphQLExecutionService executionService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Results.BadRequest(new { errors = new[] { new { message = "Query cannot be empty" } } });
            }

            var query = new GraphQLQuery(request.Query)
            {
                OperationName = request.OperationName
            };

            var context = await executionService.ExecuteAsync(query);

            var response = new
            {
                data = context.HasErrors ? null : (object)new
                {
                    executionId = context.Id,
                    state = context.State.ToString(),
                    resolvedFields = context.ResolvedFieldCount,
                    durationMs = context.DurationMs
                },
                errors = context.HasErrors
                    ? context.Errors.Select(e => new { message = e.Message, field = e.Field, line = e.LineNumber })
                    : null
            };

            return context.HasErrors ? Results.Json(response, statusCode: StatusCodes.Status400BadRequest) : Results.Json(response);
        });

        return app;
    }

    /// <summary>
    /// Maps GET /graphql/schema returning the SDL export of registered schemas.
    /// </summary>
    public static IEndpointRouteBuilder MapGraphQLSchema(this IEndpointRouteBuilder app, string pattern = "/graphql/schema")
    {
        app.MapGet(pattern, (SchemaService schemaService, string? name) =>
        {
            if (!string.IsNullOrEmpty(name))
            {
                var schema = schemaService.GetSchema(name);
                if (schema is null)
                {
                    return Results.NotFound(new { error = $"Schema '{name}' not found" });
                }

                return Results.Text(schemaService.ExportAsSDL(name), "text/plain");
            }

            var schemas = schemaService.GetAllSchemas().ToList();
            if (schemas.Count == 0)
            {
                return Results.Text(string.Empty, "text/plain");
            }

            var sdl = string.Join(
                Environment.NewLine + Environment.NewLine,
                schemas.Select(s => schemaService.ExportAsSDL(s.Name)));

            return Results.Text(sdl, "text/plain");
        });

        return app;
    }

    /// <summary>
    /// Maps GET /health returning a simple liveness payload.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthCheck(this IEndpointRouteBuilder app, string pattern = "/health")
    {
        app.MapGet(pattern, () => Results.Json(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        }));

        return app;
    }
}
