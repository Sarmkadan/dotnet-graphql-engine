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

// ... rest of the original README content ...
