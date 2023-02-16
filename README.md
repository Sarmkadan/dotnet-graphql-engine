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


// ... rest of the original README content ...
