# DependencyInjectionExtensions
The `DependencyInjectionExtensions` class provides a set of extension methods for the `IServiceCollection` interface, allowing for the easy integration of the GraphQL engine into a .NET application using dependency injection. These methods enable the configuration of the GraphQL engine with various features, such as logging, validation, and schema stitching, making it easier to set up and customize the engine according to the application's needs.

## API
* `AddGraphQLEngineWithLogging`: Adds the GraphQL engine with logging capabilities to the service collection. This method takes no parameters and returns the `IServiceCollection` instance, allowing for method chaining. It does not throw any exceptions.
* `AddGraphQLEngineWithValidation`: Adds the GraphQL engine with validation capabilities to the service collection. This method takes no parameters and returns the `IServiceCollection` instance, allowing for method chaining. It does not throw any exceptions.
* `AddGraphQLEngineWithRepositoryLifetime`: Adds the GraphQL engine with a specified repository lifetime to the service collection. This method takes no parameters and returns the `IServiceCollection` instance, allowing for method chaining. It does not throw any exceptions.
* `AddGraphQLEngineWithSchemaStitching`: Adds the GraphQL engine with schema stitching capabilities to the service collection. This method takes no parameters and returns the `IServiceCollection` instance, allowing for method chaining. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `DependencyInjectionExtensions` class to configure the GraphQL engine in a .NET application:
```csharp
// Example 1: Adding the GraphQL engine with logging
var services = new ServiceCollection();
services.AddGraphQLEngineWithLogging();

// Example 2: Adding the GraphQL engine with validation and schema stitching
var services = new ServiceCollection();
services.AddGraphQLEngineWithValidation();
services.AddGraphQLEngineWithSchemaStitching();
```
Note that in a real-world application, you would typically call these methods in the `Startup.cs` file or wherever you configure your services.

## Notes
When using these extension methods, keep in mind that they do not throw any exceptions. However, the underlying services and features they configure may have their own exceptions and edge cases. For example, if the logging or validation features are not properly configured, they may not function as expected. Additionally, the thread-safety of these methods is dependent on the thread-safety of the `IServiceCollection` instance and the underlying services being configured. It is recommended to follow standard .NET dependency injection and service configuration best practices to ensure thread-safety and proper functionality.
