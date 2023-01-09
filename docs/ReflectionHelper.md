# ReflectionHelper

A utility class providing common reflection operations for inspecting types, creating instances, invoking members, and working with generics in .NET applications.

## API

### `public static List<PropertyInfo> GetPublicProperties(Type type)`

Returns all public instance properties of the specified type, including inherited ones.

- **Parameters**
  - `type`: The type to inspect.
- **Return value**
  - A list of `PropertyInfo` objects representing the public properties.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---

### `public static List<MethodInfo> GetPublicMethods(Type type)`

Returns all public instance methods of the specified type, including inherited ones.

- **Parameters**
  - `type`: The type to inspect.
- **Return value**
  - A list of `MethodInfo` objects representing the public methods.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---

### `public static bool ImplementsInterface<TInterface>(Type type)`

Determines whether the specified type implements the given interface.

- **Type parameters**
  - `TInterface`: The interface type to check for.
- **Parameters**
  - `type`: The type to inspect.
- **Return value**
  - `true` if `type` implements `TInterface`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---

### `public static List<Type> GetDerivedTypes(Type baseType, Assembly assembly)`

Returns all types in the specified assembly that derive from or are equal to the given base type.

- **Parameters**
  - `baseType`: The base type to match.
  - `assembly`: The assembly to search.
- **Return value**
  - A list of `Type` objects representing the derived types.
- **Exceptions**
  - Throws `ArgumentNullException` if `baseType` or `assembly` is `null`.

---

### `public static object? CreateInstance(Type type)`

Creates an instance of the specified type using its parameterless constructor.

- **Parameters**
  - `type`: The type to instantiate.
- **Return value**
  - The newly created instance, or `null` if the type is abstract or an interface.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.
  - Throws `MissingMethodException` if no parameterless constructor exists.

---

### `public static object? CreateInstance(Type type, params object?[] args)`

Creates an instance of the specified type using the provided constructor arguments.

- **Parameters**
  - `type`: The type to instantiate.
  - `args`: Arguments to pass to the constructor.
- **Return value**
  - The newly created instance.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.
  - Throws `TargetInvocationException` if the constructor throws.
  - Throws `MissingMethodException` if no matching constructor is found.

---

### `public static object? GetPropertyValue(object? instance, string propertyName)`

Retrieves the value of a public property from the given instance.

- **Parameters**
  - `instance`: The object instance; may be `null` for static properties.
  - `propertyName`: The name of the property.
- **Return value**
  - The property value, or `null` if the property does not exist or is static and `instance` is `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `propertyName` is `null`.
  - Throws `TargetInvocationException` if the property getter throws.

---

### `public static void SetPropertyValue(object? instance, string propertyName, object? value)`

Sets the value of a public property on the given instance.

- **Parameters**
  - `instance`: The object instance; may be `null` for static properties.
  - `propertyName`: The name of the property.
  - `value`: The value to assign.
- **Exceptions**
  - Throws `ArgumentNullException` if `propertyName` is `null`.
  - Throws `TargetInvocationException` if the property setter throws.

---

### `public static object? InvokeMethod(object? instance, string methodName, params object?[] args)`

Invokes a public method on the given instance.

- **Parameters**
  - `instance`: The object instance; may be `null` for static methods.
  - `methodName`: The name of the method.
  - `args`: Arguments to pass to the method.
- **Return value**
  - The return value of the method, or `null` if the method is `void`.
- **Exceptions**
  - Throws `ArgumentNullException` if `methodName` is `null`.
  - Throws `TargetInvocationException` if the method throws.
  - Throws `MissingMethodException` if no matching method is found.

---

### `public static List<T> GetCustomAttributes<T>(MemberInfo memberInfo)`

Retrieves all custom attributes of the specified type from a member.

- **Type parameters**
  - `T`: The attribute type to retrieve.
- **Parameters**
  - `memberInfo`: The member to inspect.
- **Return value**
  - A list of attributes of type `T`.
- **Exceptions**
  - Throws `ArgumentNullException` if `memberInfo` is `null`.

---

### `public static bool IsNullableType(Type type)`

Determines whether the specified type is a nullable value type (e.g., `int?`).

- **Parameters**
  - `type`: The type to check.
- **Return value**
  - `true` if the type is nullable; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---
### `public static Type? GetNullableUnderlyingType(Type type)`

Gets the underlying type of a nullable value type (e.g., `int` for `int?`).

- **Parameters**
  - `type`: The nullable type.
- **Return value**
  - The underlying type, or `null` if the type is not nullable.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---
### `public static List<Type> GetGenericArguments(Type type)`

Retrieves the generic type arguments of the specified type.

- **Parameters**
  - `type`: The type to inspect.
- **Return value**
  - A list of `Type` objects representing the generic arguments.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.
  - Throws `ArgumentException` if the type is not generic.

---
### `public static bool IsGeneric(Type type)`

Determines whether the specified type is generic.

- **Parameters**
  - `type`: The type to check.
- **Return value**
  - `true` if the type is generic; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

---
### `public static void MapProperties(object source, object target)`

Copies public readable properties from the `source` object to the `target` object.

- **Parameters**
  - `source`: The source object.
  - `target`: The target object.
- **Exceptions**
  - Throws `ArgumentNullException` if `source` or `target` is `null`.
  - Throws `TargetInvocationException` if a property getter or setter throws.

---
### `public static string GetReadableTypeName(Type type)`

Returns a human-readable string representation of the type, including generic arguments.

- **Parameters**
  - `type`: The type to describe.
- **Return value**
  - A string representation of the type.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

## Usage

### Example 1: Inspecting a Type and Invoking a Method
