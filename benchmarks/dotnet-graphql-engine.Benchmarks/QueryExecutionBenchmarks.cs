using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using GraphQLEngine.Domain.Entities;
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
            Kind = TypeKind.Object
        };
        userType.AddField(new GraphQLField { Name = "id", Type = "ID!", Description = "User ID" });
        userType.AddField(new GraphQLField { Name = "name", Type = "String!", Description = "User name" });
        userType.AddField(new GraphQLField { Name = "email", Type = "String!", Description = "User email" });
        userType.AddField(new GraphQLField { Name = "age", Type = "Int", Description = "User age" });
        userType.AddField(new GraphQLField { Name = "posts", Type = "[Post!]!", Description = "User posts" });

        _schemaService.AddType("BenchmarkSchema", userType);

        // Define Post type
        var postType = new GraphQLType
        {
            Name = "Post",
            Description = "A blog post",
            Kind = TypeKind.Object
        };
        postType.AddField(new GraphQLField { Name = "id", Type = "ID!", Description = "Post ID" });
        postType.AddField(new GraphQLField { Name = "title", Type = "String!", Description = "Post title" });
        postType.AddField(new GraphQLField { Name = "content", Type = "String!", Description = "Post content" });
        postType.AddField(new GraphQLField { Name = "authorId", Type = "ID!", Description = "Author ID" });
        postType.AddField(new GraphQLField { Name = "comments", Type = "[Comment!]!", Description = "Post comments" });
        postType.AddField(new GraphQLField { Name = "createdAt", Type = "String!", Description = "Creation timestamp" });

        _schemaService.AddType("BenchmarkSchema", postType);

        // Define Comment type
        var commentType = new GraphQLType
        {
            Name = "Comment",
            Description = "A comment on a post",
            Kind = TypeKind.Object
        };
        commentType.AddField(new GraphQLField { Name = "id", Type = "ID!", Description = "Comment ID" });
        commentType.AddField(new GraphQLField { Name = "text", Type = "String!", Description = "Comment text" });
        commentType.AddField(new GraphQLField { Name = "authorId", Type = "ID!", Description = "Author ID" });
        commentType.AddField(new GraphQLField { Name = "postId", Type = "ID!", Description = "Post ID" });

        _schemaService.AddType("BenchmarkSchema", commentType);

        // Define Query type
        var queryType = new GraphQLType
        {
            Name = "Query",
            Description = "Root query type",
            Kind = TypeKind.Object
        };
        queryType.AddField(new GraphQLField { Name = "user", Type = "User", Description = "Get a user" });
        queryType.AddField(new GraphQLField { Name = "users", Type = "[User!]!", Description = "Get all users" });
        queryType.AddField(new GraphQLField { Name = "post", Type = "Post", Description = "Get a post" });
        queryType.AddField(new GraphQLField { Name = "posts", Type = "[Post!]!", Description = "Get all posts" });
        queryType.AddField(new GraphQLField { Name = "search", Type = "[Post!]!", Description = "Search posts" });

        _schemaService.AddType("BenchmarkSchema", queryType);

        // Register resolvers
        _executionService.RegisterResolver("user", async (context) => new { id = "1", name = "John Doe", email = "john@example.com", age = 30, posts = new object[] { } });
        _executionService.RegisterResolver("users", async (context) =>
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
        _executionService.RegisterResolver("post", async (context) => new { id = "1", title = "Hello World", content = "This is a post", authorId = "1", comments = new object[] { }, createdAt = DateTime.UtcNow.ToString("o") });
        _executionService.RegisterResolver("posts", async (context) =>
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
        _executionService.RegisterResolver("search", async (context) =>
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
            Kind = TypeKind.Object
        });
    }

    [Benchmark]
    [BenchmarkCategory("Resolver Registration")]
    public void RegisterResolver()
    {
        _executionService!.RegisterResolver("tempField" + Guid.NewGuid(), async (ctx) => new { value = 42 });
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