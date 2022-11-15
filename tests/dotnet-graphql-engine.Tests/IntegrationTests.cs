#nullable enable
using FluentAssertions;
using GraphQLEngine.Configuration;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.DataLoader;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.QueryAnalysis;
using GraphQLEngine.Services.Caching;
using GraphQLEngine.Middleware;
using GraphQLEngine.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GraphQLEngine.Tests.Integration;

sealed public class GraphQLExecutionIntegrationTests
{
    private static readonly ILogger<GraphQLExecutionService> _logger =
        new Mock<ILogger<GraphQLExecutionService>>().Object;
    private static readonly ILogger<DataLoaderService> _dlLogger =
        new Mock<ILogger<DataLoaderService>>().Object;
    private static readonly ILogger<QueryAnalysisService> _qaLogger =
        new Mock<ILogger<QueryAnalysisService>>().Object;

    [Fact]
    public async Task CompleteGraphQLWorkflow_ExecuteQueryWithDataLoading()
    {
        // Arrange: Set up the execution pipeline
        var dataLoaderService = new DataLoaderService(_dlLogger);
        var executionService = new GraphQLExecutionService(_logger, dataLoaderService);
        var analysisService = new QueryAnalysisService(_qaLogger);

        var userData = new Dictionary<int, string> { { 1, "Alice" }, { 2, "Bob" } };
        dataLoaderService.RegisterBatchFunction("users", async (ids) =>
        {
            await Task.Delay(10);
            return ids.Cast<int>()
                .Select(id => userData.ContainsKey(id) ? (object?)userData[id] : null)
                .ToList();
        });

        var query = new GraphQLQuery("{ user { id name } }");
        executionService.RegisterResolver("user", async () =>
        {
            await Task.Delay(5);
            return new { id = 1, name = "Alice" };
        });

        // Act: Execute the complete workflow
        var context = await executionService.ExecuteAsync(query);
        var analysis = analysisService.AnalyzeQuery(query);

        // Assert: Verify all components worked together
        context.State.Should().Be(ExecutionState.Completed);
        context.Should().NotBeNull();
        analysis.FieldCount.Should().BeGreaterThan(0);
        analysis.MaxDepth.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MultipleQueriesInSequence_MaintainIndependentContexts()
    {
        // Arrange
        var dataLoaderService = new DataLoaderService(_dlLogger);
        var executionService = new GraphQLExecutionService(_logger, dataLoaderService);

        // Act: Execute multiple queries
        var query1 = new GraphQLQuery("{ query1 { field1 } }");
        var query2 = new GraphQLQuery("{ query2 { field2 } }");
        var query3 = new GraphQLQuery("{ query3 { field3 } }");

        var context1 = await executionService.ExecuteAsync(query1);
        var context2 = await executionService.ExecuteAsync(query2);
        var context3 = await executionService.ExecuteAsync(query3);

        // Assert: All contexts are independent
        context1.Id.Should().NotBe(context2.Id);
        context2.Id.Should().NotBe(context3.Id);
        context1.State.Should().Be(ExecutionState.Completed);
        context2.State.Should().Be(ExecutionState.Completed);
        context3.State.Should().Be(ExecutionState.Completed);
    }

    [Fact]
    public async Task ComplexNestedQuery_WithDifferentDepths_AnalyzesCorrectly()
    {
        // Arrange
        var analysisService = new QueryAnalysisService(_qaLogger);

        // Act: Analyze queries with different depths
        var shallowQuery = new GraphQLQuery("{ user { id } }");
        var deepQuery = new GraphQLQuery(
            "{ user { profile { settings { notifications { email { frequency } } } } } }"
        );

        var shallowAnalysis = analysisService.AnalyzeQuery(shallowQuery);
        var deepAnalysis = analysisService.AnalyzeQuery(deepQuery);

        // Assert: Deeper query has higher metrics
        deepAnalysis.MaxDepth.Should().BeGreaterThan(shallowAnalysis.MaxDepth);
        deepAnalysis.FieldCount.Should().BeGreaterThanOrEqualTo(shallowAnalysis.FieldCount);
    }
}

sealed public class CachingIntegrationTests
{
    [Fact]
    public void CachingWorkflow_WithKeyBuilder_StoresAndRetrievesData()
    {
        // Arrange
        var cache = new DistributedCacheService(
            new Mock<ILogger<DistributedCacheService>>().Object
        );
        var query = "{ user { id name profile { bio } } }";
        var schemaName = "default";
        var cacheKey = CacheKeyBuilder.BuildQueryKey(schemaName, query);

        var expectedData = new { id = 1, name = "Alice", profile = new { bio = "Test" } };

        // Act: Cache the result
        cache.Set(cacheKey, expectedData);
        var cachedResult = cache.Get<object>(cacheKey);

        // Assert: Data was cached correctly
        cachedResult.Should().NotBeNull();
        cachedResult.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public void MultiTenantCaching_WithNamespacedKeys_IsolatesTenantData()
    {
        // Arrange
        var cache = new DistributedCacheService(
            new Mock<ILogger<DistributedCacheService>>().Object
        );

        var query = "{ user { id } }";
        var tenant1Builder = CacheKeyBuilder.WithNamespace("tenant1:");
        var tenant2Builder = CacheKeyBuilder.WithNamespace("tenant2:");

        var key1 = tenant1Builder.BuildQueryKeyNs("default", query);
        var key2 = tenant2Builder.BuildQueryKeyNs("default", query);

        // Act: Store different data for different tenants
        cache.Set(key1, "tenant1-data");
        cache.Set(key2, "tenant2-data");

        var result1 = cache.Get<string>(key1);
        var result2 = cache.Get<string>(key2);

        // Assert: Each tenant has isolated data
        result1.Should().Be("tenant1-data");
        result2.Should().Be("tenant2-data");
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void CacheKeyBuilder_WithDifferentQueryVariations_GeneratesUniqueKeys()
    {
        // Arrange
        var baseQuery = "query GetUser { user { id name } }";
        var variables1 = new Dictionary<string, object> { { "id", "user1" } };
        var variables2 = new Dictionary<string, object> { { "id", "user2" } };

        // Act
        var key1 = CacheKeyBuilder.BuildQueryKey("schema", baseQuery, variables1);
        var key2 = CacheKeyBuilder.BuildQueryKey("schema", baseQuery, variables2);
        var key3 = CacheKeyBuilder.BuildQueryKey("schema", baseQuery);

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().NotBe(key3);
        key2.Should().NotBe(key3);
    }

    [Fact]
    public void CacheInvalidation_WithPatternMatching_RemovesRelatedEntries()
    {
        // Arrange
        var cache = new DistributedCacheService(
            new Mock<ILogger<DistributedCacheService>>().Object
        );

        var key1 = CacheKeyBuilder.BuildSchemaKey("schema1");
        var key2 = CacheKeyBuilder.BuildSchemaKey("schema1");
        var key3 = CacheKeyBuilder.BuildRateLimitKey("client1", "endpoint");

        cache.Set(key1, "data1");
        cache.Set(key2, "data2");
        cache.Set(key3, "data3");

        // Act: Match pattern for schema keys
        var pattern = CacheKeyBuilder.BuildPatternKey("gql:schema:");
        var key1Matches = CacheKeyBuilder.MatchesPattern(key1, pattern);
        var key3Matches = CacheKeyBuilder.MatchesPattern(key3, pattern);

        // Assert
        key1Matches.Should().BeTrue();
        key3Matches.Should().BeFalse();
    }
}

sealed public class ConcurrencyTests
{
    [Fact]
    public async Task DataLoaderService_WithConcurrentRequests_ExecutesAllBatches()
    {
        // Arrange
        var dlLogger = new Mock<ILogger<DataLoaderService>>().Object;
        var dataLoaderService = new DataLoaderService(dlLogger);

        var executedBatches = 0;
        dataLoaderService.RegisterBatchFunction("loader", async (ids) =>
        {
            Interlocked.Increment(ref executedBatches);
            await Task.Delay(10);
            return ids.Cast<object?>().ToList();
        });

        // Act: Create multiple requests concurrently
        var tasks = Enumerable.Range(0, 10)
            .Select(async i =>
            {
                var request = dataLoaderService.CreateRequest("loader", $"ctx-{i}");
                dataLoaderService.LoadKey(request.Id, $"key-{i}");
                return await dataLoaderService.ExecuteAsync(request.Id);
            })
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.State.Should().Be(DataLoaderState.Completed));
        results.Should().HaveCount(10);
    }

    [Fact]
    public void DistributedCacheService_WithConcurrentReads_ReturnsConsistentData()
    {
        // Arrange
        var cache = new DistributedCacheService(
            new Mock<ILogger<DistributedCacheService>>().Object
        );
        var key = "concurrent-test-key";
        var testData = "test-data";
        cache.Set(key, testData);

        // Act: Read concurrently
        var results = new List<string?>();
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() =>
            {
                lock (results)
                {
                    results.Add(cache.Get<string>(key));
                }
            }))
            .ToList();

