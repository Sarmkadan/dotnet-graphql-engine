using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class UserJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Converts a User object to a JSON string.
    /// </summary>
    /// <param name="value">The User object to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the User object.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the User object is null.</exception>
    public static string ToJson(this User value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? _jsonSerializerOptions with { WriteIndented = true } : _jsonSerializerOptions);
    }

    /// <summary>
    /// Converts a JSON string to a User object.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <returns>A User object representation of the JSON string, or null if the JSON string is null or empty.</returns>
    /// <exception cref="ArgumentException">Thrown if the JSON string is empty.</exception>
    /// <exception cref="JsonException">Thrown if the JSON string is invalid.</exception>
    public static User? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            return JsonSerializer.Deserialize<User>(_jsonSerializerOptions with { PropertyNameCaseInsensitive = true }, json);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Invalid JSON string", ex);
        }
    }

    /// <summary>
    /// Attempts to convert a JSON string to a User object.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <param name="value">The User object representation of the JSON string, or null if the conversion fails.</param>
    /// <returns>True if the conversion is successful, false otherwise.</returns>
    public static bool TryFromJson(string json, out User? value)
    {
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }
}
