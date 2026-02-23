// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Example 5: Schema Stitching
///
/// Demonstrates how to compose multiple GraphQL APIs into a single unified schema.
/// This allows clients to query across multiple services transparently, as if
/// they were a single GraphQL API.
///
/// Useful for:
/// - Combining legacy and modern services
/// - Federation of microservices
/// - Hiding internal API boundaries
/// - Progressive migration of services
/// </summary>

using Microsoft.Extensions.DependencyInjection;

public class SchemastItchingExample
{
    public static async Task Run(IServiceProvider serviceProvider)
    {
        var schemaService = serviceProvider.GetRequiredService<SchemaService>();
        var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();

        // Example 1: Create main local schema
        Console.WriteLine("=== Setting Up Main Schema ===\n");
        var mainSchema = schemaService.CreateSchema("MainAPI");

        var userType = new GraphQLType
        {
            Name = "User",
            Description = "Local user type",
            Fields = new List<GraphQLField>
            {
                new() { Name = "id", Type = "ID!", Description = "User ID" },
                new() { Name = "name", Type = "String!", Description = "User name" },
                new() { Name = "email", Type = "String!", Description = "User email" }
            }
        };

        schemaService.AddType("MainAPI", userType);
        Console.WriteLine("✓ Added User type to main schema");

        // Example 2: Stitch user service schema
        Console.WriteLine("\n=== Stitching User Service ===\n");
        var userServiceConfig = new SchemaStitchingConfig
        {
            Enabled = true,
            BaseUrl = "http://user-service.local",
            DiscoveryEndpoint = "/graphql",
            Timeout = TimeSpan.FromSeconds(30)
        };

        try
        {
            var stitchedWithUsers = await schemaService.StitchSchemaAsync(mainSchema, userServiceConfig);
            Console.WriteLine("✓ Successfully stitched User Service schema");
            Console.WriteLine("  Available types: User, UserProfile, UserSettings");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("⚠ User Service is unavailable (using fallback schema)");
        }

        // Example 3: Stitch post service schema
        Console.WriteLine("\n=== Stitching Post Service ===\n");
        var postServiceConfig = new SchemaStitchingConfig
        {
            Enabled = true,
            BaseUrl = "http://post-service.local",
            DiscoveryEndpoint = "/graphql",
            Timeout = TimeSpan.FromSeconds(30)
        };

        try
        {
            var stitchedWithPosts = await schemaService.StitchSchemaAsync(mainSchema, postServiceConfig);
            Console.WriteLine("✓ Successfully stitched Post Service schema");
            Console.WriteLine("  Available types: Post, Comment, Tag");
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("⚠ Post Service is unavailable");
        }

        // Example 4: Query across stitched schemas
        Console.WriteLine("\n=== Querying Stitched Schema ===\n");
        var crossServiceQuery = new GraphQLQuery(@"
        {
            user(id: ""1"") {
                id
                name
                email
                # From stitched User Service
                profile {
                    bio
                    avatar
                }
                # From stitched Post Service
                posts {
                    id
                    title
                    content
                    comments {
                        id
                        text
                        author {
                            name
                        }
                    }
                }
            }
        }");

        Console.WriteLine("Executing cross-service query...");
        var context = await executionService.ExecuteAsync(crossServiceQuery);

        if (context.Errors.Any())
        {
            Console.WriteLine("Query errors:");
            foreach (var error in context.Errors)
            {
                Console.WriteLine($"  - {error.Message} (field: {error.Field})");
            }
        }
        else
        {
            Console.WriteLine("✓ Query successful");
            Console.WriteLine($"Response received from multiple services in {context.Duration.TotalMilliseconds}ms");
        }

        // Example 5: Dynamic schema composition
        Console.WriteLine("\n=== Dynamic Schema Composition ===\n");
        var services = new Dictionary<string, SchemaStitchingConfig>
        {
            {
                "UserService",
                new SchemaStitchingConfig
                {
                    Enabled = true,
                    BaseUrl = "http://user-service.local",
                    DiscoveryEndpoint = "/graphql",
                    Timeout = TimeSpan.FromSeconds(30)
                }
            },
            {
                "PostService",
                new SchemaStitchingConfig
                {
                    Enabled = true,
                    BaseUrl = "http://post-service.local",
                    DiscoveryEndpoint = "/graphql",
                    Timeout = TimeSpan.FromSeconds(30)
                }
            },
            {
                "NotificationService",
                new SchemaStitchingConfig
                {
                    Enabled = true,
                    BaseUrl = "http://notification-service.local",
                    DiscoveryEndpoint = "/graphql",
                    Timeout = TimeSpan.FromSeconds(30)
                }
            }
        };

        Console.WriteLine("Composing unified schema from services:");
        foreach (var (serviceName, config) in services)
        {
            Console.WriteLine($"  - {serviceName}: {config.BaseUrl}");
        }

        // In production, you'd do this asynchronously and handle failures gracefully
        var composedSchema = mainSchema;
        foreach (var (serviceName, config) in services)
        {
            try
            {
                composedSchema = await schemaService.StitchSchemaAsync(composedSchema, config);
                Console.WriteLine($"    ✓ {serviceName} stitched successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ⚠ {serviceName} failed: {ex.Message}");
            }
        }

        // Example 6: Export stitched schema
        Console.WriteLine("\n=== Exported Schema (SDL) ===\n");
        var sdl = schemaService.ExportAsSDL("MainAPI");
        Console.WriteLine("Schema includes types from:");
        Console.WriteLine("  - Local User type");
        Console.WriteLine("  - User Service types (UserProfile, UserSettings, etc.)");
        Console.WriteLine("  - Post Service types (Post, Comment, Tag, etc.)");
        Console.WriteLine("  - Notification Service types");
        Console.WriteLine("\nClients can query against unified schema transparently!");
    }
}

/// <summary>
/// Schema Stitching Architecture:
///
/// Before Stitching (Monolithic):
///    Client ──────┬─────────┬─────────┬──────────
///                 │         │         │
///         User API   Post API  Notification API
///    (Requires multiple endpoints)
///
///
/// After Stitching (Federated):
///    Client ─────────── Gateway API ──────┬─────────┬──────────
///                                         │         │
///                               User API  Post API  Notification API
///    (Single unified endpoint)
///
///
/// Benefits:
/// 1. Single client endpoint (no hardcoded service URLs)
/// 2. Transparent cross-service queries
/// 3. Centralized authentication/authorization
/// 4. Easy to evolve service boundaries
/// 5. Backward compatible service migration
///
///
/// Common Patterns:
///
/// 1. Transparent Field Resolution:
///    query {
///        user(id: "1") {           ──→ User Service
///            id
///            name
///            posts {               ──→ Post Service
///                id
///                title
///            }
///        }
///    }
///
/// 2. Reference Resolution:
///    When User Service returns user with postIds,
///    Post Service automatically resolves full post objects
///
/// 3. Fragment Composition:
///    Different services define types that reference each other
///    Gateway stitches them together
/// </summary>
