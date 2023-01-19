// ... rest of the original README content ...

## TypeConverter

The `TypeConverter` class provides a set of utility methods for converting between different types. It offers methods for converting a value to a specific type, checking if a type can be converted, getting the default value for a type, converting a list of values, and converting a value to a JSON-compatible format.

### Usage Example

```csharp
using GraphQLEngine.Common.Utilities;

// Convert a string to an integer
var result = TypeConverter.Convert<int>("123");
// Returns: 123

// Try to convert a string to a boolean
if (TypeConverter.TryConvert<bool>("true"))
{
    Console.WriteLine("Converted successfully");
}
// Returns: true

// Check if a type can be converted
var canConvert = TypeConverter.CanConvert(typeof(int));
// Returns: true

// Get the default value for a type
var defaultValue = TypeConverter.GetDefaultValue(typeof(int));
// Returns: 0

// Convert a list of strings to a list of integers
var list = TypeConverter.ConvertList<int>("1", "2", "3");
// Returns: [1, 2, 3]

// Convert a value to a JSON-compatible format
var jsonCompatible = TypeConverter.ToJsonCompatible("Hello, World!");
// Returns: "Hello, World!"
```

## ValidationHelper

The `ValidationHelper` class provides a set of static methods for validating common input values such as query strings, type names, field names, emails, URLs, IDs, complexity scores, and depth levels. These helpers return a boolean indicating whether the supplied value meets the expected format or constraints.

### Usage Example

```csharp
using GraphQLEngine.Common.Utilities;

bool emailOk = ValidationHelper.ValidateEmail("user@example.com");
bool urlOk = ValidationHelper.ValidateUrl("https://example.com");
bool idOk = ValidationHelper.ValidateId("12345");
Console.WriteLine($"Email valid: {emailOk}, URL valid: {urlOk}, ID valid: {idOk}");
```

// ... rest of the original README content ...
