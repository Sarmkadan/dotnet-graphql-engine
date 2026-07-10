# GraphQLHttpRequest

`GraphQLHttpRequest` represents an incoming GraphQL request over HTTP. It encapsulates the three standard components of a GraphQL POST body—the query string, an optional operation name, and an optional dictionary of variables—and exposes static endpoint-mapping methods that register GraphQL, schema, and health-check routes into the ASP.NET Core routing infrastructure.

## API

### `public string Query`

The GraphQL query or mutation string to execute. This field is required and must contain a syntactically valid GraphQL document. The runtime will parse and validate it before execution.

### `public string? OperationName`

An optional name identifying which operation within a multi-operation document should be executed. When the `Query` string contains multiple named operations, this field must be set to the desired operation name; otherwise the server will return an error. When `null` and the document contains exactly one operation, that single operation is executed implicitly.

### `public Dictionary<string, object?>? Variables`

An optional dictionary of variable values to supply to the GraphQL operation. Keys correspond to variable names declared in the operation signature (`$variableName`). Values are weakly typed as `object?` and will be coerced to the expected GraphQL types during execution. When `null`, the operation receives no external variable bindings.

### `public static IEndpointRouteBuilder MapGraphQL(/* parameters omitted */)`

Maps the primary GraphQL execution endpoint onto the provided `IEndpointRouteBuilder`. This registers a route (typically `POST /graphql`) that accepts a `GraphQLHttpRequest` body, executes the query, and returns the standard GraphQL JSON response. Returns the same `IEndpointRouteBuilder` instance to support fluent chaining of further endpoint mappings.

### `public static IEndpointRouteBuilder MapGraphQLSchema(/* parameters omitted */)`

Maps an endpoint that serves the current GraphQL schema definition, typically as an SDL string or introspection JSON result. The exact route and format depend on internal configuration. Returns the `IEndpointRouteBuilder` for fluent composition.

### `public static IEndpointRouteBuilder MapHealthCheck(/* parameters omitted */)`

Maps a lightweight health-check endpoint that reports the readiness of the GraphQL engine. This endpoint performs minimal internal checks (e.g., schema availability) and returns a success or failure status. Returns the `IEndpointRouteBuilder` for fluent composition.

## Usage

### Example 1: Minimal GraphQL endpoint with health check

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGraphQL();
app.MapHealthCheck();

app.Run();
```

This registers a default `POST /graphql` endpoint accepting `GraphQLHttpRequest` payloads and a health-check endpoint at a conventional route. Clients can POST:

```json
{
  "query": "{ hello }",
  "operationName": null,
  "variables": null
}
```

### Example 2: Explicit route configuration with schema endpoint

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGraphQLSchema()
   .MapGraphQL()
   .MapHealthCheck();

app.Run();
```

All three endpoints are registered in a fluent chain. The schema endpoint allows tooling to introspect the API. A client sending a parameterised query would POST:

```json
{
  "query": "query Greet($name: String!) { greet(name: $name) }",
  "operationName": "Greet",
  "variables": { "name": "World" }
}
```

## Notes

- The `Query` field must not be `null` or empty; the runtime will reject requests with a missing or blank query.
- When `OperationName` is `null` and the document contains multiple operations, execution fails with a descriptive error. Clients should always supply the operation name for multi-operation documents.
- Variable coercion follows GraphQL specification rules; incompatible types or missing required variables cause field-level errors in the response, not HTTP-level exceptions.
- The static `Map*` methods are designed for registration during application startup and are not thread-safe for concurrent mutation of the route table at runtime. Once the application is running, the registered routes are served safely under concurrent HTTP requests.
- `GraphQLHttpRequest` itself is a plain data object; instances are typically deserialised per-request by the framework and are not shared across threads.
