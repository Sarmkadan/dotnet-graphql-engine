using System;
using System.Text.Json;
using GraphQLEngine.Domain.Entities;
using Xunit;

namespace dotnet_graphql_engine.Tests;

public class QueryFieldValidationJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithSimpleQueryField_ReturnsValidJson()
    {
        // Arrange
        var field = new QueryField("user");

        // Act
        var json = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.ToJson(field);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"name\":\"user\"", json);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var field = new QueryField("user");

        // Act
        var json = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.ToJson(field, indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("{\n", json);
        Assert.Contains("\"name\":", json);
    }

    [Fact]
    public void ToJson_WithAllProperties_ReturnsCompleteJson()
    {
        // Arrange
        var args = new[] { new QueryArgument("id", 1), new QueryArgument("name", "test") };
        var subFields = new[] { new QueryField("name"), new QueryField("email") };
        var field = new QueryField(
            name: "user",
            alias: "u",
            typeCondition: "User",
            arguments: args,
            fields: subFields
        );

        // Act
        var json = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.ToJson(field);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"name\":\"user\"", json);
        Assert.Contains("\"alias\":\"u\"", json);
        Assert.Contains("\"typeCondition\":\"User\"", json);
        Assert.Contains("\"arguments\"", json);
        Assert.Contains("\"fields\"", json);
    }

    [Fact]
    public void ToJson_WithNullArgumentValue_SerializesCorrectly()
    {
        // Arrange
        var args = new[] { new QueryArgument("optional", null) };
        var field = new QueryField("user", arguments: args);

        // Act
        var json = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.ToJson(field);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"arguments\"", json);
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsDeserializedQueryField()
    {
        // Arrange
        var json = "{\"name\":\"user\",\"alias\":\"u\"}";

        // Act
        var field = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(field);
        Assert.Equal("user", field.Name);
        Assert.Equal("u", field.Alias);
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string json = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => QueryFieldValidationJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueryFieldValidationJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithWhitespaceJson_ThrowsJsonException()
    {
        // Arrange
        var json = " \n\t ";

        // Act & Assert
        Assert.Throws<JsonException>(() => QueryFieldValidationJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "{invalid json}";

        // Act & Assert
        Assert.Throws<JsonException>(() => QueryFieldValidationJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithEmptyObject_ReturnsQueryFieldWithDefaults()
    {
        // Arrange
        var json = "{\"name\":\"test\"}";

        // Act
        var field = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(field);
        Assert.Equal("test", field.Name);
        Assert.Null(field.Alias);
        Assert.Null(field.TypeCondition);
        Assert.Empty(field.Arguments);
        Assert.Empty(field.Fields);
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedValue()
    {
        // Arrange
        var json = "{\"name\":\"user\",\"alias\":\"u\"}";

        // Act
        var result = QueryFieldValidationJsonExtensions.TryFromJson(json, out var field);

        // Assert
        Assert.True(result);
        Assert.NotNull(field);
        Assert.Equal("user", field.Name);
        Assert.Equal("u", field.Alias);
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string json = null!;
        QueryField? field = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => QueryFieldValidationJsonExtensions.TryFromJson(json, out field));
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";
        QueryField? field = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => QueryFieldValidationJsonExtensions.TryFromJson(json, out field));
    }

    [Fact]
    public void TryFromJson_WithWhitespaceJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = " \n\t ";
        QueryField? field = null;

        // Act
        var result = QueryFieldValidationJsonExtensions.TryFromJson(json, out field);

        // Assert
        Assert.False(result);
        Assert.Null(field);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNullValue()
    {
        // Arrange
        var json = "{invalid json}";
        QueryField? field = null;

        // Act
        var result = QueryFieldValidationJsonExtensions.TryFromJson(json, out field);

        // Assert
        Assert.False(result);
        Assert.Null(field);
    }

    [Fact]
    public void TryFromJson_WithEmptyObject_ReturnsTrueAndQueryFieldWithDefaults()
    {
        // Arrange
        var json = "{\"name\":\"test\"}";
        QueryField? field = null;

        // Act
        var result = QueryFieldValidationJsonExtensions.TryFromJson(json, out field);

        // Assert
        Assert.True(result);
        Assert.NotNull(field);
        Assert.Equal("test", field.Name);
        Assert.Null(field.Alias);
        Assert.Null(field.TypeCondition);
        Assert.Empty(field.Arguments);
        Assert.Empty(field.Fields);
    }

    [Fact]
    public void Roundtrip_WithSimpleQueryField_PreservesAllProperties()
    {
        // Arrange
        var originalField = new QueryField(
            name: "user",
            alias: "currentUser"
        );

        // Act
        var json = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.ToJson(originalField);
        var deserializedField = QueryFieldValidationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedField);
        Assert.Equal(originalField.Name, deserializedField.Name);
        Assert.Equal(originalField.Alias, deserializedField.Alias);
        Assert.Equal(originalField.TypeCondition, deserializedField.TypeCondition);
        Assert.Empty(deserializedField.Arguments);
        Assert.Empty(deserializedField.Fields);
    }

    [Fact]
    public void Roundtrip_WithQueryFieldWithArguments_PreservesAllProperties()
    {
        // Arrange
        var originalField = new QueryField(
            name: "user",
            alias: "currentUser",
            arguments: new[] { new QueryArgument("id", 123), new QueryArgument("active", true) }
        );

        // Act
        var json = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.ToJson(originalField);
        var deserializedField = QueryFieldValidationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedField);
        Assert.Equal(originalField.Name, deserializedField.Name);
        Assert.Equal(originalField.Alias, deserializedField.Alias);
        Assert.Equal(originalField.TypeCondition, deserializedField.TypeCondition);
        Assert.Equal(originalField.Arguments.Count, deserializedField.Arguments.Count);
        for (int i = 0; i < originalField.Arguments.Count; i++)
        {
            Assert.Equal(originalField.Arguments[i].Name, deserializedField.Arguments[i].Name);
            Assert.Equal(originalField.Arguments[i].Value, deserializedField.Arguments[i].Value);
        }
        Assert.Empty(deserializedField.Fields);
    }

    [Fact]
    public void Roundtrip_WithTryFromJson_PreservesAllProperties()
    {
        // Arrange
        var originalField = new QueryField(
            name: "query",
            alias: "q",
            arguments: new[] { new QueryArgument("limit", 10) },
            fields: new[] { new QueryField("results") }
        );

        // Act
        var json = GraphQLEngine.Domain.Entities.QueryFieldValidationJsonExtensions.ToJson(originalField);
        var result = QueryFieldValidationJsonExtensions.TryFromJson(json, out var deserializedField);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserializedField);
        Assert.Equal(originalField.Name, deserializedField.Name);
        Assert.Equal(originalField.Alias, deserializedField.Alias);
        Assert.Equal(originalField.Arguments.Count, deserializedField.Arguments.Count);
        Assert.Equal(originalField.Fields.Count, deserializedField.Fields.Count);
    }
}