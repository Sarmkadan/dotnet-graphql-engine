using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using GraphQLEngine.Configuration;
using GraphQLEngine.Domain.Entities;
using ExecutionContext = GraphQLEngine.Domain.Entities.ExecutionContext;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Benchmarks;

/// <summary>
/// Benchmarks for GraphQL query execution performance
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[GcServer(true)]
[GcConcurrent(true)]
public class QueryExecutionBenchmarks
{
    private ServiceProvider? _serviceProvider;
    private SchemaService? _schemaService;
    private GraphQLExecutionService? _executionService;
    private GraphQLQuery? _simpleQuery;
    private GraphQLQuery? _nestedQuery;
    private GraphQLQuery? _complexQuery;
    private GraphQLQuery? _largeQuery;
    private GraphQLQuery? _introspectionQuery;

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

        // Setup DI container
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(loggerFactory);
        services.AddGraphQLEngine(options =>
        {
            options.MaxQueryComplexity = 10000;
            options.EnableCaching = false;
        });

        _serviceProvider = services.BuildServiceProvider();
        _schemaService = _serviceProvider.GetRequiredService<SchemaService>();
        _executionService = _serviceProvider.GetRequiredService<GraphQLExecutionService>();

        // Define User type
        var userType = new GraphQLType
        {
            Name = "User",
            Description = "A user in the system",
            Kind = GraphQLTypeKind.Object
        };
        userType.AddField(new GraphQLField { Name = "id", ReturnType = "ID!", Description = "User ID" });
        userType.AddField(new GraphQLField { Name = "name", ReturnType = "String!", Description = "User name" });
        userType.AddField(new GraphQLField { Name = "email", ReturnType = "String!", Description = "User email" });
        userType.AddField(new GraphQLField { Name = "age", ReturnType = "Int", Description = "User age" });
        userType.AddField(new GraphQLField { Name = "posts", ReturnType = "[Post!]!", Description = "User posts" });

        _schemaService.AddType("BenchmarkSchema", userType);

        // Define Post type
        var postType = new GraphQLType
        {
            Name = "Post",
            Description = "A blog post",
            Kind = GraphQLTypeKind.Object
        };
        postType.AddField(new GraphQLField { Name = "id", ReturnType = "ID!", Description = "Post ID" });
        postType.AddField(new GraphQLField { Name = "title", ReturnType = "String!", Description = "Post title" });
        postType.AddField(new GraphQLField { Name = "content", ReturnType = "String!", Description = "Post content" });
        postType.AddField(new GraphQLField { Name = "authorId", ReturnType = "ID!", Description = "Author ID" });
        postType.AddField(new GraphQLField { Name = "comments", ReturnType = "[Comment!]!", Description = "Post comments" });
        postType.AddField(new GraphQLField { Name = "createdAt", ReturnType = "String!", Description = "Creation timestamp" });

        _schemaService.AddType("BenchmarkSchema", postType);

        // Define Comment type
        var commentType = new GraphQLType
        {
            Name = "Comment",
            Description = "A comment on a post",
            Kind = GraphQLTypeKind.Object
        };
        commentType.AddField(new GraphQLField { Name = "id", ReturnType = "ID!", Description = "Comment ID" });
        commentType.AddField(new GraphQLField { Name = "text", ReturnType = "String!", Description = "Comment text" });
        commentType.AddField(new GraphQLField { Name = "authorId", ReturnType = "ID!", Description = "Author ID" });
        commentType.AddField(new GraphQLField { Name = "postId", ReturnType = "ID!", Description = "Post ID" });

        _schemaService.AddType("BenchmarkSchema", commentType);

        // Define Query type
        var queryType = new GraphQLType
        {
            Name = "Query",
            Description = "Root query type",
            Kind = GraphQLTypeKind.Object
        };
        queryType.AddField(new GraphQLField { Name = "user", ReturnType = "User", Description = "Get a user" });
        queryType.AddField(new GraphQLField { Name = "users", ReturnType = "[User!]!", Description = "Get all users" });
        queryType.AddField(new GraphQLField { Name = "post", ReturnType = "Post", Description = "Get a post" });
        queryType.AddField(new GraphQLField { Name = "posts", ReturnType = "[Post!]!", Description = "Get all posts" });
        queryType.AddField(new GraphQLField { Name = "search", ReturnType = "[Post!]!", Description = "Search posts" });

        _schemaService.AddType("BenchmarkSchema", queryType);

