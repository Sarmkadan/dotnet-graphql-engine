#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Example 3: Query Complexity Analysis
///
/// Demonstrates how to prevent expensive/malicious queries using complexity analysis.
/// This protects your API from DoS attacks where clients submit deeply nested queries
/// that could consume excessive server resources.
///
/// Complexity scoring:
/// - Simple field: 1 point
/// - Array field: multiplied by estimated size
/// - Nested fields: accumulated
/// - Total must be below MaxQueryComplexity (default 5000)
/// </summary>

using Microsoft.Extensions.DependencyInjection;

sealed public class ComplexityAnalysisExample
{
    public static async Task Run(IServiceProvider serviceProvider)
    {
        var analysisService = serviceProvider.GetRequiredService<QueryAnalysisService>();
        var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();

        // Configure limits
        analysisService.SetMaxComplexity(5000);
        analysisService.SetMaxDepth(10);
        analysisService.SetMaxFields(200);

        // Example 1: Simple query (low complexity)
        AnalyzeQuery(analysisService, @"
        {
            user(id: ""1"") {
                id
                name
                email
            }
        }", "Simple Query");

        // Example 2: Query with list (medium complexity)
        AnalyzeQuery(analysisService, @"
        {
            users {
                id
                name
                posts {
                    id
                    title
                }
            }
        }", "Query with Lists");

        // Example 3: Deeply nested query (high complexity - might be rejected)
        AnalyzeQuery(analysisService, @"
        {
            user(id: ""1"") {
                posts {
                    comments {
                        author {
                            posts {
                                comments {
                                    author {
                                        name
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }", "Deeply Nested Query");

        // Example 4: Attempting to fetch too much data (rejected)
        AnalyzeQuery(analysisService, @"
        {
            users {
                id
                name
                posts(limit: 1000) {
                    id
                    title
                    comments(limit: 1000) {
                        id
                        text
                        author {
                            name
                        }
                    }
                }
            }
        }", "Expensive Query");

        // Example 5: Enforcing limits during execution
        Console.WriteLine("\n=== Enforcing Limits ===\n");
        var expensiveQuery = new GraphQLQuery(@"
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

        var analysis = analysisService.AnalyzeQuery(expensiveQuery);
        Console.WriteLine($"Query Complexity: {analysis.TotalComplexity}");
        Console.WriteLine($"Max Depth: {analysis.MaxDepth}");
        Console.WriteLine($"Field Count: {analysis.FieldCount}");
        Console.WriteLine($"Classification: {analysis.Level}");

        if (!analysisService.IsQueryAllowed(expensiveQuery))
        {
            Console.WriteLine("❌ REJECTED: Query exceeds complexity limits");
            Console.WriteLine("   This prevents denial-of-service attacks");
        }
        else
        {
            Console.WriteLine("✓ ALLOWED: Query passes complexity checks");
            var context = await executionService.ExecuteAsync(expensiveQuery);
            Console.WriteLine($"Execution time: {context.Duration.TotalMilliseconds}ms");
        }
    }

    private static void AnalyzeQuery(QueryAnalysisService analysisService, string queryString, string title)
    {
        var query = new GraphQLQuery(queryString);
        var analysis = analysisService.AnalyzeQuery(query);

        Console.WriteLine($"Query: {title}");
        Console.WriteLine($"  Complexity: {analysis.TotalComplexity}/5000");
        Console.WriteLine($"  Depth: {analysis.MaxDepth}/10");
        Console.WriteLine($"  Fields: {analysis.FieldCount}");
        Console.WriteLine($"  Level: {analysis.Level}");
        Console.WriteLine($"  Status: {(analysisService.IsQueryAllowed(query) ? "✓ ALLOWED" : "❌ REJECTED")}");
        Console.WriteLine();
    }
}

/// <summary>
/// Complexity Classifications:
///
/// LOW (0-1000): Simple queries
/// - Single object lookups: user(id: "1") { id name }
/// - Small list queries: users { id name }
/// Action: Execute immediately
///
/// MEDIUM (1000-3000): Standard queries
/// - Nested objects with lists: users { posts { comments { text } } }
/// - Multiple fields across types
/// Action: Cache results, apply normal rate limiting
///
/// HIGH (3000-6000): Complex queries
/// - Deeply nested structures with multiple list expansions
/// Action: May timeout, apply strict rate limiting
///
/// CRITICAL (>6000): Very expensive queries
/// - Extreme nesting, large list multipliers
/// Action: Rejected by default
///
/// Adjust limits in options based on your hardware and requirements.
/// </summary>
