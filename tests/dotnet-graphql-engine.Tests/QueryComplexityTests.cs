#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GraphQLEngine.Configuration;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.GraphQL;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GraphQLEngine.Tests.Domain;

sealed public class QueryComplexityTests
{
    [Fact]
    public void RecordFieldComplexity_WhenCalled_AccumulatesTotalScoreAndFieldCount()
    {
        // Arrange
        var complexity = new QueryComplexity("test-query");

        // Act
        complexity.RecordFieldComplexity("users", 30);
        complexity.RecordFieldComplexity("posts", 70);

        // Assert
        complexity.TotalScore.Should().Be(100);
        complexity.FieldCount.Should().Be(2);
        complexity.FieldComplexities.Should().ContainKey("users").WhoseValue.Should().Be(30);
    }

    [Fact]
    public void RecordFieldComplexity_WithNegativeValue_ThrowsArgumentException()
    {
        // Arrange
        var complexity = new QueryComplexity("test-query");

        // Act
        Action act = () => complexity.RecordFieldComplexity("users", -5);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*negative*");
    }

    [Fact]
    public void CalculateLevel_WhenTotalScoreExceedsCriticalThreshold_SetsCriticalLevel()
    {
        // Arrange
        var complexity = new QueryComplexity("test-query") { TotalScore = 2500 };

        // Act
        complexity.CalculateLevel();

        // Assert
        complexity.Level.Should().Be(QueryComplexityLevel.Critical);
    }

    [Fact]
    public void IsAcceptable_WhenLevelIsCritical_ReturnsFalseRegardlessOfMaxScore()
    {
        // Arrange – score is within a generous max, but Critical level must still block it
        var complexity = new QueryComplexity("test-query") { TotalScore = 2001 };
        complexity.CalculateLevel();

        // Act
        var result = complexity.IsAcceptable(maxScore: 9999);

        // Assert
        result.Should().BeFalse();
    }
}

sealed public class CacheServiceTests
{
    private static CacheService CreateCacheService(bool enableCaching = true)
    {
        var mockLogger = new Mock<ILogger<CacheService>>();
        var options = new GraphQLEngineOptions
        {
            EnableCaching = enableCaching,
            CacheTTLSeconds = 300,
            CacheMaxSize = 1000
        };
        return new CacheService(mockLogger.Object, options);
    }

    [Fact]
    public void SetAndGet_WhenCachingIsEnabled_ReturnsTheStoredValue()
    {
        // Arrange
        var cache = CreateCacheService(enableCaching: true);
        const string key = "gql:query:users-list";
        const string value = "{ users { id name } }";

        // Act
        cache.Set(key, value);
        var result = cache.Get(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Get_WhenKeyDoesNotExist_ReturnsNull()
    {
        // Arrange
        var cache = CreateCacheService(enableCaching: true);

        // Act
        var result = cache.Get("nonexistent-cache-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Set_WhenCachingIsDisabled_GetReturnsNull()
    {
        // Arrange
        var cache = CreateCacheService(enableCaching: false);

        // Act
        cache.Set("gql:query:disabled", "some-result");
        var result = cache.Get("gql:query:disabled");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Remove_WhenKeyExists_ReturnsTrueAndKeyIsNoLongerAccessible()
    {
        // Arrange
        var cache = CreateCacheService(enableCaching: true);
        cache.Set("gql:schema:main", "schema-sdl");

        // Act
        var removed = cache.Remove("gql:schema:main");
        var afterRemoval = cache.Get("gql:schema:main");

        // Assert
        removed.Should().BeTrue();
        afterRemoval.Should().BeNull();
    }
}
