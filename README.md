// ... rest of the original README content ...

## Event

The `Event` class represents a specific event that can be published and subscribed to within the event bus. It provides metadata about the event, including its ID, timestamp, source, and data.

### Usage Example

```csharp
using GraphQLEngine.Services.Events;

// Create an event with metadata
var event = new Event
{
    Id = "my-event-id",
    Timestamp = DateTime.UtcNow,
    Source = "MySource",
    Metadata = new Dictionary<string, object>
    {
        ["key"] = "value"
    },
    Data = new object[] { 1, 2, 3 }
};

// Publish the event
var eventBus = new EventBus();
eventBus.Publish(event);
```

## GraphQLHttpRequest

The `GraphQLHttpRequest` class provides a set of utility methods for configuring and validating GraphQL requests. It allows you to specify the query, operation name, and variables for a GraphQL request.

### Usage Example

```csharp
using GraphQLEngine.Configuration;

// Create a GraphQL request with query and variables
var request = new GraphQLHttpRequest
{
    Query = "query { hello }",
    Variables = new Dictionary<string, object?>
    {
        ["name"] = "John Doe"
    }
};

// Map the GraphQL request to an endpoint route
var endpoint = request.MapGraphQL();
```

## DependencyInjection

The `DependencyInjection` class provides extension methods for configuring dependency injection in .NET applications using the GraphQL engine. It registers all required services, repositories, and configuration options with the service collection, enabling easy integration into ASP.NET Core applications or other .NET host environments.

### Usage Example

```csharp
using GraphQLEngine.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Basic setup - registers all GraphQL engine services with default configuration
var services = new ServiceCollection();
services.AddGraphQLEngine();

// Configure GraphQL engine options
services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 5000;
    options.MaxQueryDepth = 10;
    options.QueryTimeoutMs = 30000;
    options.EnableDetailedErrorMessages = true;
    options.EnableIntrospection = true;
});

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Use the configured services
var executionService = serviceProvider.GetRequiredService<GraphQLExecutionService>();
```

### Predefined Configuration Profiles

```csharp
// Strict configuration with conservative limits
services.AddGraphQLEngineStrict();

// Permissive configuration with higher limits for development/testing
services.AddGraphQLEnginePermissive();

// Default configuration (same as AddGraphQLEngine())
service.AddGraphQLEngineDefault();
```

### Testing Support

```csharp
// Create a preconfigured service provider for unit tests
var testServiceProvider = DependencyInjection.CreateTestServiceProvider(options =>
{
    options.MaxQueryComplexity = 1000;
    options.EnableDetailedErrorMessages = false;
});

// Use in tests
var executionService = testServiceProvider.GetRequiredService<GraphQLExecutionService>();
```

// ... rest of the original README content ...
