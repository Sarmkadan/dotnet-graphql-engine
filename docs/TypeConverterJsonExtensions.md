# TypeConverterJsonExtensions

Provides extension methods for converting between objects and their JSON string representations using a configured `TypeConverter`. This utility bridges the gap between structured .NET types and serialized JSON, enabling straightforward serialization and deserialization with optional fallback mechanisms for error-tolerant parsing.

## API

### ToJson

```csharp
public static string ToJson(this TypeConverter converter, object? value)
```

Serializes an object to its JSON string representation using the specified `TypeConverter`.

**Parameters:**
- `converter` — The `TypeConverter` instance responsible for performing the conversion.
- `value` — The object to serialize. May be `null`.

**Return Value:**
A JSON string representing the supplied object. Returns `"null"` if the input value is `null`.

**Exceptions:**
- `NotSupportedException` — Thrown when the `TypeConverter` cannot perform the conversion to string for the given type.

---

### FromJson\<T\>

```csharp
public static T? FromJson<T>(this TypeConverter converter, string json)
```

Deserializes a JSON string to an instance of the specified type `T`.

**Parameters:**
- `converter` — The `TypeConverter` instance responsible for performing the conversion.
- `json` — A valid JSON string to deserialize.

**Return Value:**
An instance of type `T` populated from the JSON data, or `null` if `T` is a reference type and the JSON represents `null`.

**Exceptions:**
- `NotSupportedException` — Thrown when the `TypeConverter` cannot convert from string for type `T`.
- `FormatException` or other exceptions — Thrown when the JSON string is malformed or cannot be parsed into type `T`.

---

### TryFromJson\<T\>

```csharp
public static bool TryFromJson<T>(this TypeConverter converter, string json, out T? result)
```

Attempts to deserialize a JSON string to type `T` without throwing exceptions on failure.

**Parameters:**
- `converter` — The `TypeConverter` instance responsible for performing the conversion.
- `json` — A JSON string to deserialize.
- `result` — When this method returns `true`, contains the deserialized instance; when `false`, contains the default value for `T`.

**Return Value:**
`true` if deserialization succeeded; `false` if the JSON was invalid, the conversion is not supported, or any other error occurred during parsing.

**Exceptions:**
None. All exceptions are caught internally.

---

### FromJson

```csharp
public static object? FromJson(this TypeConverter converter, string json, Type type)
```

Deserializes a JSON string to an instance of the specified runtime `Type`.

**Parameters:**
- `converter` — The `TypeConverter` instance responsible for performing the conversion.
- `json` — A valid JSON string to deserialize.
- `type` — The target `Type` to which the JSON should be converted.

**Return Value:**
An object of the specified type, or `null` if the JSON represents `null`.

**Exceptions:**
- `ArgumentNullException` — Thrown when `type` is `null`.
- `NotSupportedException` — Thrown when the `TypeConverter` cannot convert from string for the given type.
- `FormatException` or other exceptions — Thrown when the JSON string is malformed or incompatible with the target type.

---

### TryFromJson

```csharp
public static bool TryFromJson(this TypeConverter converter, string json, Type type, out object? result)
```

Attempts to deserialize a JSON string to the specified runtime `Type` without throwing exceptions on failure.

**Parameters:**
- `converter` — The `TypeConverter` instance responsible for performing the conversion.
- `json` — A JSON string to deserialize.
- `type` — The target `Type` to which the JSON should be converted.
- `result` — When this method returns `true`, contains the deserialized object; when `false`, contains `null`.

**Return Value:**
`true` if deserialization succeeded; `false` if the JSON was invalid, the conversion is not supported, `type` is `null`, or any other error occurred.

**Exceptions:**
None. All exceptions are caught internally.

## Usage

### Example 1: Serializing and Deserializing a Simple Model

```csharp
var converter = TypeDescriptor.GetConverter(typeof(MyModel));

// Serialize an object to JSON
var model = new MyModel { Id = 42, Name = "Example" };
string json = converter.ToJson(model);
// json: {"Id":42,"Name":"Example"}

// Deserialize back to a strongly-typed instance
MyModel? restored = converter.FromJson<MyModel>(json);
Console.WriteLine(restored?.Name); // Output: Example
```

### Example 2: Safe Deserialization with Fallback

```csharp
var converter = TypeDescriptor.GetConverter(typeof(MyModel));

string userInput = GetUntrustedJsonInput();

// Attempt deserialization without risking an exception
if (converter.TryFromJson(userInput, out MyModel? result))
{
    ProcessModel(result);
}
else
{
    // Log the failure and use a default instance
    Logger.Warn("Invalid JSON received, using default model.");
    ProcessModel(new MyModel());
}
```

## Notes

- **TypeConverter Dependency:** These methods rely entirely on the capabilities of the provided `TypeConverter`. If the converter does not support conversion from or to strings for the target type, `NotSupportedException` is thrown by the non-try variants.
- **Null Handling:** `ToJson` explicitly returns the string `"null"` for a `null` input value. The `FromJson` methods return `null` (or the default value for value types in the case of `TryFromJson<T>`) when the JSON literal `null` is parsed.
- **Thread Safety:** These extension methods are stateless and delegate all work to the supplied `TypeConverter` instance. Thread safety depends on the thread-safety characteristics of that specific converter. Most `TypeConverter` implementations in the .NET base class library are thread-safe for read operations, but custom converters should be verified individually.
- **Error Suppression in Try Methods:** The `TryFromJson` overloads catch all exceptions indiscriminately. This guarantees no exception propagation but may mask programming errors unrelated to JSON parsing (e.g., `OutOfMemoryException` in extreme edge cases). Use the non-try overloads when precise error diagnosis is required.
