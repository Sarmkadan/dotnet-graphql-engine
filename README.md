# dotnet-graphql-engine

A self-contained, code-first GraphQL engine for .NET 8 - parsing, execution, complexity 
analysis, DataLoader batching, caching, persisted queries and subscriptions, with no 
dependency on ASP.NET Core or any existing GraphQL library.

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full picture: source layout, 
component breakdown, data flow, design decisions and known limitations.

## GraphQLExceptionExtensions

The `GraphQLExceptionExtensions` class provides a set of utility methods for working with `GraphQLException` instances. 
It allows you to easily add extensions, serialize extensions to JSON, and retrieve error codes.

### Usage Example

```csharp
using GraphQLEngine.Exceptions;

// Create a GraphQL exception
var exception = new GraphQLException("Something went wrong");

// Add extensions to the exception
exception.AddExtensions(new Dictionary<string, object> 
{
    ["errorCode"] = "MY_ERROR_CODE",
    ["additionalInfo"] = "More information about the error"
});

// Serialize the exception extensions to JSON
var extensionsJson = exception.SerializeExtensions();

// Get the error code or a default value
var errorCode = exception.GetErrorCodeOrDefault("UNKNOWN_ERROR");

// Create a new exception with additional context
var newException = exception.WithContext("Additional context information");

// Retrieve a typed extension value
var errorCodeValue = exception.GetExtension<string>("errorCode");

// Add a formatted error code extension
exception.AddFormattedErrorCode("MY_PREFIX");
```

## ReflectionHelperJsonExtensions

`ReflectionHelperJsonExtensions` adds JSON‑serialization helpers for `System.Type` objects. 
It can turn a `Type` into a compact JSON representation that includes the type's name, assembly‑qualified name and several characteristics (generic, abstract, value‑type), and it can recreate the `Type` from that JSON.

### Usage Example

```csharp
using System;
using GraphQLEngine.Common.Utilities;

class Program
{
    static void Main()
    {
        // Choose a type to serialize
        Type originalType = typeof(System.Collections.Generic.Dictionary<string, int>);

        // Serialize the type information to JSON (indented for readability)
        string json = originalType.ToJson(indented: true);
        Console.WriteLine("Serialized JSON:");
        Console.WriteLine(json);
        // Example output:
        // {
        // "typeName":"System.Collections.Generic.Dictionary`2",
        // "assemblyQualifiedName":"System.Collections.Generic.Dictionary`2[[System.String,...",
        // "isGenericType":true,
        // "isAbstract":false,
        // "isValueType":false
        // }

        // Deserialize back to a Type instance
        Type? deserialized = ReflectionHelperJsonExtensions.FromJson(json);
        Console.WriteLine($"Deserialized equals original: {deserialized == originalType}");

        // Or use the TryFromJson pattern
        if (ReflectionHelperJsonExtensions.TryFromJson(json, out var tryDeserialized))
        {
            Console.WriteLine($"TryFromJson succeeded: {tryDeserialized}");
        }
    }
}
```

The JSON produced contains the following fields, which correspond to the public members of the internal `TypeInfo` representation:

- `typeName` – the full name of the type (`TypeName` property)
- `assemblyQualifiedName` – the assembly‑qualified name (`AssemblyQualifiedName` property)
- `isGenericType` – whether the type is generic (`IsGenericType` property)
- `isAbstract` – whether the type is abstract (`IsAbstract` property)
- `isValueType` – whether the type is a value type (`IsValueType` property)

These helpers are useful when you need to persist type metadata (e.g., in a cache or configuration file) and later reconstruct the exact `System.Type` at runtime.


## TypeConverterJsonExtensions

`TypeConverterJsonExtensions` provides JSON serialization and deserialization utilities that leverage `TypeConverter` for converting between .NET objects and JSON representations. It supports both generic and non-generic scenarios, allowing you to serialize any object to JSON and deserialize it back to its original type or a specified target type.


### Usage Example

```csharp
using System;
using GraphQLEngine.Common.Utilities;

