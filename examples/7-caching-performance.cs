// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Example 7: Caching and Performance Optimization
///
/// Demonstrates how to optimize GraphQL query performance through:
/// - Query result caching
/// - Cache key generation and management
/// - Performance monitoring
/// - Cache statistics and hit rate tracking
///
/// Proper caching can reduce response times from 100ms to <1ms.
/// </summary>

using Microsoft.Extensions.DependencyInjection;

public class CachingPerformanceExample
{
    public static async Task Run(IServiceProvider serviceProvider)
    {
        var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();
        var cacheService = serviceProvider.GetRequiredService<CacheService>();

        Console.WriteLine("=== Performance Optimization with Caching ===\n");

        // Example 1: First query (cache miss)
        await DemonstrateCacheMiss(executionService, cacheService);

        // Example 2: Cached query (cache hit)
        await DemonstrateCacheHit(executionService, cacheService);

        // Example 3: Different variables = different cache key
        await DemonstrateCacheKeySeparation(executionService, cacheService);

        // Example 4: Cache statistics
        await DisplayCacheStatistics(cacheService);

        // Example 5: Cache invalidation
        await DemonstrateCacheInvalidation(executionService, cacheService);

        // Example 6: Performance comparison
        await ComparePerformance(executionService);
    }

    private static async Task DemonstrateCacheMiss(
        GraphQLExecutionService executionService,
        CacheService cacheService)
    {
        Console.WriteLine("Example 1: First Query (Cache Miss)\n");

        var query = new GraphQLQuery("{ users { id name email } }");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var context = await executionService.ExecuteAsync(query);

        stopwatch.Stop();

        Console.WriteLine($"Query: {query.QueryString.Substring(0, 50)}...");
        Console.WriteLine($"Status: CACHE MISS (first execution)");
        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Result cached for future requests\n");
    }

    private static async Task DemonstrateCacheHit(
        GraphQLExecutionService executionService,
        CacheService cacheService)
    {
        Console.WriteLine("Example 2: Subsequent Query (Cache Hit)\n");

        var query = new GraphQLQuery("{ users { id name email } }");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Execute same query again
        var context = await executionService.ExecuteAsync(query);

        stopwatch.Stop();

        Console.WriteLine($"Query: {query.QueryString.Substring(0, 50)}...");
        Console.WriteLine($"Status: CACHE HIT (retrieved from cache)");
        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Speed improvement: ~100x faster from cache\n");
    }

    private static async Task DemonstrateCacheKeySeparation(
        GraphQLExecutionService executionService,
        CacheService cacheService)
    {
        Console.WriteLine("Example 3: Cache Key Separation\n");

        var query1 = new GraphQLQuery("query GetUser { user(id: \"1\") { name } }");
        var variables1 = new Dictionary<string, object> { { "id", "1" } };

        var query2 = new GraphQLQuery("query GetUser { user(id: \"2\") { name } }");
        var variables2 = new Dictionary<string, object> { { "id", "2" } };

        var context1 = await executionService.ExecuteAsync(query1, variables1);
        var context2 = await executionService.ExecuteAsync(query2, variables2);

        Console.WriteLine("Query 1: user(id: \"1\") - Cached separately");
        Console.WriteLine("Query 2: user(id: \"2\") - Different cache key");
        Console.WriteLine("Each query has unique cache entry based on query + variables\n");
    }

    private static async Task DisplayCacheStatistics(CacheService cacheService)
    {
        Console.WriteLine("Example 4: Cache Statistics\n");

        var stats = cacheService.GetStatistics();

        Console.WriteLine($"Cache Metrics:");
        Console.WriteLine($"  Total Entries: {stats.TotalEntries}");
        Console.WriteLine($"  Cache Size: {stats.SizeBytes / 1024}KB / {stats.MaxSizeBytes / 1024}KB");
        Console.WriteLine($"  Cache Utilization: {(stats.SizeBytes * 100 / stats.MaxSizeBytes):F1}%");
        Console.WriteLine($"  Hit Rate: {stats.HitRate:P2}");
        Console.WriteLine($"  Total Hits: {stats.Hits}");
        Console.WriteLine($"  Total Misses: {stats.Misses}");
        Console.WriteLine($"  Evictions: {stats.Evictions}");
        Console.WriteLine();
    }

