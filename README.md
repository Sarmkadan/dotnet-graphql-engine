// ... rest of the original README content ...

## GraphQLExceptionExtensions

The `GraphQLExceptionExtensions` class provides a set of utility methods for working with `GraphQLException` instances. It allows you to easily add extensions, serialize extensions to JSON, and retrieve error codes.

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
It can turn a `Type` into a compact JSON representation that includes the type’s name, assembly‑qualified name and several characteristics (generic, abstract, value‑type), and it can recreate the `Type` from that JSON.

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
        //   "typeName":"System.Collections.Generic.Dictionary`2",
        //   "assemblyQualifiedName":"System.Collections.Generic.Dictionary`2[[System.String,...",
        //   "isGenericType":true,
        //   "isAbstract":false,
        //   "isValueType":false
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



// ... rest of the original README content ...