class Program
{
    static void Main()
    {
        // Example 1: Serialize and deserialize a simple object
        var person = new { Name = "Alice", Age = 30, IsActive = true };

        // Serialize to JSON string
        string json = person.ToJson();
        Console.WriteLine("Serialized JSON:");
        Console.WriteLine(json);
        // Output: {"name":"Alice","age":30,"isActive":true}

        // Deserialize back to the same anonymous type
        var deserialized = new { Name = "", Age = 0, IsActive = false }.FromJson(json);
        Console.WriteLine($"Deserialized: {deserialized?.Name}, {deserialized?.Age}");

        // Example 2: Serialize and deserialize with indentation
        string indentedJson = person.ToJson(indented: true);
        Console.WriteLine("Indented JSON:");
        Console.WriteLine(indentedJson);

        // Example 3: Deserialize to a specific type
        string userJson = "{\"username\":\"bob\",\"email\":\"bob@example.com\",\"isPremium\":true}";
        var user = TypeConverterJsonExtensions.FromJson<User>(userJson);
        Console.WriteLine($"User: {user?.Username}, Premium: {user?.IsPremium}");

        // Example 4: Try pattern for safe deserialization
        if (TypeConverterJsonExtensions.TryFromJson(userJson, out User? safeUser))
        {
            Console.WriteLine("Successfully deserialized user with TryFromJson");
        }

        // Example 5: Deserialize to a specific target type
        string dataJson = "\"2024-01-15\"";
        var dateValue = TypeConverterJsonExtensions.FromJson(dataJson, typeof(DateTime));
        Console.WriteLine($"Parsed date: {dateValue}");

        // Example 6: Try pattern with target type
        if (TypeConverterJsonExtensions.TryFromJson(dataJson, typeof(DateTime), out var tryDate))
        {
            Console.WriteLine("Successfully parsed date with TryFromJson");
        }
    }
}

// Simple POCO for example
class User
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool IsPremium { get; set; }
}
```

The `TypeConverterJsonExtensions` class is particularly useful when you need to:

- Serialize complex .NET objects to JSON while preserving type information
- Deserialize JSON back to strongly-typed objects using .NET's type conversion system
- Handle both generic and non-generic scenarios with the same API
- Safely attempt deserialization without throwing exceptions on failure

The methods support both camelCase and indented formatting options for JSON output, making them suitable for both API responses and debugging scenarios.


## DependencyInjectionJsonExtensions

`DependencyInjectionJsonExtensions` provides JSON serialization and deserialization utilities for GraphQL engine configuration options. It supports both `GraphQLEngineOptions` and `DotnetGraphqlEngineOptions` types, allowing you to serialize configuration objects to JSON strings and deserialize them back to strongly-typed configuration instances. The extension methods handle null safety, provide both throwing and non-throwing patterns, and support both compact and indented JSON formatting.

### Usage Example

```csharp
using GraphQLEngine.Configuration;

