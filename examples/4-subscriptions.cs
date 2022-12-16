// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/// <summary>
/// Example 4: Real-Time Subscriptions
///
/// Demonstrates how to implement real-time data updates using GraphQL subscriptions.
/// Subscriptions maintain persistent connections (via WebSocket) and push data
/// to clients when events occur.
///
/// This is essential for real-time applications like notifications, live updates,
/// collaborative editing, live feeds, etc.
/// </summary>

using Microsoft.Extensions.DependencyInjection;

public class SubscriptionExample
{
    public static async Task Run(IServiceProvider serviceProvider)
    {
        var subscriptionService = serviceProvider.GetRequiredService<SubscriptionService>();
        var eventBus = serviceProvider.GetRequiredService<EventBus>();

        // Example 1: Create subscription connections
        Console.WriteLine("=== Creating Subscription Connections ===\n");

        var client1Id = "client-1";
        var client2Id = "client-2";
        var client3Id = "client-3";

        var subscription1 = subscriptionService.CreateConnection(client1Id, "subscription { userUpdated { id name } }");
        var subscription2 = subscriptionService.CreateConnection(client2Id, "subscription { postCreated { id title } }");
        var subscription3 = subscriptionService.CreateConnection(client3Id, "subscription { userCreated { id email } }");

        Console.WriteLine($"✓ Created 3 subscription connections");
        Console.WriteLine($"  Active subscriptions: {subscriptionService.GetActiveSubscriptionCount()}");

        // Example 2: Subscribe to specific events
        Console.WriteLine("\n=== Setting Up Event Handlers ===\n");

        subscriptionService.Subscribe(client1Id, "UserUpdated", async (update) =>
        {
            Console.WriteLine($"[Client 1] User Updated: {update}");
            await Task.Delay(100);  // Simulate processing
        });

        subscriptionService.Subscribe(client2Id, "PostCreated", async (update) =>
        {
            Console.WriteLine($"[Client 2] Post Created: {update}");
            await Task.Delay(100);
        });

        subscriptionService.Subscribe(client3Id, "UserCreated", async (update) =>
        {
            Console.WriteLine($"[Client 3] User Created: {update}");
            await Task.Delay(100);
        });

        // Example 3: Publish events (simulate data changes)
        Console.WriteLine("\n=== Publishing Events ===\n");

        // Event 1: User updated
        var userUpdateEvent = new
        {
            id = "user-123",
            name = "Alice Johnson",
            updatedAt = DateTime.UtcNow
        };
        Console.WriteLine($"Publishing: User Updated (ID: {userUpdateEvent.id})");
        await eventBus.PublishAsync("UserUpdated", userUpdateEvent);
        await Task.Delay(500);

        // Event 2: Post created
        var postCreateEvent = new
        {
            id = "post-456",
            title = "GraphQL Best Practices",
            authorId = "user-123",
            createdAt = DateTime.UtcNow
        };
        Console.WriteLine($"\nPublishing: Post Created (ID: {postCreateEvent.id})");
        await eventBus.PublishAsync("PostCreated", postCreateEvent);
        await Task.Delay(500);

        // Event 3: Multiple subscribers might listen to same event
        Console.WriteLine($"\nPublishing: User Created");
        var userCreateEvent = new
        {
            id = "user-789",
            name = "Bob Smith",
            email = "bob@example.com"
        };
        await eventBus.PublishAsync("UserCreated", userCreateEvent);
        await Task.Delay(500);

        // Example 4: Unsubscribe from events
        Console.WriteLine("\n=== Unsubscribing ===\n");
        subscriptionService.Unsubscribe(client1Id, "UserUpdated");
        Console.WriteLine("✓ Client 1 unsubscribed from UserUpdated");

        // Event after unsubscribe won't be received by Client 1
        Console.WriteLine("\nPublishing: User Updated (Client 1 won't receive this)");
        var anotherUpdate = new { id = "user-123", name = "Alice Updated" };
        await eventBus.PublishAsync("UserUpdated", anotherUpdate);
        await Task.Delay(500);

        // Example 5: Close connection
        Console.WriteLine("\n=== Closing Connections ===\n");
        subscriptionService.CloseConnection(client2Id);
        Console.WriteLine("✓ Closed Client 2 connection");
        Console.WriteLine($"Active subscriptions: {subscriptionService.GetActiveSubscriptionCount()}");

        // Example 6: Real-world scenario - Live notification system
        Console.WriteLine("\n=== Real-World Example: Notification System ===\n");
        await SimulateNotificationSystem(subscriptionService, eventBus);
    }

    private static async Task SimulateNotificationSystem(SubscriptionService subscriptionService, EventBus eventBus)
    {
        // Create user connections
        var userId1 = "user-1";
        var userId2 = "user-2";

        subscriptionService.CreateConnection(userId1, "subscription { notificationReceived { id type message } }");
        subscriptionService.CreateConnection(userId2, "subscription { notificationReceived { id type message } }");

        // Subscribe to notifications
        subscriptionService.Subscribe(userId1, "NotificationReceived", async (notification) =>
        {
            Console.WriteLine($"[User 1] New notification: {notification}");
        });

        subscriptionService.Subscribe(userId2, "NotificationReceived", async (notification) =>
        {
            Console.WriteLine($"[User 2] New notification: {notification}");
        });

        // Simulate sending notifications
        var notifications = new[]
        {
            new { id = "notif-1", type = "like", message = "Alice liked your post", timestamp = DateTime.UtcNow },
            new { id = "notif-2", type = "comment", message = "Bob commented on your post", timestamp = DateTime.UtcNow },
            new { id = "notif-3", type = "follow", message = "Charlie started following you", timestamp = DateTime.UtcNow }
        };

        foreach (var notif in notifications)
        {
            Console.WriteLine($"Sending notification: {notif.type} - {notif.message}");
            await eventBus.PublishAsync("NotificationReceived", notif);
            await Task.Delay(1000);
        }

        Console.WriteLine($"\nFinal active subscriptions: {subscriptionService.GetActiveSubscriptionCount()}");
    }
}

/// <summary>
/// Subscription Architecture:
///
/// 1. Client connects via WebSocket
///    ├─ Connection established
///    └─ Client sends subscription query
///
/// 2. Server creates connection context
///    ├─ Validates subscription query
///    ├─ Registers connection
///    └─ Waits for events
///
/// 3. When event occurs
///    ├─ Event published via EventBus
///    ├─ All subscribers notified
///    ├─ Data sent to client
///    └─ Connection remains open for more events
///
/// 4. On disconnect
///    ├─ Clean up connection
///    ├─ Stop listening for events
///    └─ Release resources
///
/// Key Benefits:
/// - Real-time data delivery
/// - Reduced polling overhead
/// - Persistent connections
/// - Multiple subscribers per event
/// - Automatic reconnection handling
/// </summary>
