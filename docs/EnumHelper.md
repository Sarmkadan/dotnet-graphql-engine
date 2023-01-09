# EnumHelper

A utility class providing common operations for working with .NET enums, including parsing, reflection-based attribute inspection, flag manipulation, and display value retrieval.

## API

### `public static List<T> GetEnumValues<T>()`

Returns a list of all values defined in the enum type `T`.

- **Returns**: A list containing all enum values in declaration order.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static List<string> GetEnumNames<T>()`

Returns a list of the names of all values defined in the enum type `T`.

- **Returns**: A list of enum value names in declaration order.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static T? Parse<T>(string value)`

Parses a string into the corresponding enum value of type `T`.

- **Parameters**:
  - `value`: The string representation of the enum value.
- **Returns**: The parsed enum value, or `null` if parsing fails.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static bool TryParse<T>(string value, out T? result)`

Attempts to parse a string into the corresponding enum value of type `T`.

- **Parameters**:
  - `value`: The string representation of the enum value.
  - `result`: Output parameter receiving the parsed enum value on success.
- **Returns**: `true` if parsing succeeds; otherwise, `false`.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static string? GetDisplayName<T>(T value)`

Retrieves the display name of an enum value using `DisplayAttribute` or `DisplayNameAttribute`.

- **Parameters**:
  - `value`: The enum value.
- **Returns**: The display name string if available; otherwise, `null`.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static string? GetDescription<T>(T value)`

Retrieves the description of an enum value using `DescriptionAttribute`.

- **Parameters**:
  - `value`: The enum value.
- **Returns**: The description string if available; otherwise, `null`.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static bool HasAttribute<T, TAttr>(T value)`

Determines whether the specified enum value has a custom attribute of type `TAttr`.

- **Parameters**:
  - `value`: The enum value.
- **Returns**: `true` if the attribute is present; otherwise, `false`.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static List<TAttr> GetAttributes<T, TAttr>(T value)`

Retrieves all custom attributes of type `TAttr` applied to the specified enum value.

- **Parameters**:
  - `value`: The enum value.
- **Returns**: A list of attributes; empty if none are found.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static Dictionary<string, string> GetEnumDisplayDictionary<T>()`

Generates a dictionary mapping enum value names to their display names.

- **Returns**: A dictionary with value names as keys and display names as values.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static T? GetNextValue<T>(T current)`

Returns the next enum value in declaration order, or the first value if `current` is the last.

- **Parameters**:
  - `current`: The current enum value.
- **Returns**: The next enum value, or `null` if `T` is not an enum type or has no values.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static T? GetPreviousValue<T>(T current)`

Returns the previous enum value in declaration order, or the last value if `current` is the first.

- **Parameters**:
  - `current`: The current enum value.
- **Returns**: The previous enum value, or `null` if `T` is not an enum type or has no values.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static bool IsFlagsEnum<T>()`

Determines whether the enum type `T` is a flags enum.

- **Returns**: `true` if `T` is a flags enum; otherwise, `false`.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static T CombineFlags<T>(IEnumerable<T> flags)`

Combines multiple flag values into a single value.

- **Parameters**:
  - `flags`: An enumerable of flag values to combine.
- **Returns**: A combined flag value.
- **Throws**:
  - `InvalidOperationException` if `T` is not a flags enum.
  - `ArgumentNullException` if `flags` is `null`.
  - `ArgumentException` if `flags` contains an invalid value.

### `public static bool HasFlag<T>(T value, T flag)`

Determines whether the specified flag is set in the enum value.

- **Parameters**:
  - `value`: The enum value to check.
  - `flag`: The flag to test for.
- **Returns**: `true` if the flag is set; otherwise, `false`.
- **Throws**:
  - `InvalidOperationException` if `T` is not a flags enum.
  - `ArgumentException` if either `value` or `flag` is not a valid value for `T`.

### `public static List<T> GetFlags<T>()`

Retrieves all individual flags set in a flags enum value.

- **Returns**: A list of set flags; empty if none are set.
- **Throws**:
  - `InvalidOperationException` if `T` is not a flags enum.
  - `ArgumentException` if `value` is not a valid value for `T`.

### `public static object GetUnderlyingValue<T>(T value)`

Returns the underlying numeric value of the enum.

- **Parameters**:
  - `value`: The enum value.
- **Returns**: The underlying numeric value as an `object`.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

### `public static bool IsValidEnumValue<T>(object? value)`

Determines whether the given value is a valid value for the enum type `T`.

- **Parameters**:
  - `value`: The value to validate.
- **Returns**: `true` if the value is valid; otherwise, `false`.
- **Throws**: `InvalidOperationException` if `T` is not an enum type.

## Usage
