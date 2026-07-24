using System;
using Xunit;
using GraphQLEngine.Common.Utilities;

namespace GraphQLEngine.Tests;

public class DateTimeExtensionsValidationTests
{
    [Fact]
    public void Validate_ValidDate_ReturnsEmptyList()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var result = date.Validate();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_DateTooEarly_ReturnsProblems()
    {
        // Arrange
        var date = new DateTime(1990, 1, 1);

        // Act
        var result = date.Validate();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("Unix timestamp is outside reasonable range", result);
    }

    [Fact]
    public void Validate_MinValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var date = DateTime.MinValue;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => date.Validate());
    }

    [Fact]
    public void Validate_MaxValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var date = DateTime.MaxValue;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => date.Validate());
    }

    [Fact]
    public void IsValid_ValidDate_ReturnsTrue()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var result = date.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidDate_ReturnsFalse()
    {
        // Arrange
        var date = new DateTime(1990, 1, 1);

        // Act
        var result = date.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EnsureValid_ValidDate_DoesNotThrow()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act & Assert
        date.EnsureValid();
    }

    [Fact]
    public void EnsureValid_InvalidDate_ThrowsArgumentException()
    {
        // Arrange
        var date = new DateTime(1990, 1, 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => date.EnsureValid());
    }
}
