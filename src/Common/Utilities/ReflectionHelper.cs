#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Helper utilities for reflection and type inspection
/// Provides methods for querying type metadata and instantiation
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Gets all public properties of a type
    /// </summary>
    public static List<PropertyInfo> GetPublicProperties(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToList();
    }

    /// <summary>
    /// Gets all public methods of a type
    /// </summary>
    public static List<MethodInfo> GetPublicMethods(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => !m.IsSpecialName)
            .ToList();
    }

    /// <summary>
    /// Checks if a type implements a specific interface
    /// </summary>
    public static bool ImplementsInterface<TInterface>(Type type) where TInterface : class
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.GetInterfaces().Contains(typeof(TInterface));
    }

    /// <summary>
    /// Gets all types in an assembly that inherit from a base type
    /// </summary>
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
    public static object? GetPropertyValue(object obj, string propertyName)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase);
        return property?.GetValue(obj);
    }

    /// <summary>
    /// Sets the value of a property on an object
    /// </summary>
    public static void SetPropertyValue(object obj, string propertyName, object? value)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property?.CanWrite == true)
            property.SetValue(obj, value);
    }

    /// <summary>
    /// Invokes a method on an object
    /// </summary>
    public static object? InvokeMethod(object obj, string methodName, params object?[] parameters)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentNullException(nameof(methodName));

        var method = obj.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.IgnoreCase);
        if (method is null)
            throw new InvalidOperationException($"Method '{methodName}' not found on type {obj.GetType().FullName}");

        try
        {
            return method.Invoke(obj, parameters);
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
    public static bool IsNullableType(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return Nullable.GetUnderlyingType(type) is not null;
    }

    /// <summary>
    /// Gets the underlying type of a nullable type
    /// </summary>
    public static Type? GetNullableUnderlyingType(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return Nullable.GetUnderlyingType(type);
    }

    /// <summary>
    /// Gets all generic type arguments
    /// </summary>
    public static List<Type> GetGenericArguments(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.GetGenericArguments().ToList();
    }

    /// <summary>
    /// Checks if a type is generic
    /// </summary>
    public static bool IsGeneric(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.IsGenericType;
    }

    /// <summary>
    /// Maps property values from one object to another
    /// </summary>
    public static void MapProperties(object source, object destination)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        if (destination is null)
            throw new ArgumentNullException(nameof(destination));

        var sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destProperties = destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceProp in sourceProperties)
        {
            var destProp = destProperties.FirstOrDefault(p => p.Name == sourceProp.Name);
            if (destProp is not null && destProp.CanWrite)
            {
                try
                {
                    var value = sourceProp.GetValue(source);
                    if (value is not null || destProp.PropertyType.IsGenericType == false)
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
