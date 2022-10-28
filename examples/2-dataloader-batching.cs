#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Example 2: DataLoader for Batch Operations
///
/// Demonstrates how to prevent N+1 query problems using DataLoader.
/// Without DataLoader, fetching 100 users with their posts would result in
/// 101 database queries (1 for users + 100 for individual posts).
/// With DataLoader, it becomes 2 queries (1 for users + 1 for all posts).
///
/// This is essential for GraphQL performance.
/// </summary>

using Microsoft.Extensions.DependencyInjection;

sealed public class DataLoaderExample
{
    public static async Task Run(IServiceProvider serviceProvider)
    {
        var dataLoaderService = serviceProvider.GetRequiredService<DataLoaderService>();
        var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();

        // Register batch function for loading posts
        // This function is called ONCE per batch, not once per post ID
        dataLoaderService.RegisterBatchFunction("GetPostsByUserIds",
            async (userIds) =>
            {
                var ids = userIds.Cast<string>().ToList();
                Console.WriteLine($"Batch loading posts for {ids.Count} users");

                // Simulate database query
                var allPosts = GetMockPosts();
                return allPosts
                    .Where(p => ids.Contains(p.UserId))
                    .GroupBy(p => p.UserId)
                    .ToDictionary(g => (object)g.Key, g => (object)g.ToList());
            });

        // Register batch function for loading user comments
        dataLoaderService.RegisterBatchFunction("GetCommentsByPostIds",
            async (postIds) =>
            {
                var ids = postIds.Cast<string>().ToList();
                Console.WriteLine($"Batch loading comments for {ids.Count} posts");

                var allComments = GetMockComments();
                return allComments
                    .Where(c => ids.Contains(c.PostId))
                    .GroupBy(c => c.PostId)
                    .ToDictionary(g => (object)g.Key, g => (object)g.ToList());
            });

        // Example query that would cause N+1 without DataLoader
        var query = new GraphQLQuery(@"
        {
            users {
                id
                name
                posts {
                    id
                    title
                    comments {
                        id
                        text
                    }
                }
            }
        }");

        Console.WriteLine("Executing query with DataLoader...");
        var context = await executionService.ExecuteAsync(query);

        if (context.Errors.Any())
        {
            Console.WriteLine("Errors:");
            foreach (var error in context.Errors)
            {
                Console.WriteLine($"  - {error.Message}");
            }
        }
        else
        {
            Console.WriteLine("Success! Results retrieved efficiently.");
            Console.WriteLine($"Execution time: {context.Duration.TotalMilliseconds}ms");
        }
    }

    private static List<Post> GetMockPosts()
    {
        return new()
        {
            new Post { Id = "p1", UserId = "1", Title = "GraphQL Tips", Content = "..." },
            new Post { Id = "p2", UserId = "1", Title = "Performance", Content = "..." },
            new Post { Id = "p3", UserId = "2", Title = "C# Async", Content = "..." },
            new Post { Id = "p4", UserId = "3", Title = "Docker", Content = "..." }
        };
    }

    private static List<Comment> GetMockComments()
    {
        return new()
        {
            new Comment { Id = "c1", PostId = "p1", Text = "Great post!" },
            new Comment { Id = "c2", PostId = "p1", Text = "Very helpful" },
            new Comment { Id = "c3", PostId = "p2", Text = "Thanks for sharing" },
            new Comment { Id = "c4", PostId = "p3", Text = "Excellent explanation" }
        };
    }

    public record User(string Id, string Name);
    public record Post(string Id, string UserId, string Title, string Content);
    public record Comment(string Id, string PostId, string Text);
}
