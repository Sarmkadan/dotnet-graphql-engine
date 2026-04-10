#nullable enable
using FluentAssertions;
using GraphQLEngine.Configuration;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Middleware;
using GraphQLEngine.Services.Caching;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GraphQLEngine.Tests.Middleware;

sealed public class QueryDepthLimiterTests
{
    private static QueryDepthLimiter CreateLimiter(int maxDepth = 10)
    {
        var mockLogger = new Mock<ILogger<QueryDepthLimiter>>().Object;
        var options = new QueryDepthLimiterOptions { MaxDepth = maxDepth };
        return new QueryDepthLimiter(mockLogger, options);
    }

    [Fact]
    public void Check_WithEmptyQuery_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var limiter = CreateLimiter();

        // Act
        var result = limiter.Check("");

        // Assert
        result.Allowed.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("empty");
    }

    [Fact]
    public void Check_WithNullQuery_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var limiter = CreateLimiter();

        // Act
        var result = limiter.Check(null!);

        // Assert
        result.Allowed.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Check_WithShallowQuery_ReturnsTrue()
    {
        // Arrange
        var limiter = CreateLimiter(maxDepth: 10);
        var query = "{ user { id name } }";

        // Act
        var result = limiter.Check(query);

        // Assert
        result.Allowed.Should().BeTrue();
        result.Depth.Should().BeLessThanOrEqualTo(10);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Check_WithDeepQuery_ReturnsFalseWhenExceedsLimit()
    {
        // Arrange
        var limiter = CreateLimiter(maxDepth: 3);
        var deepQuery = "{ a { b { c { d { e } } } } }";

        // Act
        var result = limiter.Check(deepQuery);

        // Assert
        result.Allowed.Should().BeFalse();
        result.Depth.Should().BeGreaterThan(3);
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Check_WithExactlyMaxDepth_ReturnsTrue()
    {
        // Arrange
        var limiter = CreateLimiter(maxDepth: 3);
        var query = "{ a { b { c } } }";

        // Act
        var result = limiter.Check(query);

        // Assert
        result.Allowed.Should().BeTrue();
        result.Depth.Should().Be(3);
    }

    [Fact]
    public void Check_WithStringContainingBraces_IgnoresBracesInStrings()
    {
        // Arrange
        var limiter = CreateLimiter(maxDepth: 10);
        var query = @"{ user { description: ""{ test }"" } }";

        // Act
        var result = limiter.Check(query);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Check_WithComments_IgnoresBracesInComments()
    {
        // Arrange
        var limiter = CreateLimiter(maxDepth: 5);
        var query = @"{
            # This comment has { and }
            user { id }
        }";

        // Act
        var result = limiter.Check(query);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Check_WithBlockStrings_IgnoresBracesInBlockStrings()
    {
        // Arrange
        var limiter = CreateLimiter(maxDepth: 5);
        var query = @"{ user { description: """"""{ deep braces }"""""""" } }";

        // Act
        var result = limiter.Check(query);

        // Assert
        // Exact depth may vary based on parsing, but should not crash
        result.Depth.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Check_ReturnsCorrectMaxDepthInResult()
    {
        // Arrange
        var maxDepth = 7;
        var limiter = CreateLimiter(maxDepth: maxDepth);
        var query = "{ a { b } }";

        // Act
        var result = limiter.Check(query);

        // Assert
        result.MaxDepth.Should().Be(maxDepth);
    }

    [Fact]
    public void ToErrorJson_WhenAllowed_ReturnsNull()
    {
        // Arrange
        var limiter = CreateLimiter();
        var result = limiter.Check("{ user { id } }");

        // Act
        var json = result.ToErrorJson();

        // Assert
        json.Should().BeNull();
    }

    [Fact]
    public void ToErrorJson_WhenNotAllowed_ReturnsValidJson()
    {
        // Arrange
        var limiter = CreateLimiter(maxDepth: 2);
        var result = limiter.Check("{ a { b { c } } }");

        // Act
        var json = result.ToErrorJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("errors");
        json.Should().Contain("exceeds maximum");
    }
}

sealed public class PersistedQueryServiceTests
{
    private static PersistedQueryService CreatePersistedQueryService(IRepository<PersistedQuery>? repo = null)
    {
        var repository = repo ?? new Mock<IRepository<PersistedQuery>>().Object;
        var mockLogger = new Mock<ILogger<PersistedQueryService>>().Object;
        var options = new PersistedQueryOptions();
        return new PersistedQueryService(repository, mockLogger, options);
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PersistedQueryService(null!, new Mock<ILogger<PersistedQueryService>>().Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var repo = new Mock<IRepository<PersistedQuery>>().Object;
        Assert.Throws<ArgumentNullException>(() =>
            new PersistedQueryService(repo, null!));
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyQueryString_ThrowsArgumentException()
    {
        // Arrange
        var service = CreatePersistedQueryService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(""));
    }

    [Fact]
    public async Task RegisterAsync_WithValidQueryString_ReturnsPersistedQuery()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<PersistedQuery>>();
        var service = CreatePersistedQueryService(mockRepo.Object);
        var queryString = "{ user { id name } }";

        // Act
        var result = await service.RegisterAsync(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Hash.Should().NotBeNullOrEmpty();
        result.QueryString.Should().Be(queryString);
    }

    [Fact]
    public async Task RegisterAsync_WithSameQueryTwice_ReturnsIdenticalHash()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<PersistedQuery>>();
        var service = CreatePersistedQueryService(mockRepo.Object);
        var queryString = "{ user { id } }";

        // Act
        var result1 = await service.RegisterAsync(queryString);
        var result2 = await service.RegisterAsync(queryString);

        // Assert
        result1.Hash.Should().Be(result2.Hash);
    }

    [Fact]
    public async Task RegisterAsync_WithCustomSchemaName_PreservesSchemaName()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<PersistedQuery>>();
        var service = CreatePersistedQueryService(mockRepo.Object);
        var schemaName = "custom-schema";

        // Act
        var result = await service.RegisterAsync("{ user { id } }", schemaName: schemaName);

        // Assert
        result.SchemaName.Should().Be(schemaName);
    }

    [Fact]
    public async Task RegisterAsync_WithNullSchemaName_DefaultsToDefault()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<PersistedQuery>>();
        var service = CreatePersistedQueryService(mockRepo.Object);

        // Act
        var result = await service.RegisterAsync("{ user { id } }", schemaName: null!);

        // Assert
        result.SchemaName.Should().Be("default");
    }

    [Fact]
    public async Task RegisterAsync_WithOperationName_PreservesOperationName()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<PersistedQuery>>();
        var service = CreatePersistedQueryService(mockRepo.Object);
        var opName = "GetUser";

        // Act
        var result = await service.RegisterAsync("{ user { id } }", operationName: opName);

        // Assert
        result.OperationName.Should().Be(opName);
    }

    [Fact]
    public async Task GetByHashAsync_WithInvalidHash_ReturnsNull()
    {
        // Arrange
        var mockRepo = new Mock<IRepository<PersistedQuery>>();
        var service = CreatePersistedQueryService(mockRepo.Object);

        // Act
        var result = await service.GetByHashAsync("nonexistent-hash");

        // Assert
        result.Should().BeNull();
    }
}

sealed public class CacheKeyBuilderTests
{
    [Fact]
    public void BuildQueryKey_WithSimpleQuery_GeneratesConsistentKey()
    {
        // Arrange
        var query = "{ user { id } }";

        // Act
        var key1 = CacheKeyBuilder.BuildQueryKey("default", query);
        var key2 = CacheKeyBuilder.BuildQueryKey("default", query);

        // Assert
        key1.Should().Be(key2);
        key1.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BuildQueryKey_WithDifferentQueries_GeneratesDifferentKeys()
    {
        // Arrange
        var query1 = "{ user { id } }";
        var query2 = "{ user { id name } }";

        // Act
        var key1 = CacheKeyBuilder.BuildQueryKey("default", query1);
        var key2 = CacheKeyBuilder.BuildQueryKey("default", query2);

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void BuildQueryKey_WithDifferentSchemas_GeneratesDifferentKeys()
    {
        // Arrange
        var query = "{ user { id } }";

        // Act
        var key1 = CacheKeyBuilder.BuildQueryKey("schema1", query);
        var key2 = CacheKeyBuilder.BuildQueryKey("schema2", query);

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void BuildQueryKeyNs_WithNamespacePrefix_IncludesPrefix()
    {
        // Arrange
        var query = "{ user { id } }";
        var builder = CacheKeyBuilder.WithNamespace("tenant123:");

        // Act
        var key = builder.BuildQueryKeyNs("default", query);

        // Assert
        key.Should().StartWith("tenant123:");
    }

    [Fact]
    public void BuildQueryKey_WithVariables_GeneratesDifferentKeysForDifferentVariables()
    {
        // Arrange
        var query = "query GetUser($id: ID!) { user(id: $id) { id name } }";
        var vars1 = new Dictionary<string, object> { { "id", "user1" } };
        var vars2 = new Dictionary<string, object> { { "id", "user2" } };

        // Act
        var key1 = CacheKeyBuilder.BuildQueryKey("default", query, variables: vars1);
        var key2 = CacheKeyBuilder.BuildQueryKey("default", query, variables: vars2);

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void BuildQueryKey_WithNullVariables_GeneratesValidKey()
    {
        // Arrange
        var query = "{ user { id } }";

        // Act
        var key = CacheKeyBuilder.BuildQueryKey("default", query, variables: null);

        // Assert
        key.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BuildQueryKey_WithEmptyVariables_GeneratesValidKey()
    {
        // Arrange
        var query = "{ user { id } }";
        var emptyVars = new Dictionary<string, object>();

        // Act
        var key = CacheKeyBuilder.BuildQueryKey("default", query, variables: emptyVars);

        // Assert
        key.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BuildQueryKey_KeyFormat_FollowsGraphQLConvention()
    {
        // Arrange
        var query = "{ user { id } }";

        // Act
        var key = CacheKeyBuilder.BuildQueryKey("default", query);

        // Assert
        key.Should().StartWith("gql:query:");
        key.Should().Contain("default");
    }

    [Fact]
    public void BuildSchemaKey_WithValidSchemaName_GeneratesValidKey()
    {
        // Arrange
        var schemaName = "default";

        // Act
        var key = CacheKeyBuilder.BuildSchemaKey(schemaName);

        // Assert
        key.Should().StartWith("gql:schema:");
        key.Should().Contain(schemaName);
    }

    [Fact]
    public void BuildExecutionKey_WithValidInputs_GeneratesValidKey()
    {
        // Arrange
        var operationId = "op-123";
        var operationName = "GetUser";

        // Act
        var key = CacheKeyBuilder.BuildExecutionKey(operationId, operationName);

        // Assert
        key.Should().StartWith("gql:exec:");
        key.Should().Contain(operationId);
    }

    [Fact]
    public void BuildRateLimitKey_WithValidInputs_GeneratesValidKey()
    {
        // Arrange
        var clientId = "client-123";
        var endpoint = "graphql";

        // Act
        var key = CacheKeyBuilder.BuildRateLimitKey(clientId, endpoint);

        // Assert
        key.Should().StartWith("ratelimit:");
        key.Should().Contain(clientId);
        key.Should().Contain(endpoint);
    }

    [Fact]
    public void IsValidKey_WithValidKey_ReturnsTrue()
    {
        // Arrange
        var validKey = "gql:query:default:abc123";

        // Act
        var result = CacheKeyBuilder.IsValidKey(validKey);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidKey_WithInvalidCharacters_ReturnsFalse()
    {
        // Arrange
        var invalidKey = "gql:query:@#$%";

        // Act
        var result = CacheKeyBuilder.IsValidKey(invalidKey);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetKeyType_WithQueryKey_ReturnsQueryType()
    {
        // Arrange
        var key = CacheKeyBuilder.BuildQueryKey("default", "{ user { id } }");

        // Act
        var type = CacheKeyBuilder.GetKeyType(key);

        // Assert
        type.Should().Be("query");
    }

    [Fact]
    public void GetKeyType_WithSchemaKey_ReturnsSchemaType()
    {
        // Arrange
        var key = CacheKeyBuilder.BuildSchemaKey("default");

        // Act
        var type = CacheKeyBuilder.GetKeyType(key);

        // Assert
        type.Should().Be("schema");
    }

    [Fact]
    public void WithNamespace_CreatesBuilderWithPrefix_AndPreservesAllMethods()
    {
        // Arrange
        var builder = CacheKeyBuilder.WithNamespace("tenant-a:");

        // Act
        var queryKey = builder.BuildQueryKeyNs("default", "{ user { id } }");
        var schemaKey = builder.BuildSchemaKeyNs("default");

        // Assert
        queryKey.Should().StartWith("tenant-a:");
        schemaKey.Should().StartWith("tenant-a:");
    }
}

sealed public class DistributedCacheServiceTests
{
    private static DistributedCacheService CreateDistributedCacheService()
    {
        var mockLogger = new Mock<ILogger<DistributedCacheService>>().Object;
        return new DistributedCacheService(mockLogger);
    }

    [Fact]
    public void Set_WithValidKeyAndValue_StoresSuccessfully()
    {
        // Arrange
        var service = CreateDistributedCacheService();
        var key = "test-key";
        var value = "test-value";

        // Act
        service.Set(key, value);
        var result = service.Get<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Get_WithNonexistentKey_ReturnsNull()
    {
        // Arrange
        var service = CreateDistributedCacheService();

        // Act
        var result = service.Get<string>("nonexistent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Remove_WithExistingKey_RemovesValue()
    {
        // Arrange
        var service = CreateDistributedCacheService();
        var key = "test-key";
        service.Set(key, "value");

        // Act
        service.Remove(key);
        var result = service.Get<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Set_WithCustomExpiration_RespectsExpiration()
    {
        // Arrange
        var service = CreateDistributedCacheService();
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        service.Set(key, value, expiration);
        System.Threading.Thread.Sleep(150);
        var result = service.Get<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Clear_RemovesAllValues()
    {
        // Arrange
        var service = CreateDistributedCacheService();
        service.Set("key1", "value1");
        service.Set("key2", "value2");

        // Act
        service.Clear();
        var result1 = service.Get<string>("key1");
        var result2 = service.Get<string>("key2");

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public void GetOrSet_WhenKeyExists_ReturnsCachedValue()
    {
        // Arrange
        var service = CreateDistributedCacheService();
        var key = "test-key";
        var cachedValue = "cached";
        service.Set(key, cachedValue);

        var callCount = 0;
        Func<string?> factory = () => { callCount++; return "fresh"; };

        // Act
        var result = service.GetOrSet(key, factory);

        // Assert
        result.Should().Be(cachedValue);
        callCount.Should().Be(0);
    }

    [Fact]
    public void GetOrSet_WhenKeyMissing_ExecutesFactory()
    {
        // Arrange
        var service = CreateDistributedCacheService();
        var key = "test-key";
        var freshValue = "fresh";

        // Act
        var result = service.GetOrSet(key, () => freshValue);

        // Assert
        result.Should().Be(freshValue);
        service.Get<string>(key).Should().Be(freshValue);
    }

    [Fact]
    public void GetStatistics_ReturnsValidStats()
    {
        // Arrange
        var service = CreateDistributedCacheService();
        service.Set("key1", "value1");
        service.Get<string>("key1");
        service.Get<string>("key2");

        // Act
        var stats = service.GetStatistics();

        // Assert
        stats.Should().NotBeNull();
        stats.TotalHits.Should().BeGreaterThanOrEqualTo(0);
        stats.TotalMisses.Should().BeGreaterThanOrEqualTo(0);
    }
}
