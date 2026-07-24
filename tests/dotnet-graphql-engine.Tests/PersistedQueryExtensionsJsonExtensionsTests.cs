using System;
using Xunit;
using GraphQLEngine.Configuration;

namespace GraphQLEngine.Tests;

public class PersistedQueryExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithDefaultOptions_ReturnsValidJson()
    {
        // Arrange
        var options = new PersistedQueryOptions();

        // Act
        var json = options.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("enforceHashVerification", json);
        Assert.Contains("maxIndexSize", json);
        Assert.Contains("allowlistOnly", json);
        Assert.Contains("returnNotFoundError", json);
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var options = new PersistedQueryOptions
        {
            EnforceHashVerification = false,
            MaxIndexSize = 5000,
            AllowlistOnly = true,
            ReturnNotFoundError = false
        };

        // Act
        var json = options.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("enforceHashVerification", json);
        Assert.Contains("5000", json);
        Assert.Contains("true", json);
        // Should contain newlines and indentation
        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void ToJson_WithModifiedOptions_ReturnsCorrectValues()
    {
        // Arrange
        var options = new PersistedQueryOptions
        {
            EnforceHashVerification = false,
            MaxIndexSize = 2500,
            AllowlistOnly = true,
            ReturnNotFoundError = false
        };

        // Act
        var json = options.ToJson();

        // Assert
        Assert.Contains("\"enforceHashVerification\":false", json);
        Assert.Contains("\"maxIndexSize\":2500", json);
        Assert.Contains("\"allowlistOnly\":true", json);
        Assert.Contains("\"returnNotFoundError\":false", json);
    }

    [Fact]
    public void ToJson_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        PersistedQueryOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options!.ToJson());
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedOptions()
    {
        // Arrange
        var json = "{\"enforceHashVerification\":false,\"maxIndexSize\":5000,\"allowlistOnly\":true,\"returnNotFoundError\":false}";

        // Act
        var options = PersistedQueryExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(options);
        Assert.False(options.EnforceHashVerification);
        Assert.Equal(5000, options.MaxIndexSize);
        Assert.True(options.AllowlistOnly);
        Assert.False(options.ReturnNotFoundError);
    }

    [Fact]
    public void FromJson_WithDefaultOptionsJson_ReturnsDefaultOptions()
    {
        // Arrange
        var json = "{\"enforceHashVerification\":true,\"maxIndexSize\":10000,\"allowlistOnly\":false,\"returnNotFoundError\":true}";

        // Act
        var options = PersistedQueryExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(options);
        Assert.True(options.EnforceHashVerification);
        Assert.Equal(10000, options.MaxIndexSize);
        Assert.False(options.AllowlistOnly);
        Assert.True(options.ReturnNotFoundError);
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PersistedQueryExtensionsJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PersistedQueryExtensionsJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithWhitespaceOnlyJson_ThrowsJsonException()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => PersistedQueryExtensionsJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "invalid json {{{";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => PersistedQueryExtensionsJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedOptions()
    {
        // Arrange
        var json = "{\"enforceHashVerification\":false,\"maxIndexSize\":3000,\"allowlistOnly\":true,\"returnNotFoundError\":false}";

        // Act
        var result = PersistedQueryExtensionsJsonExtensions.TryFromJson(json, out var options);

        // Assert
        Assert.True(result);
        Assert.NotNull(options);
        Assert.False(options!.EnforceHashVerification);
        Assert.Equal(3000, options.MaxIndexSize);
        Assert.True(options.AllowlistOnly);
        Assert.False(options.ReturnNotFoundError);
    }

    [Fact]
    public void TryFromJson_WithDefaultOptionsJson_ReturnsTrueAndDefaultOptions()
    {
        // Arrange
        var json = "{\"enforceHashVerification\":true,\"maxIndexSize\":10000,\"allowlistOnly\":false,\"returnNotFoundError\":true}";

        // Act
        var result = PersistedQueryExtensionsJsonExtensions.TryFromJson(json, out var options);

        // Assert
        Assert.True(result);
        Assert.NotNull(options);
        Assert.True(options!.EnforceHashVerification);
        Assert.Equal(10000, options.MaxIndexSize);
        Assert.False(options.AllowlistOnly);
        Assert.True(options.ReturnNotFoundError);
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PersistedQueryExtensionsJsonExtensions.TryFromJson(json!, out _));
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PersistedQueryExtensionsJsonExtensions.TryFromJson(json, out _));
    }

    [Fact]
    public void TryFromJson_WithWhitespaceOnlyJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act
        var result = PersistedQueryExtensionsJsonExtensions.TryFromJson(json, out var options);

        // Assert
        Assert.False(result);
        Assert.Null(options);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "invalid json {{{";

        // Act
        var result = PersistedQueryExtensionsJsonExtensions.TryFromJson(json, out var options);

        // Assert
        Assert.False(result);
        Assert.Null(options);
    }

    [Fact]
    public void TryFromJson_WithEmptyObjectJson_ReturnsTrueAndDefaultOptions()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = PersistedQueryExtensionsJsonExtensions.TryFromJson(json, out var options);

        // Assert
        Assert.True(result);
        Assert.NotNull(options);
        // Should use default values
        Assert.True(options!.EnforceHashVerification);
        Assert.Equal(10000, options.MaxIndexSize);
        Assert.False(options.AllowlistOnly);
        Assert.True(options.ReturnNotFoundError);
    }

    [Fact]
    public void Roundtrip_SerializationDeserialization_PreservesValues()
    {
        // Arrange
        var originalOptions = new PersistedQueryOptions
        {
            EnforceHashVerification = false,
            MaxIndexSize = 7500,
            AllowlistOnly = true,
            ReturnNotFoundError = false
        };

        // Act
        var json = originalOptions.ToJson();
        var deserializedOptions = PersistedQueryExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedOptions);
        Assert.Equal(originalOptions.EnforceHashVerification, deserializedOptions.EnforceHashVerification);
        Assert.Equal(originalOptions.MaxIndexSize, deserializedOptions.MaxIndexSize);
        Assert.Equal(originalOptions.AllowlistOnly, deserializedOptions.AllowlistOnly);
        Assert.Equal(originalOptions.ReturnNotFoundError, deserializedOptions.ReturnNotFoundError);
    }

    [Fact]
    public void Roundtrip_TryFromJsonDeserialization_PreservesValues()
    {
        // Arrange
        var originalOptions = new PersistedQueryOptions
        {
            EnforceHashVerification = true,
            MaxIndexSize = 15000,
            AllowlistOnly = false,
            ReturnNotFoundError = true
        };

        // Act
        var json = originalOptions.ToJson();
        var result = PersistedQueryExtensionsJsonExtensions.TryFromJson(json, out var deserializedOptions);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserializedOptions);
        Assert.Equal(originalOptions.EnforceHashVerification, deserializedOptions!.EnforceHashVerification);
        Assert.Equal(originalOptions.MaxIndexSize, deserializedOptions.MaxIndexSize);
        Assert.Equal(originalOptions.AllowlistOnly, deserializedOptions.AllowlistOnly);
        Assert.Equal(originalOptions.ReturnNotFoundError, deserializedOptions.ReturnNotFoundError);
    }
}