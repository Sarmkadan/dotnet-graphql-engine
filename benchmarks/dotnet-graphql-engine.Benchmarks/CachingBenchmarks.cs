using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Benchmarks;

/// <summary>
/// Benchmarks for GraphQL caching performance
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[GcServer(true)]
[GcConcurrent(true)]
public class CachingBenchmarks
{
    private ServiceProvider? _serviceProvider;
    private SchemaService? _schemaService;
    private GraphQLExecutionService? _executionService;
    private GraphQLQuery? _cachedQuery;

    [GlobalSetup]
    public void Setup()
    {
        // Configure logging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddFilter(level => level >= LogLevel.Warning);
        });

        // Setup DI container with caching enabled
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(loggerFactory);
        services.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 10000;
            options.EnableCaching = true;
            options.CacheTTLSeconds = 300;
            options.CacheMaxSize = 1000;
        });

        _serviceProvider = services.BuildServiceProvider();
        _schemaService = _serviceProvider.GetRequiredService<SchemaService>();
        _executionService = _serviceProvider.GetRequiredService<GraphQLExecutionService>();

        // Define User type
        var userType = new GraphQLType
        {
            Name = "User",
            Description = "A user in the system",
            Kind = TypeKind.Object
        };
        userType.AddField(new GraphQLField { Name = "id", Type = "ID!", Description = "User ID" });
        userType.AddField(new GraphQLField { Name = "name", Type = "String!", Description = "User name" });
        userType.AddField(new GraphQLField { Name = "email", Type = "String!", Description = "User email" });

        _schemaService.AddType("BenchmarkSchema", userType);

        // Define Query type
        var queryType = new GraphQLType
        {
            Name = "Query",
            Description = "Root query type",
            Kind = TypeKind.Object
        };
        queryType.AddField(new GraphQLField { Name = "user", Type = "User", Description = "Get a user" });
        queryType.AddField(new GraphQLField { Name = "users", Type = "[User!]!", Description = "Get all users" });

        _schemaService.AddType("BenchmarkSchema", queryType);

        // Register resolvers
        _executionService.RegisterResolver("user", async (context) => new { id = "1", name = "John Doe", email = "john@example.com" });
        _executionService.RegisterResolver("users", async (context) =>
        {
            return new object[]
            {
                new { id = "1", name = "John Doe", email = "john@example.com" },
                new { id = "2", name = "Jane Smith", email = "jane@example.com" },
                new { id = "3", name = "Bob Johnson", email = "bob@example.com" }
            };
        });

        // Create a query that will be cached
        _cachedQuery = new GraphQLQuery("{ users { id name email } }");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
    }

    [Benchmark]
    [BenchmarkCategory("Caching")]
    public async Task FirstExecution()
    {
        var result = await _executionService!.ExecuteAsync(_cachedQuery!);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }
    }

    [Benchmark]
    [BenchmarkCategory("Caching")]
    public async Task CachedExecution()
    {
        var result = await _executionService!.ExecuteAsync(_cachedQuery!);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }
    }

    [Benchmark]
    [BenchmarkCategory("Caching")]
    public async Task CacheMissAfterExpiration()
    {
        // Execute once to populate cache
        await _executionService!.ExecuteAsync(_cachedQuery!);

        // Wait for cache to expire (simulate by creating new execution service with fresh cache)
        var newServices = new ServiceCollection();
        newServices.AddLogging();
        newServices.AddSingleton(LoggerFactory.Create(l => l.AddFilter(level => level >= LogLevel.Warning)));
        newServices.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 10000;
            options.EnableCaching = true;
            options.CacheTTLSeconds = 1; // Very short TTL
            options.CacheMaxSize = 1000;
        });

        var newServiceProvider = newServices.BuildServiceProvider();
        var newExecutionService = newServiceProvider.GetRequiredService<GraphQLExecutionService>();

        // This should miss the cache
        var result = await newExecutionService.ExecuteAsync(_cachedQuery!);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }

        newServiceProvider.Dispose();
    }

    [Benchmark]
    [BenchmarkCategory("Caching")]
    public async Task CacheHitRatio()
    {
        // Execute the same query multiple times
        for (int i = 0; i < 100; i++)
        {
            var result = await _executionService!.ExecuteAsync(_cachedQuery!);
            if (result.HasErrors)
            {
                throw new InvalidOperationException("Query execution failed");
            }
        }
    }
}

/// <summary>
/// Benchmarks for comparing caching vs non-caching performance
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CachingComparisonBenchmarks
{
    private ServiceProvider? _cachedServiceProvider;
    private ServiceProvider? _nonCachedServiceProvider;
    private GraphQLExecutionService? _cachedExecutionService;
    private GraphQLExecutionService? _nonCachedExecutionService;
    private GraphQLQuery? _testQuery;

    [Params(1, 5, 10, 50, 100)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Configure logging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddFilter(level => level >= LogLevel.Warning);
        });

        // Setup DI container WITH caching
        var cachedServices = new ServiceCollection();
        cachedServices.AddLogging();
        cachedServices.AddSingleton(loggerFactory);
        cachedServices.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 10000;
            options.EnableCaching = true;
            options.CacheTTLSeconds = 300;
        });

        _cachedServiceProvider = cachedServices.BuildServiceProvider();
        _cachedExecutionService = _cachedServiceProvider.GetRequiredService<GraphQLExecutionService>();

        // Setup DI container WITHOUT caching
        var nonCachedServices = new ServiceCollection();
        nonCachedServices.AddLogging();
        nonCachedServices.AddSingleton(loggerFactory);
        nonCachedServices.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 10000;
            options.EnableCaching = false;
        });

        _nonCachedServiceProvider = nonCachedServices.BuildServiceProvider();
        _nonCachedExecutionService = _nonCachedServiceProvider.GetRequiredService<GraphQLExecutionService>();

        // Define simple schema
        var schemaService = _cachedExecutionService.GetRequiredService<SchemaService>();
        var userType = new GraphQLType
        {
            Name = "User",
            Kind = TypeKind.Object
        };
        userType.AddField(new GraphQLField { Name = "id", Type = "ID!" });
        userType.AddField(new GraphQLField { Name = "name", Type = "String!" });

        schemaService.AddType("TestSchema", userType);

        var queryType = new GraphQLType
        {
            Name = "Query",
            Kind = TypeKind.Object
        };
        queryType.AddField(new GraphQLField { Name = "user", Type = "User" });
        schemaService.AddType("TestSchema", queryType);

        _cachedExecutionService.RegisterResolver("user", async (ctx) => new { id = "1", name = "Test User" });
        _nonCachedExecutionService.RegisterResolver("user", async (ctx) => new { id = "1", name = "Test User" });

        _testQuery = new GraphQLQuery("{ user { id name } }");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cachedServiceProvider?.Dispose();
        _nonCachedServiceProvider?.Dispose();
    }

    [Benchmark]
    [BenchmarkCategory("Caching Comparison")]
    public async Task Cached_Iterations()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var result = await _cachedExecutionService!.ExecuteAsync(_testQuery!);
            if (result.HasErrors)
            {
                throw new InvalidOperationException("Query execution failed");
            }
        }
    }

    [Benchmark]
    [BenchmarkCategory("Caching Comparison")]
    public async Task NonCached_Iterations()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var result = await _nonCachedExecutionService!.ExecuteAsync(_testQuery!);
            if (result.HasErrors)
            {
                throw new InvalidOperationException("Query execution failed");
            }
        }
    }
}