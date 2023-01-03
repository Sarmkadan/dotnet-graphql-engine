# GraphQLException

`GraphQLException` is the base exception type used throughout the DotNet GraphQL engine to represent errors that occur during schema validation, query execution, or related operations. It provides a common structure for error codes, extensible metadata, and specialized derived exception types that convey additional context such as field paths, line numbers, or validation details.

## API

### Constructors

| Constructor | Description |
|-------------|-------------|
| `GraphQLException()` | Initializes a new instance of the `GraphQLException` class with a default message. |
| `GraphQLException(string message)` | Initializes a new instance with the specified error message. The `message` parameter is passed to the base `Exception` constructor. |

### Properties

| Property | Description |
|----------|-------------|
| `string? ErrorCode` | Gets or sets a string that identifies the category of the error (e.g., `"SCHEMA_ERROR"`). May be `null` if no code is supplied. |
| `Dictionary<string, object> Extensions` | Gets a read‑only dictionary that holds custom key/value pairs attached to the exception. The dictionary is instantiated when the exception is created and can be populated via `AddExtension`. |

### Methods

| Method | Description |
|--------|-------------|
| `void AddExtension(string key, object value)` | Adds an entry to the `Extensions` dictionary. Throws `ArgumentNullException` if `key` is `null`. If `key` already exists, its value is replaced. |

### Nested Exception Types

#### `SchemaException`

Represents an error encountered while building or validating the GraphQL schema.

- **Constructors**
  - `SchemaException()` – creates an instance with a default message.
  - `SchemaException(string message)` – creates an instance with the supplied message and sets the inherited `ErrorCode` to `"SCHEMA_ERROR"`.
- **Inherited members**: `ErrorCode`, `Extensions`, `AddException` from `GraphQLException`.

#### `ExecutionException`

Represents an error that occurred during the execution of a GraphQL request.

- **Constructors**
  - `ExecutionException()` – default constructor.
  - `ExecutionException(string message)` – sets the message and inherits `ErrorCode` = `"EXECUTION_ERROR"`.
- **Properties**
  - `string? FieldPath` – Gets or sets the dot‑separated path to the field that triggered the error (e.g., `"user.posts.title"`). May be `null`.
  - `int? LineNumber` – Gets or sets the line number in the source query where the error occurred. May be `null`.

#### `QueryComplexityException`

Indicates that a query exceeded the configured complexity limit.

- **Constructor**
  - `QueryComplexityException()` – default constructor.
- **Properties**
  - `int ActualScore` – The calculated complexity score of the query.
  - `int MaxScore` – The maximum allowed complexity score.

#### `ConfigurationException`

Signals a misconfiguration in the GraphQL server setup.

- **Constructors**
  - `ConfigurationException()` – default constructor.
  - `ConfigurationException(string message)` – sets the message and inherits `ErrorCode` = `"CONFIGURATION_ERROR"`.
- **Inherited members**: as with `SchemaException`.

#### `ValidationException`

Contains a collection of validation errors that occurred during schema or document validation.

- **Constructor**
  - `ValidationException()` – default constructor.
- **Property**
  - `List<string> ValidationErrors` – Gets or sets a list of descriptive validation error messages.

#### `DataLoaderException`

Represents an error that originated inside a `DataLoader` implementation.

- **Constructor**
  - `DataLoaderException()` – default constructor.
- **Property**
  - `string LoaderName` – Gets or sets the name of the `DataLoader` that threw the exception.

## Usage

### Throwing a typed exception with custom extensions

```csharp
using DotNet.GraphQL.Engine; // namespace containing GraphQLException

public IExecutionResult ExecuteRequest(string query)
{
    try
    {
        // ... execution logic ...
        if (query.Contains("forbiddenField"))
        {
            var ex = new ExecutionException("Access to forbidden field is denied.")
            {
                FieldPath = "user.secretData",
                LineNumber = 12
            };
            ex.AddExtension("details", new { reason = "policy violation" });
            throw ex;
        }
        // normal processing ...
    }
    catch (GraphQLException gex) when (gex.ErrorCode == "EXECUTION_ERROR")
    {
        // Log or enrich the exception before returning to client
        return new ExecutionResult { Errors = new[] { gex } };
    }
}
```

### Handling validation errors and exposing them to the client

```csharp
public IExecutionResult ValidateSchema(ISchema schema)
{
    var validator = new SchemaValidator(schema);
    var errors = validator.Validate();

    if (errors.Any())
    {
        var vex = new ValidationException();
        vex.ValidationErrors = errors.Select(e => e.Message).ToList();
        // Optionally add a custom extension for monitoring
        vex.AddExtension("validationCount", errors.Count);
        throw vex;
    }

    return ExecutionResult.Success();
}
```

## Notes

- The `Extensions` dictionary is instantiated lazily by the base class; calling `AddExtension` on a newly created exception will never return `null`. Consumers should treat the dictionary as mutable only through `AddExtension`; direct replacement of the property is not supported.
- Setting `ErrorCode` to `null` clears any previously assigned code; derived types such as `SchemaException` automatically override this value in their constructors, so manual assignment after construction may hide the intended semantics.
- None of the members of `GraphQLException` or its nested exception types perform thread‑safety guarantees. Concurrent modifications to the same `Extensions` dictionary from multiple threads must be synchronized externally (e.g., using a lock or `ConcurrentDictionary` wrapper). Reading the properties after construction is safe.
- The parameterless constructors of the derived exception types do not set a message; the base `Exception` message will be an empty string. It is recommended to use the overload that accepts a `message` parameter when a meaningful description is available.
- The `FieldPath` and `LineNumber` properties of `ExecutionException` are intended to assist debugging; they are not validated for correctness by the exception itself. Supplying inconsistent values will not cause an exception to be thrown but may confuse downstream error handling.
