// ... rest of the original README content ...

## PersistedQueryExtensions

The `PersistedQueryExtensions` class provides a set of utility methods for configuring and validating persisted queries. It allows you to add persisted queries to the service collection, enforce hash verification, set the maximum index size, and configure other settings.

### Usage Example

```csharp
using GraphQLEngine.Configuration;

// Add persisted queries to the service collection
var services = new ServiceCollection();
services.AddPersistedQueries();

// Configure persisted query settings
services.AddPersistedQueries(options => 
{
  options.EnforceHashVerification = true;
  options.MaxIndexSize = 1024;
  options.AllowlistOnly = false;
  options.ReturnNotFoundError = true;
  options.Validate = true;
});
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