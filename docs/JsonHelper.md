# JsonHelper
Static utility class that simplifies common JSON serialization and deserialization tasks using `System.Text.Json`. It provides helpers for converting objects to JSON strings, parsing JSON into strongly‑typed objects or dictionaries, merging dictionaries, comparing JSON values, and performing lightweight validation or transformation operations.

## API
### Serialize(object value)
Serializes the supplied object to a JSON formatted string using the library’s default options.

- **Parameters**
  - `value`: The object to serialize. Must not be `null`.
- **Return value**: A JSON string representing `value`.
- **Exceptions**
  - `ArgumentNullException` if `value` is `null`.
  - `JsonException` if serialization fails (e.g., circular references not handled by the default options).

### Serialize(object value, JsonSerializerOptions options)
Serializes the supplied object to a JSON formatted string using the provided `JsonSerializerOptions`.

- **Parameters**
  - `value`: The object to serialize. Must not be `null`.
  - `options`: Serialization options that control behavior such as property naming, ignoring nulls, or custom converters. Must not be `null`.
- **Return value**: A JSON string representing `value` according to `options`.
- **Exceptions**
  - `ArgumentNullException` if `value` or `options` is `null`.
  - `JsonException` if serialization fails under the given options.

### Deserialize<T>(string json)
Deserializes a JSON string into an instance of type `T` using the library’s default options.

- **Parameters**
  - `json`: The JSON input JSON must not be `null`.
- **Return value**: An instance of `T` populated from JSON string` if the JSON represents a nullable reference types are enabled.
- **Exceptions**
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if `json` is not valid JSON or cannot be mapped to type `T`.

### Deserialize<T>(string json, JsonSerializerOptions options)
Deserializes a JSON string into an instance of type `T` using the supplied `JsonSerializerOptions`.

- **Parameters**
  - `json`: The JSON input. Must not be `null`.
  - `options`: Deserialization options. Must not be `null`.
- **Return value**: An instance of `T` deserialized from `json` according to `options`, or `null` when the JSON is `null` and `T` is a nullable reference type.
- **Exceptions**
  - `ArgumentNullException` if `json` or `options` is `null`.
  - `JsonException` if `json` is malformed or incompatible with `T` given the options.

### DeserializeToDictionary(string json)
Deserializes a JSON object into a `Dictionary<string, object?>` using default options.

- **Parameters**
  - `json`: JSON object text. Must not be `null`.
- **Return value**: A dictionary whose keys are the property names and values are the corresponding JSON values (primitives, nested dictionaries, lists, or `null`). Returns `null` if `json` is `null` and the return type permits it.
- **Exceptions**
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if `json` is not a valid JSON object.

### DeserializeToList<T>(string json)
Deserializes a JSON array into a `List<T>` using default options.

- **Parameters**
  - `json`: JSON array text. Must not be `null`.
- **Return value**: A list of elements of type `T`. Returns `null` if `json` is `null` and the return type permits it.
- **Exceptions**
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if `json` is not a valid JSON array or its elements cannot be converted to `T`.

### ToDict(object obj)
Converts an object into a `Dictionary<string, object?>` by serializing it to JSON and then deserializing that JSON into a dictionary.

- **Parameters**
  - `obj`: The object to convert. Must not be `null`.
- **Return value**: A dictionary representation of the object's public properties and fields. Returns `null` if `obj` is `null` and the return type permits it.
- **Exceptions**
  - `ArgumentNullException` if `obj` is `null`.
  - `JsonException` if serialization or deserialization fails.

### FromDict<T>(Dictionary<string, object?> dict)
Creates an instance of type `T` from a dictionary by serializing the dictionary to JSON and then deserializing that JSON into `T`.

- **Parameters**
  - `dict`: Source dictionary. Must not be `null`.
- **Return value**: An instance of `T` populated from the dictionary’s key/value pairs. Returns `null` if `dict` is `null` and `T` is a nullable reference type.
- **Exceptions**
  - `ArgumentNullException` if `dict` is `null`.
  - `JsonException` if the round‑trip serialization/deserialization fails.

### IsValidJson(string json)
Checks whether a string contains well‑formed JSON.

- **Parameters**
  - `json`: The string to test. May be `null`.
- **Return value**: `true` if `json` is valid JSON; otherwise `false`. Returns `false` for `null` input.
- **Exceptions**: None.

### GetValueByPath(object obj, string path)
Retrieves a value from an object graph using a JSON‑Path‑like expression.

- **Parameters**
  - `obj`: The root object to query. Must not be `null`.
  - `path`: Path expression (e.g., `"prop.sub[0].name"`). Must not be `null` or empty.
- **Return value**: The value located at `path`, or `null` if the path does not exist.
- **Exceptions**
  - `ArgumentNullException` if `obj` or `path` is `null`.
  - `ArgumentException` if `path` is empty or contains invalid syntax.

### Merge(Dictionary<string, object?> first, Dictionary<string, object?> second)
Merges two dictionaries, with entries from `second` overwriting those from `first` on key conflicts.

- **Parameters**
  - `first`: Base dictionary. Must not be `null`.
  - `second`: Overlay dictionary. Must not be `null`.
- **Return value**: A new dictionary containing the merged key/value pairs. The input dictionaries are not modified.
- **Exceptions**
  - `ArgumentNullException` if either `first` or `second` is `null`.

### PrintJson(object obj)
Writes a pretty‑printed representation of the object's JSON to the standard output.

- **Parameters**
  - `obj`: The object to print. Must not be `null`.
- **Return value**: `void`.
- **Exceptions**
  - `ArgumentNullException` if `obj` is `null`.
  - `JsonException` if serialization fails.

### AreEqual(object first, object second)
Compares two objects for JSON‑semantic equality (ignoring formatting differences).

- **Parameters**
  - `first`: First object to compare. May be `null`.
  - `second`: Second object to compare. May be `null`.
- **Return value**: `true` if both objects serialize to equivalent JSON; otherwise `false`.
- **Exceptions**: None.

### RemoveNulls(string json)
Returns a JSON string where all properties whose value is `null` have been removed.

- **Parameters**
  - `json`: Input JSON. Must not be `null`.
- **Return value**: A JSON string identical to the input except that any `"property": null` pairs are omitted. Returns `null` if the input is `null` and the return type permits it.
- **Exceptions**
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if `json` is not valid JSON.

### GetMinimalOptions()
Provides a pre‑configured `JsonSerializerOptions` instance suitable for lightweight, high‑performance scenarios.

- **Parameters**: None.
- **Return value**: A new `JsonSerializerOptions` with commonly useful defaults (e.g., `PropertyNameCaseInsensitive = true`, `IgnoreNullValues = true`).
- **Exceptions**: None.

## Usage
```csharp
using System.Collections.Generic;

