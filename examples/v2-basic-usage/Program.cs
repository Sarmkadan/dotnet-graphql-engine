using dotnet_graphql_engine;
using dotnet_graphql_engine.Services.GraphQL;
using dotnet_graphql_engine.Domain.Entities;
using dotnet_graphql_engine.Services.Schema;
using dotnet_graphql_engine.Services.DataLoader;
using dotnet_graphql_engine.Services.Subscriptions;
using dotnet_graphql_engine.Services.QueryAnalysis;
using dotnet_graphql_engine.Services.Caching;
using dotnet_graphql_engine.Services.BackgroundServices;
using dotnet_graphql_engine.Services.Events;
using dotnet_graphql_engine.Services.GraphQL.PersistedQueryService;
using dotnet_graphql_engine.Services.GraphQL.CacheService;
using dotnet_graphql_engine.Services.GraphQL.ErrorFormattingService;
using dotnet_graphql_engine.Services.GraphQL.GraphQLExecutionService;
using dotnet_graphql_engine.Services.GraphQL.PersistedQueryService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Create host builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add GraphQL Engine with v2 features
builder.Services.AddGraphQLEngine(options =>
{
    // Enable v2 features
    options.EnableFederation = true;           // Enable federation support
    options.EnableSchemaStitching = true;      // Enable schema stitching
    options.EnableSubscriptions = true;        // Enable WebSocket subscriptions
    options.EnableCaching = true;              // Enable query result caching

    // Performance settings
    options.MaxQueryComplexity = 5000;
    options.MaxQueryDepth = 15;
    options.MaxQueryFields = 200;
    options.QueryTimeoutMs = 30000;

    // Caching settings
    options.CacheTtlSeconds = 300;
    options.CacheMaxSizeBytes = 104857600; // 100 MB

    // Federation settings
    options.FederationDiscoveryEndpoint = "/.well-known/federation";
    options.FederationTimeout = TimeSpan.FromSeconds(30);
    options.EntityCacheTtlSeconds = 300;
    options.EntityCacheMaxSize = 10000;
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map GraphQL endpoints
app.MapGraphQL();           // POST /graphql
app.MapGraphQLSchema();     // GET /graphql/schema
app.MapHealthCheck();       // GET /health

app.Run();