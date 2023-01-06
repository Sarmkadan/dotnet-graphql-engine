#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extension methods for ReflectionHelper
/// These methods serialize and deserialize type information that ReflectionHelper works with
/// </summary>
public static class ReflectionHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes type information to a JSON string
    /// </summary>
    /// <param name="type">The Type to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the type information</returns>
    public static string ToJson(this Type type, bool indented = false)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        var typeInfo = new TypeInfo
        {
            TypeName = type.FullName,
            AssemblyQualifiedName = type.AssemblyQualifiedName,
            IsGenericType = type.IsGenericType,
            IsAbstract = type.IsAbstract,
            IsValueType = type.IsValueType
        };

        return JsonSerializer.Serialize(typeInfo, options);
    }

    /// <summary>
    /// Deserializes type information from JSON to a Type instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A Type instance if deserialization succeeded; otherwise null</returns>
    public static Type? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            var typeInfo = JsonSerializer.Deserialize<TypeInfo>(json, _jsonSerializerOptions);
            return typeInfo?.AssemblyQualifiedName is null
                ? null
                : Type.GetType(typeInfo.AssemblyQualifiedName);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize type information from JSON to a Type instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="type">Outputs the deserialized Type if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out Type? type)
    {
        type = default;

        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            var typeInfo = JsonSerializer.Deserialize<TypeInfo>(json, _jsonSerializerOptions);
            if (typeInfo?.AssemblyQualifiedName is null)
                return false;

            type = Type.GetType(typeInfo.AssemblyQualifiedName);
            return type is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private sealed class TypeInfo
    {
        public string? TypeName { get; set; }
        public string? AssemblyQualifiedName { get; set; }
        public bool IsGenericType { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsValueType { get; set; }
    }
}