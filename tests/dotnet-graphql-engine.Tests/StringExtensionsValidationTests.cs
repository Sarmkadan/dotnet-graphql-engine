using System;
using Xunit;
using GraphQLEngine.Common.Utilities;

namespace GraphQLEngine.Tests;

public class StringExtensionsValidationTests
{
    [Fact]
    public void Validate_ValidString_ReturnsEmptyList()
    {
        // Arrange
        string input = "valid";

        // Act
        var result = input.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullString_ThrowsArgumentNullException()
    {
        // Arrange
        string input = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => input.Validate());
    }

    [Fact]
    public void Validate_EmptyString_ReturnsProblemList()
    {
        // Arrange
        string input = "";

        // Act
        var result = input.Validate();

        // Assert
        Assert.Single(result);
        Assert.Contains("cannot be null, empty, or whitespace", result[0]);
    }

    [Fact]
    public void Validate_WhitespaceString_ReturnsProblemList()
    {
        // Arrange
        string input = "   ";

        // Act
        var result = input.Validate();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void IsValid_ValidString_ReturnsTrue()
    {
        // Arrange
        string input = "valid";

        // Act
        var result = input.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidString_ReturnsFalse()
    {
        // Arrange
        string input = "";

        // Act
        var result = input.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ValidString_DoesNotThrow()
    {
        // Arrange
        string input = "valid";

        // Act & Assert
        input.EnsureValid();
    }

    [Fact]
    public void EnsureValid_InvalidString_ThrowsArgumentException()
    {
        // Arrange
        string input = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => input.EnsureValid());
    }
}
