using System;
using System.Text.Json;
using GraphQLEngine.Exceptions;
using Xunit;

namespace GraphQLEngine.Tests;

public class GraphQLExceptionExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidException_ReturnsJsonString()
    {
        // Arrange
        var exception = new GraphQLException("Test error");

        // Act
        var json = GraphQLExceptionExtensionsJsonExtensions.ToJson(exception);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"message\":\"Test error\"", json);
    }

    [Fact]
    public void ToJson_WithNullException_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GraphQLExceptionExtensionsJsonExtensions.ToJson(null!));
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsException()
    {
        // Arrange
        var json = "{\"message\":\"Deserialized error\"}";

        // Act
        var exception = GraphQLExceptionExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Deserialized error", exception!.Message);
    }

    [Fact]
    public void FromJson_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var json = "this is not json";

        // Act
        var exception = GraphQLExceptionExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndException()
    {
        // Arrange
        var json = "{\"message\":\"TryFromJson success\"}";

        // Act
        var result = GraphQLExceptionExtensionsJsonExtensions.TryFromJson(json, out var exception);

        // Assert
        Assert.True(result);
        Assert.NotNull(exception);
        Assert.Equal("TryFromJson success", exception!.Message);
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GraphQLExceptionExtensionsJsonExtensions.TryFromJson(null!, out _));
    }
}
