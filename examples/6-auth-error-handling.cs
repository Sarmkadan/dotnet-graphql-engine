// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Example 6: Authentication and Error Handling
///
/// Demonstrates proper error handling in GraphQL queries and implementing
/// authentication/authorization in resolvers.
///
/// This is critical for production applications to handle errors gracefully
/// and protect sensitive data.
/// </summary>

using Microsoft.Extensions.DependencyInjection;

public class AuthAndErrorHandlingExample
{
    public static async Task Run(IServiceProvider serviceProvider)
    {
        var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();
        var errorFormatter = serviceProvider.GetRequiredService<ErrorFormattingService>();

        // Example 1: Handling validation errors
        Console.WriteLine("=== Example 1: Validation Errors ===\n");
        await HandleValidationErrors(executionService);

        // Example 2: Authorization errors
        Console.WriteLine("\n=== Example 2: Authorization Errors ===\n");
        await HandleAuthorizationErrors(executionService);

        // Example 3: Not found errors
        Console.WriteLine("\n=== Example 3: Not Found Errors ===\n");
        await HandleNotFoundErrors(executionService);

        // Example 4: Timeout errors
        Console.WriteLine("\n=== Example 4: Timeout Errors ===\n");
        await HandleTimeoutErrors(executionService);

        // Example 5: Error formatting for different environments
        Console.WriteLine("\n=== Example 5: Environment-Specific Error Formatting ===\n");
        await DemonstrateErrorFormatting(executionService, errorFormatter);
    }

    private static async Task HandleValidationErrors(GraphQLExecutionService executionService)
    {
        var invalidQuery = new GraphQLQuery("{ user { invalidField } }");

        try
        {
            var context = await executionService.ExecuteAsync(invalidQuery);

            if (context.Errors.Any())
            {
                Console.WriteLine("❌ Query validation failed:");
                foreach (var error in context.Errors)
                {
                    Console.WriteLine($"  Error: {error.Message}");
                    Console.WriteLine($"  Field: {error.Field}");
                    Console.WriteLine($"  Location: Line {error.LineNumber}, Column {error.ColumnNumber}");
                }
            }
        }
        catch (GraphQLException ex)
        {
            Console.WriteLine($"GraphQL Exception: {ex.Message}");
        }
    }

    private static async Task HandleAuthorizationErrors(GraphQLExecutionService executionService)
    {
        Console.WriteLine("Scenario: User tries to access sensitive data without permission\n");

        var sensitiveQuery = new GraphQLQuery(@"
        {
            user(id: ""admin-123"") {
                id
                name
                email
                # Sensitive data - requires admin role
                internalNotes
                apiKeys
            }
        }");

        // Simulate context with non-admin user
        var context = new ExecutionContext
        {
            ExecutionId = Guid.NewGuid().ToString(),
            Data = new Dictionary<string, object>
            {
                { "userId", "user-456" },
                { "userRole", "viewer" }  // Not admin
            }
        };

        var result = await executionService.ExecuteAsync(sensitiveQuery, context);

        if (result.Errors.Any())
        {
            Console.WriteLine("❌ Authorization denied:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  {error.Message}");
                if (error.Extensions.TryGetValue("code", out var code))
                {
                    Console.WriteLine($"  Code: {code}");
                }
            }
        }
        else
        {
            Console.WriteLine("⚠ Should have been blocked!");
        }
    }

    private static async Task HandleNotFoundErrors(GraphQLExecutionService executionService)
    {
        Console.WriteLine("Scenario: Querying non-existent user\n");

        var notFoundQuery = new GraphQLQuery("{ user(id: \"nonexistent-999\") { id name } }");
        var context = await executionService.ExecuteAsync(notFoundQuery);

        if (context.Errors.Any())
        {
            Console.WriteLine("❌ Resource not found:");
            foreach (var error in context.Errors)
            {
                Console.WriteLine($"  Message: {error.Message}");
                Console.WriteLine($"  Extensions: {string.Join(", ", error.Extensions)}");
            }
        }
    }

    private static async Task HandleTimeoutErrors(GraphQLExecutionService executionService)
    {
        Console.WriteLine("Scenario: Query exceeds timeout\n");

        // Very complex query that might timeout
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

        try
        {
            var context = await executionService.ExecuteAsync(complexQuery);

            if (context.Errors.Any())
            {
                foreach (var error in context.Errors)
                {
                    if (error.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("❌ Query timeout:");
                        Console.WriteLine($"  {error.Message}");
                        Console.WriteLine($"  Execution time: {context.Duration.TotalMilliseconds}ms");
                    }
                }
            }
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"❌ Query timeout: {ex.Message}");
        }
    }

    private static async Task DemonstrateErrorFormatting(
        GraphQLExecutionService executionService,
        ErrorFormattingService errorFormatter)
    {
        Console.WriteLine("Error formatting for different environments:\n");

        var query = new GraphQLQuery("{ user { invalidField } }");
        var context = await executionService.ExecuteAsync(query);

        if (context.Errors.Any())
        {
            var error = context.Errors.First();

            // Development: Detailed error
            Console.WriteLine("DEVELOPMENT Environment:");
            Console.WriteLine(new
            {
                error.Message,
                error.Field,
                error.LineNumber,
                error.ColumnNumber,
                error.Path,
                extensions = error.Extensions
            });

            // Production: Sanitized error
            Console.WriteLine("\nPRODUCTION Environment:");
            Console.WriteLine(new
            {
                message = "Query validation error",
                // StackTrace and internal details removed
                code = "GRAPHQL_VALIDATION_ERROR"
            });
        }
    }
}

/// <summary>
/// Error Handling Best Practices:
///
/// 1. DEVELOPMENT vs PRODUCTION
///    Dev: Include full error details, stack traces, field paths
///    Prod: Sanitized messages, generic "error" descriptions
///
/// 2. ERROR TYPES
///    - Validation Errors: Invalid query syntax/structure
///    - Execution Errors: Runtime errors in resolvers
///    - Authorization Errors: Missing permissions
///    - Not Found: Resource doesn't exist
///    - Timeout: Query took too long
///
/// 3. ERROR STRUCTURE
///    {
///        message: "Human-readable message",
///        extensions: {
///            code: "ERROR_CODE",
///            field: "fieldName",
///            // Additional context
///        }
///    }
///
/// 4. AUTHENTICATION FLOW
///    a) Extract token from Authorization header
///    b) Verify token signature
///    c) Decode token to get user info
///    d) Store user info in ExecutionContext
///    e) Check permissions in resolvers
///
/// 5. AUTHORIZATION IN RESOLVERS
///    public async Task<User> GetUserAsync(string id, ExecutionContext context)
///    {
///        // Get user from context
///        var currentUserId = context.GetHeader("X-User-Id");
///        var isAdmin = context.GetData("isAdmin") is bool b && b;
///
///        // Check permission
///        if (currentUserId != id && !isAdmin)
///        {
///            throw new GraphQLException("Not authorized to view this user");
///        }
///
///        return await _userService.GetUserAsync(id);
///    }
///
/// 6. SECURE ERROR MESSAGES
///    Bad:  \"User 'alice@example.com' not found\"
///          (Leaks that email exists in system)
///
///    Good: \"User not found\"
///          (Doesn't confirm email existence)
///
/// 7. LOGGING
///    - Log full errors internally
///    - Return sanitized errors to clients
///    - Track error patterns for debugging
/// </summary>