        // Register resolvers
        _executionService.RegisterResolver("user", async (ExecutionContext context) => new { id = "1", name = "John Doe", email = "john@example.com", age = 30, posts = new object[] { } });
        _executionService.RegisterResolver("users", async (ExecutionContext context) =>
        {
            var users = new object[]
            {
                new { id = "1", name = "John Doe", email = "john@example.com", age = 30 },
                new { id = "2", name = "Jane Smith", email = "jane@example.com", age = 25 },
                new { id = "3", name = "Bob Johnson", email = "bob@example.com", age = 40 },
                new { id = "4", name = "Alice Brown", email = "alice@example.com", age = 35 },
                new { id = "5", name = "Charlie Wilson", email = "charlie@example.com", age = 28 }
            };
            return users;
        });
        _executionService.RegisterResolver("post", async (ExecutionContext context) => new { id = "1", title = "Hello World", content = "This is a post", authorId = "1", comments = new object[] { }, createdAt = DateTime.UtcNow.ToString("o") });
        _executionService.RegisterResolver("posts", async (ExecutionContext context) =>
        {
            var posts = new object[]
            {
                new { id = "1", title = "Hello World", content = "This is a post", authorId = "1", comments = new object[] { }, createdAt = DateTime.UtcNow.ToString("o") },
                new { id = "2", title = "Second Post", content = "Another post", authorId = "2", comments = new object[] { }, createdAt = DateTime.UtcNow.ToString("o") },
                new { id = "3", title = "Third Post", content = "More content", authorId = "1", comments = new object[] { }, createdAt = DateTime.UtcNow.ToString("o") },
                new { id = "4", title = "Fourth Post", content = "Yet another", authorId = "3", comments = new object[] { }, createdAt = DateTime.UtcNow.ToString("o") },
                new { id = "5", title = "Fifth Post", content = "Final one", authorId = "2", comments = new object[] { }, createdAt = DateTime.UtcNow.ToString("o") }
            };
            return posts;
        });
        _executionService.RegisterResolver("search", async (ExecutionContext context) =>
        {
            var searchTerm = context.GetArgument<string>("term");
            return new object[] { };
        });

        // Create benchmark queries
        _simpleQuery = new GraphQLQuery("{ user { id name email } }");
        _nestedQuery = new GraphQLQuery("{ users { id name posts { id title } } }");
        _complexQuery = new GraphQLQuery(
        """
        {
            users {
                id
                name
                email
                posts {
                    id
                    title
                    authorId
                }
            }
            posts {
                id
                title
                content
            }
        }
        """);

        // Large query with many fields and nested levels
        _largeQuery = new GraphQLQuery(
        """
        query LargeQuery {
            users {
                id
                name
                email
                age
                posts {
                    id
                    title
                    content
                    createdAt
                    comments {
                        id
                        text
                        authorId
                    }
                }
            }
            posts {
                id
                title
                content
                createdAt
                authorId
                comments {
                    id
                    text
                    authorId
                    postId
                }
            }
        }
        """);

        // Introspection query
        _introspectionQuery = new GraphQLQuery(
        """
        {
            __schema {
                types {
                    name
                    kind
                    description
                    fields {
                        name
                        type {
                            name
                            kind
                        }
                    }
                }
            }
        }
        """);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
    }

    [Benchmark]
    [BenchmarkCategory("Query Execution")]
    public async Task SimpleQuery()
    {
        var result = await _executionService!.ExecuteAsync(_simpleQuery!);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }
    }

    [Benchmark]
    [BenchmarkCategory("Query Execution")]
    public async Task NestedQuery()
    {
        var result = await _executionService!.ExecuteAsync(_nestedQuery!);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }
    }

    [Benchmark]
    [BenchmarkCategory("Query Execution")]
    public async Task ComplexQuery()
    {
        var result = await _executionService!.ExecuteAsync(_complexQuery!);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }
    }

    [Benchmark]
    [BenchmarkCategory("Query Execution")]
    public async Task LargeQuery()
    {
        var result = await _executionService!.ExecuteAsync(_largeQuery!);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }
    }

    [Benchmark]
    [BenchmarkCategory("Query Execution")]
    public async Task IntrospectionQuery()
    {
        var result = await _executionService!.ExecuteAsync(_introspectionQuery!);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }
    }

    [Benchmark]
    [BenchmarkCategory("Query Execution")]
    public async Task MultipleSimpleQueries()
    {
        for (int i = 0; i < 10; i++)
        {
            var result = await _executionService!.ExecuteAsync(_simpleQuery!);
            if (result.HasErrors)
            {
                throw new InvalidOperationException("Query execution failed");
            }
        }
    }

    [Benchmark]
    [BenchmarkCategory("Schema Operations")]
    public void CreateSchema()
    {
        var schema = _schemaService!.CreateSchema("TempSchema" + Guid.NewGuid());
        _schemaService.AddType("TempSchema", new GraphQLType
        {
            Name = "TempType",
            Kind = GraphQLTypeKind.Object
        });
    }

    [Benchmark]
    [BenchmarkCategory("Resolver Registration")]
    public void RegisterResolver()
    {
        _executionService!.RegisterResolver("tempField" + Guid.NewGuid(), async (ExecutionContext ctx) => new { value = 42 });
    }

    [Benchmark]
    [BenchmarkCategory("Query Execution")]
    public async Task QueryWithArguments()
    {
        var query = new GraphQLQuery("{ posts(term: \"test\") }");
        var result = await _executionService!.ExecuteAsync(query);
        if (result.HasErrors)
        {
            throw new InvalidOperationException("Query execution failed: " + string.Join(", ", result.Errors));
        }
    }
}