class Program
{
    static void Main()
    {
        // Example 1: Serialize GraphQLEngineOptions to JSON
        var options = new GraphQLEngineOptions
        {
            Schema = "https://api.example.com/graphql",
            Timeout = TimeSpan.FromSeconds(30),
            MaxQueryDepth = 10
        };

        // Serialize to compact JSON string
        string json = options.ToJson();
        Console.WriteLine("Serialized GraphQLEngineOptions:");
        Console.WriteLine(json);

        // Serialize with indentation for readability
        string indentedJson = options.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON:");
        Console.WriteLine(indentedJson);

        // Example 2: Deserialize back to GraphQLEngineOptions
        string configJson = """{
            "schema": "https://api.example.com/graphql",
            "timeout": "00:00:30",
            "maxQueryDepth": 10
        }""";

        var deserializedOptions = DependencyInjectionJsonExtensions.FromJson(configJson);
        Console.WriteLine($"\nDeserialized schema: {deserializedOptions?.Schema}");
        Console.WriteLine($"Deserialized timeout: {deserializedOptions?.Timeout}");

        // Example 3: Try pattern for safe deserialization
        if (DependencyInjectionJsonExtensions.TryFromJson(configJson, out var safeOptions))
        {
            Console.WriteLine("Successfully deserialized with TryFromJson");
        }

        // Example 4: Serialize DotnetGraphqlEngineOptions
        var dotnetOptions = new DotnetGraphqlEngineOptions
        {
            EnableTracing = true,
            BatchRequests = true,
            SubscriptionProtocol = "WebSocket"
        };

        string dotnetJson = dotnetOptions.ToJson();
        Console.WriteLine("\nSerialized DotnetGraphqlEngineOptions:");
        Console.WriteLine(dotnetJson);

        // Example 5: Deserialize DotnetGraphqlEngineOptions
        string dotnetConfigJson = """{
            "enableTracing": true,
            "batchRequests": true,
            "subscriptionProtocol": "WebSocket"
        }""";

        var deserializedDotnetOptions = dotnetConfigJson.FromJsonDotnet();
        Console.WriteLine($"\nDeserialized enableTracing: {deserializedDotnetOptions?.EnableTracing}");

        // Example 6: Try pattern for DotnetGraphqlEngineOptions
        if (dotnetConfigJson.TryFromJson(out var tryDotnetOptions))
        {
            Console.WriteLine("Successfully deserialized DotnetGraphqlEngineOptions with TryFromJson");
        }
    }
}
```

The `DependencyInjectionJsonExtensions` class is particularly useful when you need to:

- Serialize configuration objects to JSON for storage or transmission
- Deserialize JSON configuration back to strongly-typed options
- Handle both `GraphQLEngineOptions` and `DotnetGraphqlEngineOptions` types with the same API
- Safely attempt deserialization without throwing exceptions on failure
- Support both compact and human-readable JSON formatting

The extension methods use camelCase property naming and ignore null values by default, making them suitable for both API configuration and configuration file scenarios.

## DependencyInjectionJsonExtensionsJsonExtensions

`DependencyInjectionJsonExtensionsJsonExtensions` provides additional JSON serialization and deserialization utilities for GraphQL engine configuration options. It complements the existing `DependencyInjectionJsonExtensions` class by offering methods for both `GraphQLEngineOptions` and `DotnetGraphqlEngineOptions` types, with support for both throwing and non-throwing patterns, as well as compact and indented JSON formatting.

### Usage Example

```csharp
using GraphQLEngine.Configuration;

