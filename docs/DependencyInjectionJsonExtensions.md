# DependencyInjectionJsonExtensions

Provides JSON serialization and deserialization extension methods for `GraphQLEngineOptions` and `DotnetGraphqlEngineOptions`. These methods are intended for use in dependency injection configuration scenarios where engine options need to be persisted or transferred as JSON strings.

## API

### `ToJson` (for `GraphQLEngineOptions`)

```csharp
public static string ToJson(this GraphQLEngineOptions options)
```

Serializes a `GraphQLEngineOptions` instance to its JSON representation.

- **Parameters**  
  `options` – The options object to serialize. Must not be `null`.

- **Returns**  
  A JSON string representing the options.

- **Throws**  
  `ArgumentNullException` if `options` is `null`.

---

### `FromJson` (for `GraphQLEngineOptions`)

```csharp
public static GraphQLEngineOptions? FromJson(string json)
```

Deserializes a JSON string into a `GraphQLEngineOptions` instance.

- **Parameters**  
  `json` – The JSON string to deserialize. Must not be `null` or empty.

- **Returns**  
  A `GraphQLEngineOptions` instance if deserialization succeeds; otherwise `null`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  `ArgumentException` if `json` is empty or contains only whitespace.  
  `JsonException` if the JSON is malformed or cannot be mapped to the target type.

---

### `TryFromJson` (for `GraphQLEngineOptions`)

```csharp
public static bool TryFromJson(string json, out GraphQLEngineOptions? options)
```

Attempts to deserialize a JSON string into a `GraphQLEngineOptions` instance without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to deserialize. Must not be `null`.  
  `options` – When this method returns, contains the deserialized options if successful, or `null` if deserialization failed.

- **Returns**  
  `true` if deserialization succeeded; `false` otherwise.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.

---

### `ToJson` (for `DotnetGraphqlEngineOptions`)

```csharp
public static string ToJson(this DotnetGraphqlEngineOptions options)
```

Serializes a `DotnetGraphqlEngineOptions` instance to its JSON representation.

- **Parameters**  
  `options` – The options object to serialize. Must not be `null`.

- **Returns**  
  A JSON string representing the options.

- **Throws**  
  `ArgumentNullException` if `options` is `null`.

---

### `FromJsonDotnet` (for `DotnetGraphqlEngineOptions`)

```csharp
public static DotnetGraphqlEngineOptions? FromJsonDotnet(string json)
```

Deserializes a JSON string into a `DotnetGraphqlEngineOptions` instance.

- **Parameters**  
  `json` – The JSON string to deserialize. Must not be `null` or empty.

- **Returns**  
  A `DotnetGraphqlEngineOptions` instance if deserialization succeeds; otherwise `null`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  `ArgumentException` if `json` is empty or contains only whitespace.  
  `JsonException` if the JSON is malformed or cannot be mapped to the target type.

---

### `TryFromJson` (for `DotnetGraphqlEngineOptions`)

```csharp
public static bool TryFromJson(string json, out DotnetGraphqlEngineOptions? options)
```

Attempts to deserialize a JSON string into a `DotnetGraphqlEngineOptions` instance without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to deserialize. Must not be `null`.  
  `options` – When this method returns, contains the deserialized options if successful, or `null` if deserialization failed.

- **Returns**  
  `true` if deserialization succeeded; `false` otherwise.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.

---

## Usage

### Example 1: Serialize and deserialize `GraphQLEngineOptions`

```csharp
using DotnetGraphqlEngine;

var options = new GraphQLEngineOptions
{
    MaxQueryDepth = 10,
    EnableIntrospection = true
};

string json = options.ToJson();
Console.WriteLine(json);

GraphQLEngineOptions? restored = DependencyInjectionJsonExtensions.FromJson(json);
if (restored != null)
{
    Console.WriteLine($"MaxQueryDepth: {restored.MaxQueryDepth}");
}
```

### Example 2: Safe deserialization of `DotnetGraphqlEngineOptions`

```csharp
using DotnetGraphqlEngine;

string json = @"{ ""EnableMetrics"": true, ""CacheSize"": 256 }";

if (DependencyInjectionJsonExtensions.TryFromJson(json, out DotnetGraphqlEngineOptions? options))
{
    Console.WriteLine($"Metrics enabled: {options.EnableMetrics}");
}
else
{
    Console.WriteLine("Failed to deserialize options.");
}
```

## Notes

- All methods treat `null` input for the JSON string as an immediate exception (except `TryFromJson` which returns `false` without throwing). Empty or whitespace-only strings cause an `ArgumentException` in the `FromJson` and `FromJsonDotnet` methods.
- Serialization uses the default `System.Text.Json` serializer settings. Custom converters or case-insensitive property matching are not applied unless configured globally.
- The `TryFromJson` overloads are safe to call with untrusted input; they will not throw on malformed JSON or type mismatches.
- These extension methods are static and do not maintain any internal state. They are thread-safe as long as the input options objects are not mutated concurrently during serialization. Deserialization produces a new instance and does not modify the input string.
