# GraphQLExceptionExtensions

`GraphQLExceptionExtensions` is a static utility class that provides extension methods for enhancing and manipulating `GraphQLException` instances with additional context, error codes, and serialized extension data. These methods facilitate richer error handling and debugging in GraphQL applications by allowing structured metadata to be attached to exceptions and retrieved in a type-safe manner.

## API

### `AddExtensions`

Adds a collection of key-value pairs to the extensions of a `GraphQLException`.

**Declaration**
```csharp
public static void AddExtensions(this GraphQLException exception, IEnumerable<KeyValuePair<string, object>> extensions)
```

**Parameters**
- `exception`: The `GraphQLException` to which extensions will be added. Must not be `null`.
- `extensions`: A collection of key-value pairs representing the extensions to add. Keys must not be `null`, and values must be serializable.

**Returns**
- `void`

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` or `extensions` is `null`.
- `ArgumentException`: Thrown when any key in `extensions` is `null`.

---

### `SerializeExtensions`

Serializes the extensions of a `GraphQLException` into a JSON string.

**Declaration**
```csharp
public static string SerializeExtensions(this GraphQLException exception)
```

**Parameters**
- `exception`: The `GraphQLException` whose extensions will be serialized. Must not be `null`.

**Returns**
- A JSON string representation of the exception's extensions. Returns an empty string if no extensions are present.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` is `null`.

---

### `GetErrorCodeOrDefault`

Retrieves the error code from a `GraphQLException`, or returns a specified default value if the code is not present.

**Declaration**
```csharp
public static string GetErrorCodeOrDefault(this GraphQLException exception, string defaultValue)
```

**Parameters**
- `exception`: The `GraphQLException` from which to retrieve the error code. Must not be `null`.
- `defaultValue`: The value to return if the error code is not present. Can be `null`.

**Returns**
- The error code string if present; otherwise, `defaultValue`.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` is `null`.

---

### `WithContext`

Creates a new `GraphQLException` with additional context data merged into its extensions.

**Declaration**
```csharp
public static GraphQLException WithContext(this GraphQLException exception, IEnumerable<KeyValuePair<string, object>> context)
```

**Parameters**
- `exception`: The original `GraphQLException`. Must not be `null`.
- `context`: A collection of key-value pairs to add as context. Keys must not be `null`.

**Returns**
- A new `GraphQLException` instance with merged extensions. The original exception remains unmodified.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` or `context` is `null`.
- `ArgumentException`: Thrown when any key in `context` is `null`.

---

### `GetExtension<T>`

Retrieves a specific extension value by key from a `GraphQLException`, with type conversion.

**Declaration**
```csharp
public static T GetExtension<T>(this GraphQLException exception, string key)
```

**Parameters**
- `exception`: The `GraphQLException` to query. Must not be `null`.
- `key`: The key of the extension to retrieve. Must not be `null`.

**Returns**
- The extension value cast to type `T`. Returns `default(T)` if the key is not found or the value cannot be converted.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` or `key` is `null`.

---

### `AddFormattedErrorCode`

Adds a formatted error code to a `GraphQLException` using a composite format string.

**Declaration**
```csharp
public static void AddFormattedErrorCode(this GraphQLException exception, string errorCodeFormat, params object[] args)
```

**Parameters**
- `exception`: The `GraphQLException` to modify. Must not be `null`.
- `errorCodeFormat`: A composite format string (e.g., `"USER_{0}_NOT_FOUND"`). Must not be `null`.
- `args`: Arguments to format into `errorCodeFormat`.

**Returns**
- `void`

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` or `errorCodeFormat` is `null`.
- `FormatException`: Thrown when `errorCodeFormat` is invalid or `args` does not match the format placeholders.

---

## Usage

### Example 1: Adding and Serializing Extensions

```csharp
try
{
    // Simulate a GraphQL operation that fails
    throw new GraphQLException("User not found")
    {
        Extensions = new Dictionary<string, object>
        {
            ["userId"] = 123,
            ["timestamp"] = DateTime.UtcNow
        }
    };
}
catch (GraphQLException ex)
{
    // Add additional extensions
    ex.AddExtensions(new[]
    {
        new KeyValuePair<string, object>("requestId", "abc123"),
        new KeyValuePair<string, object>("retryable", true)
    });

    // Serialize all extensions to JSON
    string serialized = ex.SerializeExtensions();
    Console.WriteLine(serialized);
}
```

### Example 2: Retrieving Typed Extensions and Formatted Error Codes

```csharp
try
{
    // Simulate an error with a formatted code
    var ex = new GraphQLException("Invalid input");
    ex.AddFormattedErrorCode("VALIDATION_ERROR_{0}", "EMAIL_FORMAT");

    // Retrieve the error code or default
    string code = ex.GetErrorCodeOrDefault("GENERIC_ERROR");
    Console.WriteLine($"Error Code: {code}");

    // Retrieve a typed extension
    bool isRetryable = ex.GetExtension<bool>("retryable");
    Console.WriteLine($"Retryable: {isRetryable}");
}
catch (GraphQLException ex)
{
    // Handle exception
}
```

---

## Notes

- **Null Handling**: All methods throw `ArgumentNullException` if required parameters (`exception`, `key`, `errorCodeFormat`) are `null`. This enforces explicit error handling for invalid inputs.
- **Thread Safety**: These methods are not thread-safe. Modifying the `Extensions` property of a `GraphQLException` directly may lead to race conditions if the same exception instance is accessed concurrently.
- **Immutability**: `WithContext` returns a new exception instance, preserving immutability. Direct modifications via `AddExtensions` or `AddFormattedErrorCode` alter the original exception's state.
- **Serialization Limitations**: `SerializeExtensions` relies on the underlying JSON serializer. Non-serializable types in extensions may cause runtime exceptions during serialization.
- **Type Conversion Risks**: `GetExtension<T>` uses runtime type conversion. Mismatches between the stored value type and `T` will result in `default(T)` rather than throwing, which may mask data inconsistencies.
