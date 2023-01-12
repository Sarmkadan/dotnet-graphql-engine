# TypeConverter

A utility class for converting values between different types, handling common conversion scenarios such as primitive types, enums, and collections. This class simplifies type coercion in scenarios where runtime type flexibility is required, such as parsing user input, deserializing data, or preparing values for JSON serialization.

## API

### `public static T? Convert<T>(object? value)`
Converts the given value to the specified type `T`. If the conversion fails, it returns `default(T)` (typically `null` for reference types).

**Parameters:**
- `value` (`object?`): The value to convert. May be `null`.

**Returns:**
- `T?`: The converted value, or `default(T)` if conversion fails.

**Throws:**
- `InvalidCastException`: If the conversion is not supported for the given types.

---

### `public static object? Convert(object? value, Type targetType)`
Converts the given value to the specified `targetType`. If the conversion fails, it returns `null`.

**Parameters:**
- `value` (`object?`): The value to convert. May be `null`.
- `targetType` (`Type`): The target type to convert to.

**Returns:**
- `object?`: The converted value, or `null` if conversion fails.

**Throws:**
- `InvalidCastException`: If the conversion is not supported for the given types.

---

### `public static bool TryConvert<T>(object? value, out T? result)`
Attempts to convert the given value to the specified type `T`. Returns `true` if successful, otherwise `false`.

**Parameters:**
- `value` (`object?`): The value to convert. May be `null`.
- `result` (`out T?`): The converted value if successful, otherwise `default(T)`.

**Returns:**
- `bool`: `true` if the conversion succeeded, otherwise `false`.

---

### `public static bool CanConvert(Type sourceType, Type targetType)`
Determines whether a conversion from `sourceType` to `targetType` is supported.

**Parameters:**
- `sourceType` (`Type`): The type of the source value.
- `targetType` (`Type`): The target type to convert to.

**Returns:**
- `bool`: `true` if the conversion is supported, otherwise `false`.

---

### `public static object? GetDefaultValue(Type type)`
Returns the default value for the specified type. For reference types, this is `null`; for value types, it is the default-constructed value (e.g., `0` for `int`, `false` for `bool`).

**Parameters:**
- `type` (`Type`): The type for which to retrieve the default value.

**Returns:**
- `object?`: The default value of the specified type.

---

### `public static List<T?> ConvertList<T>(IEnumerable? source)`
Converts a collection of values to a `List<T>`. Each element in the source collection is converted to type `T` using `Convert<T>`. If an element cannot be converted, it is included as `default(T)` in the resulting list.

**Parameters:**
- `source` (`IEnumerable?`): The source collection to convert. May be `null`.

**Returns:**
- `List<T?>`: A list of converted values. Never `null`; returns an empty list if `source` is `null`.

---

### `public static object? ToJsonCompatible(object? value)`
Converts the given value to a JSON-compatible type (e.g., primitives, strings, lists, dictionaries). This is useful for preparing values for serialization.

**Parameters:**
- `value` (`object?`): The value to convert. May be `null`.

**Returns:**
- `object?`: A JSON-compatible representation of the value, or `null` if the input is `null`.

**Throws:**
- `InvalidCastException`: If the value cannot be converted to a JSON-compatible type.

## Usage

### Example 1: Converting User Input to Strongly-Typed Values
