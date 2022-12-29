#nullable enable
using GraphQLEngine;
using GraphQLEngine.Configuration;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.Schema;
using GraphQLEngine.Services.DataLoader;
using GraphQLEngine.Services.Subscriptions;
using GraphQLEngine.Services.QueryAnalysis;
using GraphQLEngine.Services.Caching;
using GraphQLEngine.Services.BackgroundServices;
using GraphQLEngine.Services.Events;
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
    options.CacheTTLSeconds = 300;
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