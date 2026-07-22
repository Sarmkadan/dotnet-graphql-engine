using System;
using System.Linq;
using GraphQLEngine.Domain.Entities;
using Xunit;

namespace dotnet_graphql_engine.Tests;

public class QueryFieldTests
{
    [Fact]
    public void Constructor_WithOnlyName_SetsPropertiesAndDefaults()
    {
        // Arrange & Act
        var field = new QueryField("user");

        // Assert
        Assert.Equal("user", field.Name);
        Assert.Null(field.Alias);
        Assert.Null(field.TypeCondition);
        Assert.NotNull(field.Arguments);
        Assert.Empty(field.Arguments);
        Assert.NotNull(field.Fields);
        Assert.Empty(field.Fields);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var args = new[] { new QueryArgument("id", 1) };
        var subFields = new[] { new QueryField("name") };

        // Act
        var field = new QueryField(
            name: "user",
            alias: "u",
            typeCondition: "User",
            arguments: args,
            fields: subFields
        );

        // Assert
        Assert.Equal("user", field.Name);
        Assert.Equal("u", field.Alias);
        Assert.Equal("User", field.TypeCondition);
        
        Assert.Single(field.Arguments);
        Assert.Equal("id", field.Arguments[0].Name);
        Assert.Equal(1, field.Arguments[0].Value);
        
        Assert.Single(field.Fields);
        Assert.Equal("name", field.Fields[0].Name);
    }

    [Fact]
    public void Constructor_NullArguments_InitializesEmptyReadOnlyList()
    {
        // Arrange & Act
        var field = new QueryField("test", arguments: null);

        // Assert
        Assert.NotNull(field.Arguments);
        Assert.Empty(field.Arguments);
        Assert.IsAssignableFrom<IReadOnlyList<QueryArgument>>(field.Arguments);
    }

    [Fact]
    public void Constructor_NullFields_InitializesEmptyReadOnlyList()
    {
        // Arrange & Act
        var field = new QueryField("test", fields: null);

        // Assert
        Assert.NotNull(field.Fields);
        Assert.Empty(field.Fields);
        Assert.IsAssignableFrom<IReadOnlyList<QueryField>>(field.Fields);
    }

    [Fact]
    public void Constructor_WithNestedFields_PreservesHierarchy()
    {
        // Arrange
        var addressField = new QueryField("city");
        var userField = new QueryField("address", fields: new[] { addressField });

        // Act
        var rootField = new QueryField("user", fields: new[] { userField });

        // Assert
        Assert.Equal("user", rootField.Name);
        Assert.Single(rootField.Fields);
        
        var nestedField = rootField.Fields[0];
        Assert.Equal("address", nestedField.Name);
        Assert.Single(nestedField.Fields);
        
        var deeplyNestedField = nestedField.Fields[0];
        Assert.Equal("city", deeplyNestedField.Name);
    }

    [Fact]
    public void QueryArgument_Constructor_SetsNameAndValue()
    {
        // Arrange & Act
        var arg = new QueryArgument("limit", 10);

        // Assert
        Assert.Equal("limit", arg.Name);
        Assert.Equal(10, arg.Value);
    }

    [Fact]
    public void QueryArgument_Constructor_WithNullValue_SetsValueToNull()
    {
        // Arrange & Act
        var arg = new QueryArgument("optional", null);

        // Assert
        Assert.Equal("optional", arg.Name);
        Assert.Null(arg.Value);
    }
}