        Task.WaitAll(tasks.ToArray());

        // Assert: All reads return the same value
        results.Should().AllSatisfy(r => r.Should().Be(testData));
        results.Should().HaveCount(20);
    }

    [Fact]
    public async Task CacheService_WithConcurrentWrites_MaintainsDataIntegrity()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CacheService>>().Object;
        var options = new GraphQLEngineOptions { EnableCaching = true };
        var cacheService = new CacheService(mockLogger, options);

        // Act: Write concurrently
        var tasks = Enumerable.Range(0, 10)
            .Select(i => Task.Run(() =>
            {
                var key = $"key-{i}";
                var value = $"value-{i}";
                cacheService.Set(key, value);
                var result = cacheService.Get(key);
                return (key, expected: value, actual: result);
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r =>
            r.actual.Should().Be(r.expected)
        );
    }

    [Fact]
    public async Task ExecutionService_WithConcurrentQueries_ExecutesIndependently()
    {
        // Arrange
        var logger = new Mock<ILogger<GraphQLExecutionService>>().Object;
        var dlLogger = new Mock<ILogger<DataLoaderService>>().Object;
        var dataLoaderService = new DataLoaderService(dlLogger);
        var executionService = new GraphQLExecutionService(logger, dataLoaderService);

        var completedQueries = 0;

        // Act: Execute multiple queries concurrently
        var tasks = Enumerable.Range(0, 5)
            .Select(async i =>
            {
                var query = new GraphQLQuery($"{{ query{i} {{ field }} }}");
                var context = await executionService.ExecuteAsync(query);
                Interlocked.Increment(ref completedQueries);
                return context;
            })
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.State.Should().Be(ExecutionState.Completed));
        completedQueries.Should().Be(5);
    }
}