// Serialize a POCO to JSON
var person = new { Name = "Ada", Age = 30 };
string json = JsonHelper.Serialize(person);
// json => {"Name":"Ada","Age":30}

// Deserialize JSON into a dictionary and merge with extra data
Dictionary<string, object?> extra = new() { { "Role", "Developer" } };
Dictionary<string, object?> dict = JsonHelper.DeserializeToDictionary(json)!;
Dictionary<string, object?> merged = JsonHelper.Merge(dict, extra);
// merged contains Name, Age, and Role
```

```csharp
using System;
using System.Text.Json;

// Validate JSON before attempting to deserialize
string payload = @"{ ""items"": [ { ""id"": 1 }, { ""id"": 2 } ] }";
if (JsonHelper.IsValidJson(payload))
{
    var options = JsonHelper.GetMinimalOptions();
    var list = JsonHelper.DeserializeToList<Item>(payload, options);
    // Process list...
}
else
{
    Console.WriteLine("Invalid JSON received.");
}

class Item
{
    public int Id { get; set; }
}
```

## Notes
- All methods are **thread‑safe**; they rely only on immutable inputs and the thread‑safe `System.Text.Json` APIs.
- Passing `null` for any non‑optional parameter results in an `ArgumentNullException`.
- JSON‑related failures (`JsonException`) are thrown when the input does not conform to expected structure or when the serializer encounters unsupported types (e.g., self‑referencing loops not handled by the provided options).
- The `Merge` method creates a new dictionary; the source dictionaries remain unchanged.
- `RemoveNulls` operates on the JSON text level; it does not alter the original object graph.
- `GetMinimalOptions` returns a fresh instance each call; callers may cache it if desired to avoid repeated allocations.
