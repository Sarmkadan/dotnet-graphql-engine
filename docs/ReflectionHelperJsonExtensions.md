# ReflectionHelperJsonExtensions

`ReflectionHelperJsonExtensions` provides a small set of extension‑style helpers that enable conversion between .NET `Type` objects and their JSON representations. The utilities are intended for scenarios where type information must be persisted, transmitted, or reconstructed from JSON payloads, such as schema generation or runtime type resolution in GraphQL services.

## API

### `public static string ToJson(this Type type)`

Converts the supplied `type` into a JSON string that contains enough information to later reconstruct the same `Type` instance.

* **Parameters**
  * `type` – The `System.Type` to serialize. Must not be `null`.
* **Returns** – A JSON string representing the type. The format includes the type’s full name and assembly information.
* **Exceptions**
  * `ArgumentNullException` – Thrown if `type` is `null`.

---

### `public static Type? FromJson(string json)`

Deserializes a JSON string produced by `ToJson` back into a `System.Type` instance.

* **Parameters**
  * `json` – The JSON string containing type information. Must not be `null` or empty.
* **Returns** – The reconstructed `Type`, or `null` if the type cannot be resolved (e.g., the assembly is not loaded).
* **Exceptions**
  * `ArgumentNullException` – Thrown if `json` is `null`.
  * `ArgumentException` – Thrown if `json` is an empty string or not a valid JSON representation of a type.

---

### `public static bool TryFromJson(string json, out Type? type)`

Attempts to deserialize a JSON string into a `System.Type` without throwing exceptions.

* **Parameters**
  * `json` – The JSON string to parse. Must not be `null`.
  * `type` – When the method returns `true`, contains the resolved `Type`; otherwise, `null`.
* **Returns** – `true` if the type was successfully resolved; `false` otherwise.
* **Exceptions**
  * `ArgumentNullException` – Thrown if `json` is `null`.

---

### `public string? TypeName { get; }`

Gets the simple name of the type (equivalent to `Type.Name`). Returns `null` if the underlying type information is unavailable.

* **Returns** – The type’s name or `null`.

---

### `public string? AssemblyQualifiedName { get; }`

Gets the assembly‑qualified name of the type (equivalent to `Type.AssemblyQualifiedName`). Returns `null` when the type cannot be resolved.

* **Returns** – The full assembly‑qualified name or `null`.

---

### `public bool IsGenericType { get; }`

Indicates whether the represented type is a generic type definition or a constructed generic type.

* **Returns** – `true` if the type is generic; otherwise, `false`.

---

### `public bool IsAbstract { get; }`

Indicates whether the represented type is abstract.

* **Returns** – `true` if the type is abstract; otherwise, `false`.

---

### `public bool IsValueType { get; }`

Indicates whether the represented type is a value type (struct, enum, etc.).

* **Returns** – `true` if the type is a value type; otherwise, `false`.

## Usage

### Example 1 – Serialising and deserialising a type for storage

```csharp
using System;
using DotNetGraphQLEngine; // assumed namespace

// Store the type information
Type original = typeof(Dictionary<string, int>);
string json = original.ToJson();

// Persist the JSON (e.g., write to a file, database, etc.)
File.WriteAllText("type.json", json);

// Later, read and reconstruct the type
string storedJson = File.ReadAllText("type.json");
Type? reconstructed = ReflectionHelperJsonExtensions.FromJson(storedJson);

Console.WriteLine(reconstructed?.FullName); // System.Collections.Generic.Dictionary`2
```

### Example 2 – Safe resolution with `TryFromJson` in a GraphQL resolver

```csharp
using System;
using DotNetGraphQLEngine;

public class TypeResolver
{
    public Type? Resolve(string typeJson)
    {
        if (ReflectionHelperJsonExtensions.TryFromJson(typeJson, out var resolved))
        {
            // Additional checks before using the type
            if (resolved.IsAbstract)
                throw new InvalidOperationException("Abstract types cannot be instantiated.");

            return resolved;
        }

        // Fallback logic when the type cannot be resolved
        return null;
    }
}
```

## Notes

* **Thread safety** – All static members (`ToJson`, `FromJson`, `TryFromJson`) are stateless and therefore safe to call concurrently from multiple threads. Instance properties (`TypeName`, `AssemblyQualifiedName`, `IsGenericType`, `IsAbstract`, `IsValueType`) are read‑only after construction, so they are also thread‑safe.

* **Assembly loading** – `FromJson` and `TryFromJson` rely on `Type.GetType(string, bool, bool)`. If the target assembly is not already loaded into the current `AppDomain`, the resolution will fail and return `null` (or `false` for `TryFromJson`). Callers may need to load the required assembly manually before invoking these methods.

* **Generic types** – The JSON representation includes generic arguments. When deserialising, the helper will attempt to construct the closed generic type. If any generic argument cannot be resolved, the whole operation fails.

* **Null handling** – Instance properties return `null` when the underlying `Type` reference is missing (e.g., after a failed deserialization). Consumers should guard against `null` before accessing members.

* **Version tolerance** – The JSON format is deliberately simple (type name + assembly). Changing the format in future versions may break compatibility with persisted data; consider versioning the stored JSON if long‑term stability is required.