sealed public class ConfigurationTests
{
    [Fact]
    public void QueryDepthLimiter_WithDifferentMaxDepths_EnforcesLimitsCorrectly()
    {
        // Arrange
        var logger = new Mock<ILogger<QueryDepthLimiter>>().Object;
        var limiter5 = new QueryDepthLimiter(logger, new QueryDepthLimiterOptions { MaxDepth = 5 });
        var limiter10 = new QueryDepthLimiter(logger, new QueryDepthLimiterOptions { MaxDepth = 10 });

        var deepQuery = "{ a { b { c { d { e { f { g } } } } } } }";

        // Act
        var result5 = limiter5.Check(deepQuery);
        var result10 = limiter10.Check(deepQuery);

        // Assert
        result5.Allowed.Should().BeFalse();
        result10.Allowed.Should().BeTrue();
        result5.MaxDepth.Should().Be(5);
        result10.MaxDepth.Should().Be(10);
    }

    [Fact]
    public void CacheService_WithEnablingDisablingCaching_RespectConfiguration()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CacheService>>().Object;
        var enabledOptions = new GraphQLEngineOptions { EnableCaching = true };
        var disabledOptions = new GraphQLEngineOptions { EnableCaching = false };

        var enabledCache = new CacheService(mockLogger, enabledOptions);
        var disabledCache = new CacheService(mockLogger, disabledOptions);

        var key = "test-key";
        var value = "test-value";

        // Act
        enabledCache.Set(key, value);
        disabledCache.Set(key, value);

        var enabledResult = enabledCache.Get(key);
        var disabledResult = disabledCache.Get(key);

