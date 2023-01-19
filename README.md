// ... rest of the original README content ...

## StringExtensions

The `StringExtensions` class provides a set of utility methods for working with strings. It includes methods for converting strings to different case formats, truncating strings, checking if a string is a valid GraphQL name, normalizing whitespace, and escaping GraphQL strings.

### Usage Example

```csharp
using GraphQLEngine.Common.Utilities;

// Create a sample string
var input = "Hello World";

// Convert to camel case
var camelCase = input.ToCamelCase(); // "helloWorld"

// Convert to Pascal case
var pascalCase = input.ToPascalCase(); // "HelloWorld"

// Convert to snake case
var snakeCase = input.ToSnakeCase(); // "hello_world"

// Truncate the string to 10 characters
var truncated = input.Truncate(10); // "Hello Wo"

// Check if the string is a valid GraphQL name
var isValidName = input.IsValidGraphQLName(); // true

// Normalize whitespace
var normalized = input.NormalizeWhitespace(); // "Hello World"

// Escape a GraphQL string
var escaped = input.EscapeGraphQLString(); // "Hello World"
```

// ... rest of the original README content ...
