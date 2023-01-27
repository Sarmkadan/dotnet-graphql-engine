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
    /// <summary>
    /// Gets or sets the GraphQL query string. Required.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation name to execute. Optional.
    /// </summary>
    public string? OperationName { get; set; }

    /// <summary>
    /// Gets or sets the variables dictionary for the query. Optional.
    /// </summary>
    public Dictionary<string, object?>? Variables { get; set; }
}

/// <summary>
/// Minimal-API endpoint mapping extensions that wire the GraphQL engine
/// services (execution, schema, health) to HTTP endpoints.
/// </summary>
/// <remarks>
/// This class provides extension methods for <see cref="IEndpointRouteBuilder"/> to map GraphQL endpoints
/// including query execution, schema introspection, and health checks.
/// </remarks>
public static class GraphQLEndpointExtensions
{
    /// <summary>
    /// Maps POST /graphql to the GraphQL execution pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> instance.</param>
    /// <param name="pattern">The URL pattern for the endpoint. Defaults to "/graphql".</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/></exception>
    public static IEndpointRouteBuilder MapGraphQL(this IEndpointRouteBuilder app, string pattern = "/graphql")
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern, nameof(pattern));

        app.MapPost(pattern, async (GraphQLHttpRequest request, GraphQLExecutionService executionService) =>
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(executionService);

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return Results.BadRequest(new { errors = new[] { new { message = "Query cannot be empty" } } });
            }

            var query = new GraphQLQuery(request.Query)
            {
                OperationName = request.OperationName,
                Variables = request.Variables
            };

            var context = await executionService.ExecuteAsync(query);

            var response = new
            {
                data = context.HasErrors
                    ? null
                    : new
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

            return context.HasErrors
                ? Results.Json(response, statusCode: StatusCodes.Status400BadRequest)
                : Results.Json(response);
        });

        return app;
    }

    /// <summary>
    /// Maps GET /graphql/schema returning the SDL export of registered schemas.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> instance.</param>
    /// <param name="pattern">The URL pattern for the endpoint. Defaults to "/graphql/schema".</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/></exception>
    public static IEndpointRouteBuilder MapGraphQLSchema(this IEndpointRouteBuilder app, string pattern = "/graphql/schema")
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern, nameof(pattern));

        app.MapGet(pattern, (SchemaService schemaService, string? name) =>
        {
            ArgumentNullException.ThrowIfNull(schemaService);

            if (!string.IsNullOrEmpty(name))
            {
                var schema = schemaService.GetSchema(name);
                return schema is null
                    ? Results.NotFound(new { error = $"Schema '{name}' not found" })
                    : Results.Text(schemaService.ExportAsSDL(name), "text/plain");
            }

            var schemas = schemaService.GetAllSchemas().ToList();
            return schemas.Count == 0
                ? Results.Text(string.Empty, "text/plain")
                : Results.Text(string.Join(
                    Environment.NewLine + Environment.NewLine,
                    schemas.Select(s => schemaService.ExportAsSDL(s.Name))),
                    "text/plain");
        });

        return app;
    }

    /// <summary>
    /// Maps GET /health returning a simple liveness payload.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> instance.</param>
    /// <param name="pattern">The URL pattern for the endpoint. Defaults to "/health".</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/></exception>
    public static IEndpointRouteBuilder MapHealthCheck(this IEndpointRouteBuilder app, string pattern = "/health")
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern, nameof(pattern));

        app.MapGet(pattern, () => Results.Json(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow
        }));

        return app;
    }
}