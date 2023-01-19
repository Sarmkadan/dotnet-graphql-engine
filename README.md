// ... rest of the original README content ...

## ReflectionHelper

The `ReflectionHelper` class provides a set of utility methods for working with reflection in .NET. It offers methods for getting public properties and methods, checking if a type implements an interface, getting derived types, creating instances, getting property values, invoking methods, and more.

### Usage Example

```csharp
using GraphQLEngine.Common.Utilities;

// Get public properties of a type
var properties = ReflectionHelper.GetPublicProperties(typeof(User));
// Returns: [Name, Age, Roles]

// Get public methods of a type
var methods = ReflectionHelper.GetPublicMethods(typeof(User));
// Returns: [ToString, Equals, GetHashCode]

// Check if a type implements an interface
var isInterfaceImplemented = ReflectionHelper.ImplementsInterface<User, IComparable>();
// Returns: true

// Get derived types of a type
var derivedTypes = ReflectionHelper.GetDerivedTypes(typeof(User));
// Returns: [User, UserWithAddress]

// Create an instance of a type
var instance = ReflectionHelper.CreateInstance<User>();
// Returns: new User()

// Get property value of an instance
var propertyValue = ReflectionHelper.GetPropertyValue(instance, "Name");
// Returns: "John Doe"

// Set property value of an instance
ReflectionHelper.SetPropertyValue(instance, "Name", "Jane Doe");

// Invoke a method of an instance
var result = ReflectionHelper.InvokeMethod(instance, "ToString");
// Returns: "Jane Doe"

// Get custom attributes of a type
var attributes = ReflectionHelper.GetCustomAttributes<ObsoleteAttribute>(typeof(User));
// Returns: [Obsolete("This type is deprecated")]

// Check if a type is nullable
var isNullable = ReflectionHelper.IsNullableType(typeof(int?));
// Returns: true

// Get nullable underlying type
var nullableUnderlyingType = ReflectionHelper.GetNullableUnderlyingType(typeof(int?));
// Returns: int

// Get generic arguments of a type
var genericArguments = ReflectionHelper.GetGenericArguments(typeof(List<int>));
// Returns: [int]

// Check if a type is generic
var isGeneric = ReflectionHelper.IsGeneric(typeof(List<int>));
// Returns: true

// Map properties of an instance
ReflectionHelper.MapProperties(instance, typeof(UserWithAddress));

// Get readable type name of a type
var typeName = ReflectionHelper.GetReadableTypeName(typeof(User));
// Returns: "User"
```

// ... rest of the original README content ...
```