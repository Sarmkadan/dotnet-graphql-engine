#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Example 8: Unit Testing GraphQL Queries
///
/// Demonstrates how to write comprehensive unit tests for GraphQL queries,
/// mutations, and resolvers. Includes mocking, assertions, and test organization.
///
/// This is essential for ensuring your GraphQL API behaves correctly
/// and catches regressions early.
/// </summary>

using Moq;
using NUnit.Framework;

[TestFixture]
sealed public class GraphQLExecutionTests
{
    private ServiceProvider _serviceProvider;
    private GraphQLExecutionService _executionService;
    private SchemaService _schemaService;
    private Mock<IUserService> _mockUserService;

    [SetUp]
    public void SetUp()
    {
        // Setup mocks
        _mockUserService = new Mock<IUserService>();

        // Configure DI container
        var services = new ServiceCollection();
        services.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 5000;
            options.EnableCaching = false;  // Disable for testing
        });

        // Register mocks
        services.AddScoped(_ => _mockUserService.Object);

        _serviceProvider = services.BuildServiceProvider();
        _executionService = _serviceProvider.GetRequiredService<GraphQLExecutionService>();
        _schemaService = _serviceProvider.GetRequiredService<SchemaService>();

        SetupTestSchema();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    // Test: Simple Query Execution
    [Test]
    public async Task ExecuteQuery_SimpleUserQuery_ReturnsUserData()
    {
        // Arrange
        var expectedUser = new User { Id = "1", Name = "Alice Johnson", Email = "alice@example.com" };
        _mockUserService.Setup(s => s.GetUserAsync("1"))
            .ReturnsAsync(expectedUser);

        var query = new GraphQLQuery("{ user(id: \"1\") { id name email } }");

        // Act
        var context = await _executionService.ExecuteAsync(query);

        // Assert
        Assert.That(context.Errors, Is.Empty);
        Assert.That(context.Data, Is.Not.Null);
        Assert.That(context.Data["user"], Is.Not.Null);

        // Verify mock was called
        _mockUserService.Verify(s => s.GetUserAsync("1"), Times.Once);
    }

    // Test: Query with Variables
    [Test]
    public async Task ExecuteQuery_WithVariables_CorrectlyPassesVariablesToResolver()
    {
        // Arrange
        var userId = "user-123";
        var expectedUser = new User { Id = userId, Name = "Bob Smith", Email = "bob@example.com" };
        _mockUserService.Setup(s => s.GetUserAsync(userId))
            .ReturnsAsync(expectedUser);

        var query = new GraphQLQuery("query GetUser($id: ID!) { user(id: $id) { name } }");
        var variables = new Dictionary<string, object> { { "id", userId } };

        // Act
        var context = await _executionService.ExecuteAsync(query, variables);

        // Assert
        Assert.That(context.Errors, Is.Empty);
        _mockUserService.Verify(s => s.GetUserAsync(userId), Times.Once);
    }

    // Test: Query with Null Result
    [Test]
    public async Task ExecuteQuery_UserNotFound_ReturnsNull()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var query = new GraphQLQuery("{ user(id: \"nonexistent\") { id } }");

        // Act
        var context = await _executionService.ExecuteAsync(query);

        // Assert
        Assert.That(context.Errors, Is.Empty);
        var user = context.Data["user"];
        Assert.That(user, Is.Null);
    }

    // Test: Multiple Results
    [Test]
    public async Task ExecuteQuery_ListQuery_ReturnsMultipleResults()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = "1", Name = "Alice", Email = "alice@example.com" },
            new() { Id = "2", Name = "Bob", Email = "bob@example.com" },
            new() { Id = "3", Name = "Charlie", Email = "charlie@example.com" }
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        var query = new GraphQLQuery("{ users { id name } }");

        // Act
        var context = await _executionService.ExecuteAsync(query);

        // Assert
        Assert.That(context.Errors, Is.Empty);
        var result = context.Data["users"] as List<User>;
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0].Name, Is.EqualTo("Alice"));
    }

    // Test: Error Handling
    [Test]
    public async Task ExecuteQuery_ThrowsException_CapturesError()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserAsync(It.IsAny<string>()))
            .ThrowsAsync(new DbException("Database connection failed"));

        var query = new GraphQLQuery("{ user(id: \"1\") { id } }");

        // Act
        var context = await _executionService.ExecuteAsync(query);

        // Assert
        Assert.That(context.Errors, Is.Not.Empty);
        Assert.That(context.Errors[0].Message, Does.Contain("Database"));
    }

    // Test: Invalid Query
    [Test]
    public async Task ExecuteQuery_InvalidSyntax_ReturnsValidationError()
    {
        // Arrange
        var invalidQuery = new GraphQLQuery("{ user { invalidField } }");

        // Act
        var context = await _executionService.ExecuteAsync(invalidQuery);

        // Assert
        Assert.That(context.Errors, Is.Not.Empty);
        Assert.That(context.Errors[0].Message, Does.Contain("not found").IgnoreCase);
    }

    // Test: Query Complexity Limit
    [Test]
    public void ExecuteQuery_ComplexQueryExceedsLimit_RaisesError()
    {
        // Arrange
        var analysisService = _serviceProvider.GetRequiredService<QueryAnalysisService>();
        var complexQuery = new GraphQLQuery(@"
        {
            users {
                posts {
                    comments {
                        author {
                            friends {
                                posts {
                                    comments { text }
                                }
                            }
                        }
                    }
                }
            }
        }");

        // Act
        var isAllowed = analysisService.IsQueryAllowed(complexQuery);

        // Assert
        Assert.That(isAllowed, Is.False);
    }

    // Test: Caching Behavior
    [Test]
    public async Task ExecuteQuery_SameQueryTwice_SecondCallUsesCache()
    {
        // Arrange
        var cacheService = _serviceProvider.GetRequiredService<CacheService>();
        var query = new GraphQLQuery("{ users { id } }");

        _mockUserService.Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(new List<User>());

        // Act - First execution
        var context1 = await _executionService.ExecuteAsync(query);
        var firstCallCount = _mockUserService.Invocations.Count;

        // Act - Second execution (should use cache)
        var context2 = await _executionService.ExecuteAsync(query);
        var secondCallCount = _mockUserService.Invocations.Count;

        // Assert
        Assert.That(secondCallCount, Is.EqualTo(firstCallCount)); // No additional calls
        Assert.That(context1.Data, Is.EqualTo(context2.Data));
    }

    // Test: ExecutionContext Data
    [Test]
    public async Task ExecuteQuery_WithContextData_ResolverCanAccessIt()
    {
        // Arrange
        var query = new GraphQLQuery("{ user(id: \"1\") { id } }");
        var context = new ExecutionContext
        {
            ExecutionId = Guid.NewGuid().ToString(),
            Data = new Dictionary<string, object>
            {
                { "userId", "current-user" },
                { "isAdmin", true }
            }
        };

        _mockUserService.Setup(s => s.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(new User { Id = "1", Name = "Test" });

        // Act
        var result = await _executionService.ExecuteAsync(query, context);

        // Assert
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(context.Data["userId"], Is.EqualTo("current-user"));
        Assert.That(context.Data["isAdmin"], Is.True);
    }

    // Test: Mutation
    [Test]
    public async Task ExecuteMutation_CreateUser_CallsServiceAndReturnsResult()
    {
        // Arrange
        var newUser = new User { Id = "new-1", Name = "New User", Email = "new@example.com" };
        _mockUserService.Setup(s => s.CreateUserAsync("New User", "new@example.com"))
            .ReturnsAsync(newUser);

        var mutation = new GraphQLQuery(
            "mutation { createUser(name: \"New User\", email: \"new@example.com\") { id name } }");

        // Act
        var context = await _executionService.ExecuteAsync(mutation);

        // Assert
        Assert.That(context.Errors, Is.Empty);
        _mockUserService.Verify(
            s => s.CreateUserAsync("New User", "new@example.com"),
            Times.Once);
    }

    // Test: Performance
    [Test]
    public async Task ExecuteQuery_SimpleCachedQuery_CompletesQuickly()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(new List<User>());

        var query = new GraphQLQuery("{ users { id } }");

        // Warm up cache
        await _executionService.ExecuteAsync(query);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _executionService.ExecuteAsync(query);
        stopwatch.Stop();

        // Assert
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(10)); // Cached query < 10ms
    }

    private void SetupTestSchema()
    {
        var schema = _schemaService.CreateSchema("TestAPI");

        var userType = new GraphQLType
        {
            Name = "User",
            Fields = new List<GraphQLField>
            {
                new() { Name = "id", Type = "ID!", Description = "User ID" },
                new() { Name = "name", Type = "String!", Description = "User name" },
                new() { Name = "email", Type = "String", Description = "User email" }
            }
        };

        _schemaService.AddType("TestAPI", userType);

        var queryType = new GraphQLType
        {
            Name = "Query",
            Fields = new List<GraphQLField>
            {
                new()
                {
                    Name = "user",
                    Type = "User",
                    Arguments = new List<GraphQLField>
                    {
                        new() { Name = "id", Type = "ID!", Description = "User ID" }
                    }
                },
                new()
                {
                    Name = "users",
                    Type = "[User!]!",
                    Description = "Get all users"
                }
            }
        };

        _schemaService.AddType("TestAPI", queryType);
    }
}

/// <summary>
/// Testing Best Practices:
///
/// 1. ISOLATION: Each test is independent
///    - SetUp/TearDown ensures clean state
///    - Mocks prevent external dependencies
///    - No shared state between tests
///
/// 2. CLARITY: Test names clearly describe what's being tested
///    - Method_Scenario_ExpectedResult pattern
///    - Arrange-Act-Assert structure
///    - One assertion per test (or related assertions)
///
/// 3. COVERAGE: Test both happy path and error cases
///    - Valid queries return correct results
///    - Invalid queries return errors
///    - Edge cases are handled
///
/// 4. MOCKING: Use mocks to isolate GraphQL engine
///    - Mock data services
///    - Mock external APIs
///    - Control mock behavior precisely
///
/// 5. PERFORMANCE: Include performance tests
///    - Cached queries are fast
///    - Complex queries are slow (as expected)
///    - Memory usage is reasonable
///
/// 6. MAINTAINABILITY: Keep tests simple and readable
///    - Short tests (< 30 lines)
///    - Descriptive variable names
///    - Reusable test fixtures
/// </summary>