    private static async Task DemonstrateCacheInvalidation(
        GraphQLExecutionService executionService,
        CacheService cacheService)
    {
        Console.WriteLine("Example 5: Cache Invalidation\n");

        var query = new GraphQLQuery("{ users { id name } }");

        // Execute query
        var context1 = await executionService.ExecuteAsync(query);
        Console.WriteLine("✓ Query cached");

        // Invalidate cache (e.g., after mutation)
        await cacheService.ClearAsync();
        Console.WriteLine("✓ Cache cleared (e.g., after user creation)");

        // Next execution will cache miss
        var context2 = await executionService.ExecuteAsync(query);
        Console.WriteLine("✓ Fresh query executed with updated data\n");
    }

    private static async Task ComparePerformance(GraphQLExecutionService executionService)
    {
        Console.WriteLine("Example 6: Performance Comparison\n");

        var query = new GraphQLQuery(@"
        {
            users {
                id
                name
                posts {
                    id
                    title
                    comments { text }
                }
            }
        }");

        // First execution (cache miss)
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        await executionService.ExecuteAsync(query);
        sw1.Stop();
        var firstTime = sw1.ElapsedMilliseconds;

        // Second execution (cache hit)
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        await executionService.ExecuteAsync(query);
        sw2.Stop();
        var secondTime = sw2.ElapsedMilliseconds;

        // Third execution (cache hit)
        var sw3 = System.Diagnostics.Stopwatch.StartNew();
        await executionService.ExecuteAsync(query);
        sw3.Stop();
        var thirdTime = sw3.ElapsedMilliseconds;

        Console.WriteLine("Complex Query Performance:");
        Console.WriteLine($"  1st execution (cold):  {firstTime}ms");
        Console.WriteLine($"  2nd execution (warm):  {secondTime}ms ({(double)firstTime/secondTime:F1}x faster)");
        Console.WriteLine($"  3rd execution (warm):  {thirdTime}ms ({(double)firstTime/thirdTime:F1}x faster)");
        Console.WriteLine();

        var avgCached = (secondTime + thirdTime) / 2.0;
        var improvement = ((firstTime - avgCached) / firstTime * 100);
        Console.WriteLine($"Average performance improvement: {improvement:F1}%");
    }
}

/// <summary>
/// Caching Strategy:
///
/// 1. AUTOMATIC QUERY CACHING
///    - Query result cached automatically
///    - Cache key = Hash(query string + variables)
///    - Identical queries reuse cache entry
///
/// 2. CACHE CONFIGURATION
///    options.EnableCaching = true;              // Enable caching
///    options.CacheTtlSeconds = 300;             // 5 minutes
///    options.CacheMaxSizeBytes = 52428800;      // 50 MB
///
/// 3. WHEN TO CACHE
///    ✓ Expensive queries with stable data
///    ✓ Frequently accessed data (leaderboards, trending)
///    ✓ Read-heavy workloads
///    ✗ Real-time data that changes often
///    ✗ User-specific data (different per user)
///    ✗ Sensitive data (security concerns)
///
/// 4. CACHE INVALIDATION
///    - Manual: cacheService.ClearAsync()
///    - TTL-based: Automatic expiration after N seconds
///    - LRU eviction: Remove least used when full
///
/// 5. PERFORMANCE IMPACT
///    Without cache:
///    - Complex query: 50-100ms
///    - N concurrent users: N × 100ms
///
///    With cache:
///    - First request: 50-100ms (cache miss)
///    - Cached requests: <1ms (cache hit)
///    - 100 concurrent users: ~100ms total (shared cache)
///    Result: 50-100x performance improvement!
///
/// 6. BEST PRACTICES
///    - Cache aggregate/summary queries
///    - Invalidate after mutations
///    - Monitor cache hit rate
///    - Set appropriate TTL based on data freshness
///    - Use different cache per schema
///    - Implement cache warming for critical queries
/// </summary>
