using System;
using System.Collections.Generic;
using System.Linq;
using GraphQLEngine.Domain.Entities;
using Xunit;

namespace dotnet_graphql_engine.Tests;

/// <summary>
/// Tests for the <see cref="QueryFieldValidation"/> static helper class.
/// </summary>
public class QueryFieldValidationTests
{
    // -------------------------------------------------------------------------
    // Happy path tests
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ValidField_ReturnsEmptyList()
    {
        // Arrange
        var field = new QueryField("user");

        // Act
        var result = field.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_ValidField_ReturnsTrue()
    {
        // Arrange
        var field = new QueryField("user");

        // Act
        var isValid = field.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_ValidField_DoesNotThrow()
    {
        // Arrange
        var field = new QueryField("user");

        // Act / Assert
        var exception = Record.Exception(() => field.EnsureValid());
        Assert.Null(exception);
    }

    // -------------------------------------------------------------------------
    // Edge / error cases
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_NullField_ThrowsArgumentNullException()
    {
        // Arrange
        QueryField? field = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => field!.Validate());
    }

    [Fact]
    public void IsValid_NullField_ReturnsFalse()
    {
        // Arrange
        QueryField? field = null;

        // Act
        var isValid = field.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_NullField_ThrowsArgumentNullException()
    {
        // Arrange
        QueryField? field = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => field!.EnsureValid());
    }

    [Fact]
    public void Validate_FieldWithInvalidName_ReturnsError()
    {
        // Arrange
        var field = new QueryField("   "); // whitespace name

        // Act
        var errors = field.Validate();

        // Assert
        Assert.Contains("QueryField.Name cannot be null, empty, or whitespace.", errors);
    }

    [Fact]
    public void Validate_FieldWithInvalidAlias_ReturnsError()
    {
        // Arrange
        var field = new QueryField("user", alias: "  ");

        // Act
        var errors = field.Validate();

        // Assert
        Assert.Contains("QueryField.Alias cannot be whitespace.", errors);
    }

    [Fact]
    public void Validate_FieldWithInvalidArgumentName_ReturnsError()
    {
        // Arrange
        var arg = new QueryArgument("  ", 1); // whitespace name
        var field = new QueryField("user", arguments: new[] { arg });

        // Act
        var errors = field.Validate();

        // Assert
        Assert.Contains("QueryField.Arguments[].Name cannot be null, empty, or whitespace.", errors);
    }

    [Fact]
    public void Validate_NestedInvalidField_PropagatesNestedError()
    {
        // Arrange
        var inner = new QueryField("  "); // invalid inner name
        var outer = new QueryField("user", fields: new[] { inner });

        // Act
        var errors = outer.Validate();

        // Assert
        // The outer field itself is valid, but the nested validation error should be prefixed.
        Assert.Contains(errors, e => e.StartsWith("Nested field validation: QueryField.Name cannot be null, empty, or whitespace."));
    }

    [Fact]
    public void EnsureValid_InvalidField_ThrowsArgumentExceptionWithMessages()
    {
        // Arrange
        var field = new QueryField("   "); // invalid name

        // Act
        var ex = Assert.Throws<ArgumentException>(() => field.EnsureValid());

        // Assert
        Assert.Contains("QueryField.Name cannot be null, empty, or whitespace.", ex.Message);
    }

    // -------------------------------------------------------------------------
    // Additional sanity checks for null collections (constructor normalises them)
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_FieldWithNullArgumentsAndFields_ReturnsEmptyList()
    {
        // Arrange
        var field = new QueryField("user", arguments: null, fields: null);

        // Act
        var errors = field.Validate();

        // Assert
        // The constructor should have turned null collections into empty read‑only lists,
        // therefore validation should not report null‑collection errors.
        Assert.Empty(errors);
    }
}
