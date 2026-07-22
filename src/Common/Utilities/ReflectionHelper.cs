#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Cached metadata for a type to avoid repeated reflection calls
/// </summary>
/// <param name="Type">The type being described</param>
/// <param name="PublicProperties">Cached list of public properties</param>
/// <param name="PublicMethods">Cached list of public methods</param>
/// <param name="PropertyAccessors">Cached compiled property accessors</param>
public record TypeMetadata(
    Type Type,
    IReadOnlyList<PropertyInfo> PublicProperties,
    IReadOnlyList<MethodInfo> PublicMethods,
    IReadOnlyDictionary<string, Func<object, object?>> PropertyAccessors
);

/// <summary>
/// Delegate for compiled method invocation
/// </summary>
/// <param name="instance">The instance to invoke the method on</param>
/// <param name="parameters">Method parameters</param>
/// <returns>Method return value</returns>
delegate object? MethodInvoker(object? instance, params object?[] parameters);

/// <summary>
/// Helper utilities for reflection and type inspection
/// Provides methods for querying type metadata and instantiation
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Cache for type metadata to avoid repeated reflection operations
    /// </summary>
    private static readonly ConcurrentDictionary<Type, TypeMetadata> _typeMetadataCache = new();

    /// <summary>
    /// Cache for compiled property getters to avoid repeated reflection calls
    /// </summary>
    private static readonly ConcurrentDictionary<PropertyInfo, Func<object, object?>> _propertyGetterCache = new();

    /// <summary>
    /// Cache for compiled property setters to avoid repeated reflection calls
    /// </summary>
    private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object?>> _propertySetterCache = new();

    /// <summary>
    /// Cache for method invokers to avoid repeated reflection calls
    /// </summary>
    private static readonly ConcurrentDictionary<(Type, string), MethodInvoker> _methodInvokerCache = new();

    /// <summary>
    /// Gets or creates cached metadata for a type
    /// </summary>
    /// <param name="type">The type to get metadata for</param>
    /// <returns>Cached type metadata</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    private static TypeMetadata GetOrCreateTypeMetadata(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return _typeMetadataCache.GetOrAdd(type, t =>
        {
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => !m.IsSpecialName)
                .ToList();

            var accessors = new Dictionary<string, Func<object, object?>>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in properties)
            {
                accessors[prop.Name] = CreatePropertyGetter(prop);
            }

            return new TypeMetadata(t, properties.AsReadOnly(), methods.AsReadOnly(), accessors.AsReadOnly());
        });
    }

    /// <summary>
    /// Creates a compiled getter function for a property
    /// </summary>
    /// <param name="property">The property to create a getter for</param>
    /// <returns>Compiled getter function</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/></exception>
    private static Func<object, object?> CreatePropertyGetter(PropertyInfo property)
    {
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        var getMethod = property.GetGetMethod();
        if (getMethod is null)
            return _ => null;

        var objParam = Expression.Parameter(typeof(object), "obj");
        var castObj = Expression.Convert(objParam, property.DeclaringType!);
        var callGet = Expression.Call(castObj, getMethod);
        var castResult = Expression.Convert(callGet, typeof(object));

        return Expression.Lambda<Func<object, object?>>(
            castResult,
            objParam
        ).Compile();
    }

    /// <summary>
    /// Creates a compiled setter function for a property
    /// </summary>
    /// <param name="property">The property to create a setter for</param>
    /// <returns>Compiled setter function</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/></exception>
    private static Action<object, object?> CreatePropertySetter(PropertyInfo property)
    {
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        var setMethod = property.GetSetMethod();
        if (setMethod is null)
            return (_, _) => { };

        var objParam = Expression.Parameter(typeof(object), "obj");
        var valueParam = Expression.Parameter(typeof(object), "value");
        var castObj = Expression.Convert(objParam, property.DeclaringType!);
        var castValue = Expression.Convert(valueParam, property.PropertyType);
        var callSet = Expression.Call(castObj, setMethod, castValue);

        return Expression.Lambda<Action<object, object?>>(
            callSet,
            objParam,
            valueParam
        ).Compile();
    }

    /// <summary>
    /// Creates a compiled method invoker
    /// </summary>
    /// <param name="method">The method to create an invoker for</param>
    /// <returns>Compiled invoker delegate</returns>
    /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/></exception>
    private static MethodInvoker CreateMethodInvoker(MethodInfo method)
    {
        if (method is null)
            throw new ArgumentNullException(nameof(method));

        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var parametersParam = Expression.Parameter(typeof(object[]), "parameters");

        // Convert instance parameter
        Expression instanceExpr = method.IsStatic
            ? Expression.Constant(null, method.DeclaringType!)
            : Expression.Convert(instanceParam, method.DeclaringType!);

        // Convert parameters
        var parameterExpressions = method.GetParameters().Select((param, index) =>
        {
            var paramValue = Expression.ArrayIndex(parametersParam, Expression.Constant(index));
            return Expression.Convert(paramValue, param.ParameterType);
        }).ToArray();

        // Create method call
        var methodCall = Expression.Call(instanceExpr, method, parameterExpressions);

        // Handle return type
        Expression resultExpr = method.ReturnType == typeof(void)
            ? Expression.Block(methodCall, Expression.Constant(null))
            : Expression.Convert(methodCall, typeof(object));

        return Expression.Lambda<MethodInvoker>(
            resultExpr,
            instanceParam,
            parametersParam
        ).Compile();
    }

    /// <summary>
    /// Gets all public properties of a type
    /// </summary>
    /// <param name="type">The type to get properties from. Cannot be <see langword="null"/>.</param>
    /// <returns>List of public properties</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static List<PropertyInfo> GetPublicProperties(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var metadata = GetOrCreateTypeMetadata(type);
        return metadata.PublicProperties.ToList();
    }

    /// <summary>
    /// Gets all public methods of a type
    /// </summary>
    /// <param name="type">The type to get methods from. Cannot be <see langword="null"/>.</param>
    /// <returns>List of public methods</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static List<MethodInfo> GetPublicMethods(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var metadata = GetOrCreateTypeMetadata(type);
        return metadata.PublicMethods.ToList();
    }

    /// <summary>
    /// Checks if a type implements a specific interface
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static bool ImplementsInterface<TInterface>(Type type) where TInterface : class
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.GetInterfaces().Contains(typeof(TInterface));
    }

    /// <summary>
    /// Gets all types in an assembly that inherit from a base type
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> or <paramref name="baseType"/> is <see langword="null"/></exception>
    public static List<Type> GetDerivedTypes(Assembly assembly, Type baseType)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        return assembly.GetTypes()
            .Where(t => t != baseType && baseType.IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();
    }

    /// <summary>
    /// Creates an instance of a type using its default constructor
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static object? CreateInstance(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        try
        {
            return Activator.CreateInstance(type);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create instance of {type.FullName}", ex);
        }
    }

    /// <summary>
    /// Creates an instance of a type using constructor parameters
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static object? CreateInstance(Type type, params object?[] parameters)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        try
        {
            return Activator.CreateInstance(type, parameters);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create instance of {type.FullName}", ex);
        }
    }

    /// <summary>
    /// Gets the value of a property from an object
    /// </summary>
    /// <param name="obj">The object to get the property value from. Cannot be <see langword="null"/>.</param>
    /// <param name="propertyName">The name of the property to get. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>The property value, or <see langword="null"/> if the property doesn't exist or can't be accessed</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> or <paramref name="propertyName"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="propertyName"/> is empty or whitespace</exception>
    public static object? GetPropertyValue(object obj, string propertyName)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        var type = obj.GetType();
        var metadata = GetOrCreateTypeMetadata(type);

        if (metadata.PropertyAccessors.TryGetValue(propertyName, out var accessor))
        {
            return accessor(obj);
        }

        return null;
    }

    /// <summary>
    /// Sets the value of a property on an object
    /// </summary>
    /// <param name="obj">The object to set the property value on. Cannot be <see langword="null"/>.</param>
    /// <param name="propertyName">The name of the property to set. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="value">The value to set</param>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> or <paramref name="propertyName"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="propertyName"/> is empty or whitespace</exception>
    public static void SetPropertyValue(object obj, string propertyName, object? value)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        var type = obj.GetType();
        var metadata = GetOrCreateTypeMetadata(type);

        if (metadata.PropertyAccessors.TryGetValue(propertyName, out var accessor))
        {
            // The accessor is a getter, but we need to set. This shouldn't happen in normal usage.
            // For set operations, we need to get the property info and use the setter cache.
            var propInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propInfo?.GetSetMethod() is not null)
            {
                var setter = _propertySetterCache.GetOrAdd(propInfo, CreatePropertySetter);
                setter(obj, value);
            }
        }
    }

    /// <summary>
    /// Invokes a method on an object
    /// </summary>
    /// <param name="obj">The object to invoke the method on. Cannot be <see langword="null"/>.</param>
    /// <param name="methodName">The name of the method to invoke. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="parameters">Method parameters</param>
    /// <returns>The method return value</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> or <paramref name="methodName"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="methodName"/> is empty or whitespace</exception>
    /// <exception cref="InvalidOperationException">Method not found or invocation fails</exception>
    public static object? InvokeMethod(object obj, string methodName, params object?[] parameters)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentNullException(nameof(methodName));

        var type = obj.GetType();
        var invoker = _methodInvokerCache.GetOrAdd(
            (type, methodName),
            key =>
            {
                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (method is null)
                    return (_, _) => throw new InvalidOperationException($"Method '{methodName}' not found on type {type.FullName}");

                return CreateMethodInvoker(method);
            }
        );

        try
        {
            return invoker(obj, parameters ?? Array.Empty<object?>());
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    /// <summary>
    /// Gets all custom attributes of a type
    /// </summary>
    public static List<T> GetCustomAttributes<T>(Type type) where T : Attribute
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.GetCustomAttributes(typeof(T), true)
            .Cast<T>()
            .ToList();
    }

    /// <summary>
    /// Checks if a type is a nullable type
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static bool IsNullableType(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return Nullable.GetUnderlyingType(type) is not null;
    }

    /// <summary>
    /// Gets the underlying type of a nullable type
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static Type? GetNullableUnderlyingType(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return Nullable.GetUnderlyingType(type);
    }

    /// <summary>
    /// Gets all generic type arguments
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static List<Type> GetGenericArguments(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.GetGenericArguments().ToList();
    }

    /// <summary>
    /// Checks if a type is generic
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static bool IsGeneric(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.IsGenericType;
    }

    /// <summary>
    /// Maps property values from one object to another
    /// </summary>
    /// <param name="source">The source object to copy properties from. Cannot be <see langword="null"/>.</param>
    /// <param name="destination">The destination object to copy properties to. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="destination"/> is <see langword="null"/></exception>
    public static void MapProperties(object source, object destination)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        if (destination is null)
            throw new ArgumentNullException(nameof(destination));

        var sourceMetadata = GetOrCreateTypeMetadata(source.GetType());
        var destMetadata = GetOrCreateTypeMetadata(destination.GetType());

        var destProperties = destMetadata.PublicProperties;

        foreach (var sourceProp in sourceMetadata.PublicProperties)
        {
            var destProp = destProperties.FirstOrDefault(p => p.Name == sourceProp.Name);
            if (destProp is not null && destProp.CanWrite)
            {
                try
                {
                    var value = sourceProp.GetValue(source);
                    if (value is not null || destProp.PropertyType.IsValueType)
                        destProp.SetValue(destination, value);
                }
                catch
                {
                    // Skip properties that can't be mapped
                }
            }
        }
    }

    /// <summary>
    /// Gets a readable type name for display purposes
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
    public static string GetReadableTypeName(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (!type.IsGenericType)
            return type.Name;

        var genericArgs = type.GetGenericArguments();
        var argNames = string.Join(", ", genericArgs.Select(GetReadableTypeName));
        return $"{type.Name.Split('`')[0]}<{argNames}>";
    }

}