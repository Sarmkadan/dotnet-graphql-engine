// SPDX-License-Identifier: MIT
// Tests for JsonHelperJsonExtensions

using System;
using Xunit;
using GraphQLEngine.Common.Utilities;

namespace GraphQLEngine.Tests;

public sealed class JsonHelperJsonExtensionsTests
{
    private sealed class TestModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed class EmptyModel
    {
        public int Value { get; set; }
    }

    [Fact]
    public void ToJson_WithObject_SerializesToJson()
    {
        // Arrange
        var model = new TestModel { Id = 42, Name = "Answer", CreatedAt = new DateTime(2024, 1, 1) };

        // Act
        string json = JsonHelperJsonExtensions.ToJson(model);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"id\":42", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"name\":\"Answer\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"createdAt\":", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_FormatsJsonWithIndentation()
    {
        // Arrange
        var model = new TestModel { Id = 7, Name = "Lucky" };

        // Act
        string json = JsonHelperJsonExtensions.ToJson(model, indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\n", json); // Should contain newlines for indentation
        Assert.Contains("{", json);
        Assert.Contains("}", json);
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ProducesCompactJson()
    {
        // Arrange
        var model = new TestModel { Id = 7, Name = "Lucky" };

        // Act
        string json = JsonHelperJsonExtensions.ToJson(model, indented: false);

        // Assert
        Assert.NotNull(json);
        Assert.DoesNotContain("\n", json); // Should not contain newlines
        Assert.Contains("{", json);
        Assert.Contains("}", json);
    }

    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        object? nullValue = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JsonHelperJsonExtensions.ToJson(nullValue));
    }

    [Fact]
    public void FromJson_ValidJson_DeserializesToObject()
    {
        // Arrange
        var model = new TestModel { Id = 13, Name = "Thirteen", CreatedAt = new DateTime(2024, 6, 1) };
        string json = JsonHelperJsonExtensions.ToJson(model);

        // Act
        TestModel? result = JsonHelperJsonExtensions.FromJson<TestModel>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(13, result.Id);
        Assert.Equal("Thirteen", result.Name);
        Assert.Equal(new DateTime(2024, 6, 1), result.CreatedAt);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullJson = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JsonHelperJsonExtensions.FromJson<TestModel>(nullJson));
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string emptyJson = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => JsonHelperJsonExtensions.FromJson<TestModel>(emptyJson));
    }

    [Fact]
    public void FromJson_WhitespaceJson_ThrowsJsonException()
    {
        // Arrange
        string whitespaceJson = "   ";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => JsonHelperJsonExtensions.FromJson<TestModel>(whitespaceJson));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        string invalidJson = "{ this is not valid json }";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => JsonHelperJsonExtensions.FromJson<TestModel>(invalidJson));
    }

    [Fact]
    public void FromJson_EmptyObject_DeserializesToDefaultValues()
    {
        // Arrange
        string json = "{}";

        // Act
        EmptyModel? result = JsonHelperJsonExtensions.FromJson<EmptyModel>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndDeserializesObject()
    {
        // Arrange
        var model = new TestModel { Id = 99, Name = "NinetyNine" };
        string json = JsonHelperJsonExtensions.ToJson(model);

        // Act
        bool success = JsonHelperJsonExtensions.TryFromJson(json, out TestModel? result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(99, result.Id);
        Assert.Equal("NinetyNine", result.Name);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullJson = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JsonHelperJsonExtensions.TryFromJson<TestModel>(nullJson, out _));
    }

    [Fact]
    public void TryFromJson_EmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string emptyJson = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => JsonHelperJsonExtensions.TryFromJson<TestModel>(emptyJson, out _));
    }

    [Fact]
    public void TryFromJson_WhitespaceJson_ReturnsFalse()
    {
        // Arrange
        string whitespaceJson = "   ";

        // Act
        bool success = JsonHelperJsonExtensions.TryFromJson<TestModel>(whitespaceJson, out TestModel? result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        string invalidJson = "{ this is not valid json }";

        // Act
        bool success = JsonHelperJsonExtensions.TryFromJson<TestModel>(invalidJson, out TestModel? result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_EmptyObject_ReturnsTrueAndDefaultValues()
    {
        // Arrange
        string json = "{}";

        // Act
        bool success = JsonHelperJsonExtensions.TryFromJson<EmptyModel>(json, out EmptyModel? result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(0, result.Value);
    }

}