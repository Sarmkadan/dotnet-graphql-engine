// SPDX-License-Identifier: MIT
// Tests for ReflectionHelperJsonExtensionsJsonExtensions
// These tests assume the class lives in the namespace dotnet_graphql_engine.Common.Utilities
// Adjust the namespace import if the actual namespace differs.

using System;
using Xunit;
using dotnet_graphql_engine.Common.Utilities;

namespace dotnet_graphql_engine.Tests;

public sealed class ReflectionHelperJsonExtensionsJsonExtensionsTests
{
    private sealed class TestModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    [Fact]
    public void ToJson_ShouldSerializeObject()
    {
        // Arrange
        var model = new TestModel { Id = 42, Name = "Answer" };

        // Act
        string json = ReflectionHelperJsonExtensionsJsonExtensions.ToJson(model);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"Id\":42", json, StringComparison.Ordinal);
        Assert.Contains("\"Name\":\"Answer\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void FromJson_ShouldDeserializeObject()
    {
        // Arrange
        var model = new TestModel { Id = 7, Name = "Lucky" };
        string json = ReflectionHelperJsonExtensionsJsonExtensions.ToJson(model);

        // Act
        object? result = ReflectionHelperJsonExtensionsJsonExtensions.FromJson(json, typeof(TestModel));

        // Assert
        Assert.NotNull(result);
        var deserialized = Assert.IsType<TestModel>(result);
        Assert.Equal(model.Id, deserialized.Id);
        Assert.Equal(model.Name, deserialized.Name);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var model = new TestModel { Id = 1, Name = "One" };
        string json = ReflectionHelperJsonExtensionsJsonExtensions.ToJson(model);

        // Act
        bool success = ReflectionHelperJsonExtensionsJsonExtensions.TryFromJson(json, typeof(TestModel), out object? result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        var deserialized = Assert.IsType<TestModel>(result);
        Assert.Equal(model.Id, deserialized.Id);
        Assert.Equal(model.Name, deserialized.Name);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        const string invalidJson = "{ this is not valid json }";

        // Act
        bool success = ReflectionHelperJsonExtensionsJsonExtensions.TryFromJson(invalidJson, typeof(TestModel), out object? result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TypeProperty_ShouldReturnNonEmptyString()
    {
        // The static class may expose a Type property describing its purpose.
        // We only verify that the property exists and returns a non‑empty string.
        string? type = ReflectionHelperJsonExtensionsJsonExtensions.Type;
        Assert.False(string.IsNullOrWhiteSpace(type));
    }
}