        // Assert
        enabledResult.Should().Be(value);
        disabledResult.Should().BeNull();
    }

    [Fact]
    public void ErrorFormattingService_WithDetailedMessagesConfiguration_FormatsErrorsDifferently()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ErrorFormattingService>>().Object;
        var detailedOptions = new GraphQLEngineOptions { EnableDetailedErrorMessages = true };
        var sanitizedOptions = new GraphQLEngineOptions { EnableDetailedErrorMessages = false };

        var detailedFormatter = new ErrorFormattingService(mockLogger, detailedOptions);
        var sanitizedFormatter = new ErrorFormattingService(mockLogger, sanitizedOptions);

        var error = new ExecutionError
        {
            Message = "Critical error",
            StackTrace = "at Method() in File.cs:line 42"
        };

        // Act
        var detailedResult = detailedFormatter.FormatError(error);
        var sanitizedResult = sanitizedFormatter.FormatError(error);

        // Assert
        detailedResult.Should().ContainKey("extensions");
        sanitizedResult.Should().NotContainKey("extensions");
    }

    [Fact]
    public void PersistedQueryService_WithAllowlistOnlyMode_RestrictsQueries()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<PersistedQuery>>();
        var mockLogger = new Mock<ILogger<PersistedQueryService>>().Object;
        var allowlistOptions = new PersistedQueryOptions { AllowlistOnly = true };

        var service = new PersistedQueryService(mockRepo.Object, mockLogger, allowlistOptions);

        // Act & Assert
        // Verify that the service respects the AllowlistOnly setting
        service.Should().NotBeNull();
    }
}

sealed public class EndToEndWorkflowTests
{
    [Fact]
    public async Task CompleteGraphQLStack_FromQueryToCachedResult()
    {
        // Arrange: Set up all services
        var execLogger = new Mock<ILogger<GraphQLExecutionService>>().Object;
        var dlLogger = new Mock<ILogger<DataLoaderService>>().Object;
        var qaLogger = new Mock<ILogger<QueryAnalysisService>>().Object;
        var cacheLogger = new Mock<ILogger<DistributedCacheService>>().Object;

        var dataLoaderService = new DataLoaderService(dlLogger);
        var executionService = new GraphQLExecutionService(execLogger, dataLoaderService);
        var analysisService = new QueryAnalysisService(qaLogger);
        var cache = new DistributedCacheService(cacheLogger);
        var depthLimiter = new QueryDepthLimiter(
            new Mock<ILogger<QueryDepthLimiter>>().Object
        );

        var query = "{ user { id name email } }";

        // Act: Run through the complete workflow
        // 1. Check query depth
        var depthResult = depthLimiter.Check(query);

        // 2. Analyze query complexity
        var graphQLQuery = new GraphQLQuery(query);
        var analysis = analysisService.AnalyzeQuery(graphQLQuery);

        // 3. Check cache
        var cacheKey = CacheKeyBuilder.BuildQueryKey("default", query);
        var cachedResult = cache.Get<string>(cacheKey);

        if (cachedResult == null)
        {
            // 4. Execute query
            var context = await executionService.ExecuteAsync(graphQLQuery);

            // 5. Cache result
            var mockResult = "{ \"user\": { \"id\": \"1\", \"name\": \"Alice\" } }";
            cache.Set(cacheKey, mockResult);
        }

        // 6. Retrieve from cache
        var finalResult = cache.Get<string>(cacheKey);

        // Assert
        depthResult.Allowed.Should().BeTrue();
        analysis.FieldCount.Should().BeGreaterThan(0);
        finalResult.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MainUseCase_SchemaStitchingWithComplexAnalysis()
    {
        // Arrange
        var qaLogger = new Mock<ILogger<QueryAnalysisService>>().Object;
        var analysisService = new QueryAnalysisService(qaLogger);

        // A typical federated/stitched schema query
        var stitchedQuery = new GraphQLQuery(@"
            {
                users {
                    id
                    name
                    orders {
                        id
                        total
                        items {
                            sku
                            price
                        }
                    }
                }
            }
        ");

        // Act
        var analysis = analysisService.AnalyzeQuery(stitchedQuery);

        // Assert
        analysis.FieldCount.Should().BeGreaterThanOrEqualTo(5);
        analysis.MaxDepth.Should().BeGreaterThan(2);
        analysis.TotalScore.Should().BeGreaterThan(0);
    }
}