class Program
{
    static void Main()
    {
        // Example 1: Serialize GraphQLEngineOptions to JSON
        var options = new GraphQLEngineOptions
        {
            Schema = "https://api.example.com/graphql",
            Timeout = TimeSpan.FromSeconds(30),
            MaxQueryDepth = 10
        };

        // Serialize to compact JSON string
        string json = options.ToJson();
        Console.WriteLine("Serialized GraphQLEngineOptions:");
        Console.WriteLine(json);

        // Serialize with indentation for readability
        string indentedJson = options.ToJson(indented: true);
        Console.WriteLine("\nIndented JSON:");
        Console.WriteLine(indentedJson);

        // Example 2: Deserialize back to GraphQLEngineOptions
        string configJson = """{
            "schema": "https://api.example.com/graphql",
            "timeout": "00:00:30",
            "maxQueryDepth": 10
        }""";

        var deserializedOptions = json.FromJson();
        Console.WriteLine($"\nDeserialized schema: {deserializedOptions?.Schema}");
        Console.WriteLine($"Deserialized timeout: {deserializedOptions?.Timeout}");

        // Example 3: Try pattern for safe deserialization
        if (json.TryFromJson(out var safeOptions))
        {
            Console.WriteLine("Successfully deserialized with TryFromJson");
        }

        // Example 4: Serialize DotnetGraphqlEngineOptions
        var dotnetOptions = new DotnetGraphqlEngineOptions
        {
            EnableTracing = true,
            BatchRequests = true,
            SubscriptionProtocol = "WebSocket"
        };

        string dotnetJson = dotnetOptions.ToJson();
        Console.WriteLine("\nSerialized DotnetGraphqlEngineOptions:");
        Console.WriteLine(dotnetJson);

        // Example 5: Deserialize DotnetGraphqlEngineOptions
        string dotnetConfigJson = """{
            "enableTracing": true,
            "batchRequests": true,
            "subscriptionProtocol": "WebSocket"
        }""";

        var deserializedDotnetOptions = dotnetConfigJson.FromJsonDotnet();
        Console.WriteLine($"\nDeserialized enableTracing: {deserializedDotnetOptions?.EnableTracing}");

        // Example 6: Try pattern for DotnetGraphqlEngineOptions
        if (dotnetConfigJson.TryFromJson(out var tryDotnetOptions))
        {
            Console.WriteLine("Successfully deserialized DotnetGraphqlEngineOptions with TryFromJson");
        }
    }
}
```

The `DependencyInjectionJsonExtensionsJsonExtensions` class is particularly useful when you need to:

- Serialize configuration objects to JSON for storage or transmission
- Deserialize JSON configuration back to strongly-typed options
- Handle both `GraphQLEngineOptions` and `DotnetGraphqlEngineOptions` types with consistent APIs
- Safely attempt deserialization without throwing exceptions on failure
- Support both compact and human-readable JSON formatting


The methods use camelCase property naming and ignore null values by default, making them suitable for both API configuration and configuration file scenarios.

## DependencyInjectionValidation


The `DependencyInjectionValidation` class provides validation utilities for GraphQL engine configuration and dependency injection setup. It offers extension methods to validate `GraphQLEngineOptions`, `DotnetGraphqlEngineOptions`, and `IServiceCollection` instances, ensuring proper configuration before application startup. Validation methods return detailed error messages for misconfigurations, while convenience methods provide boolean checks and exception-throwing variants.

### Usage Example

```csharp
using GraphQLEngine.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main()
    {
        // Example 1: Validate GraphQLEngineOptions
        var options = new GraphQLEngineOptions
        {
            Schema = "https://api.example.com/graphql",
            Timeout = TimeSpan.FromSeconds(30),
            MaxQueryDepth = 10
        };

        // Validate and get error list
        var validationErrors = options.Validate();
        if (validationErrors.Count > 0)
        {
            Console.WriteLine("Validation failed:");
            foreach (var error in validationErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
        else
        {
            Console.WriteLine("GraphQLEngineOptions is valid");
        }

        // Example 2: Check validity with IsValid
        bool isValid = options.IsValid();
        Console.WriteLine($"Is valid: {isValid}");

        // Example 3: Validate with EnsureValid (throws on failure)
        try
        {
            options.EnsureValid();
            Console.WriteLine("Options passed validation");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }

        // Example 4: Validate service collection configuration
        var services = new ServiceCollection();
        services.AddGraphQLEngine(); // Register GraphQL engine services

        var serviceErrors = services.Validate();
        if (serviceErrors.Count > 0)
        {
            Console.WriteLine("Service collection validation failed:");
            foreach (var error in serviceErrors)
            {
                Console.WriteLine($"- {error}");
            }
        }
        else
        {
            Console.WriteLine("Service collection is properly configured");
        }

        // Example 5: Check service collection validity
        bool servicesValid = services.IsValid();
        Console.WriteLine($"Services are valid: {servicesValid}");

        // Example 6: Validate service collection with EnsureValid
        try
        {
            services.EnsureValid();
            Console.WriteLine("Service collection passed validation");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Service validation failed: {ex.Message}");
        }
    }
}
```
