#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using FluentAssertions;
using GraphQLEngine.Exceptions;
using Xunit;

namespace GraphQLEngine.Tests.Exceptions;

sealed public class GraphQLExceptionExtensionsValidationTests
{
    [Fact]
    public void ValidateExtensionCompatibility_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        GraphQLException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.ValidateExtensionCompatibility());
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithValidException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new GraphQLException("Test error", "TEST_ERROR");
        exception.AddExtension("errorCode", "TEST_ERROR");

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithNullErrorCode_ReturnsError()
    {
        // Arrange
        var exception = new GraphQLException("Test error");
        exception.ErrorCode = null;
        exception.AddExtension("errorCode", "TEST_ERROR");

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("ErrorCode must not be null or whitespace");
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithWhitespaceErrorCode_ReturnsError()
    {
        // Arrange
        var exception = new GraphQLException("Test error");
        exception.ErrorCode = "   ";
        exception.AddExtension("errorCode", "TEST_ERROR");

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("ErrorCode must not be null or whitespace");
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithNullExtensions_ReturnsError()
    {
        // Arrange
        var exception = new GraphQLException("Test error", "TEST_ERROR");
        exception.Extensions = null!;

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Extensions dictionary must not be null");
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithEmptyExtensions_ReturnsError()
    {
        // Arrange
        var exception = new GraphQLException("Test error", "TEST_ERROR");
        exception.Extensions.Clear();

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Extensions should contain an 'errorCode' string extension");
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithMissingErrorCodeExtension_ReturnsError()
    {
        // Arrange
        var exception = new GraphQLException("Test error", "TEST_ERROR");
        exception.Extensions.Clear();
        exception.Extensions.Add("otherKey", "value");

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Extensions should contain an 'errorCode' string extension");
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithEmptyStringErrorCodeExtension_ReturnsError()
    {
        // Arrange
        var exception = new GraphQLException("Test error", "TEST_ERROR");
        exception.Extensions.Clear();
        exception.Extensions.Add("errorCode", string.Empty);

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("The 'errorCode' extension should not be null or whitespace");
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithWhitespaceErrorCodeExtension_ReturnsError()
    {
        // Arrange
        var exception = new GraphQLException("Test error", "TEST_ERROR");
        exception.Extensions.Clear();
        exception.Extensions.Add("errorCode", "   ");

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("The 'errorCode' extension should not be null or whitespace");
    }

    [Fact]
    public void IsValidForExtensions_WithNullException_ReturnsFalse()
    {
        // Arrange
        GraphQLException? exception = null;

        // Act
        var result = exception.IsValidForExtensions();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidForExtensions_WithValidException_ReturnsTrue()
    {
        // Arrange
        var exception = new GraphQLException("Test error", "TEST_ERROR");
        exception.AddExtension("errorCode", "TEST_ERROR");

        // Act
        var result = exception.IsValidForExtensions();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidForExtensions_WithInvalidException_ReturnsFalse()
    {
        // Arrange
        var exception = new GraphQLException("Test error");
        exception.ErrorCode = null;

        // Act
        var result = exception.IsValidForExtensions();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EnsureValidForExtensions_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        GraphQLException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.EnsureValidForExtensions());
    }

    [Fact]
    public void EnsureValidForExtensions_WithValidException_DoesNotThrow()
    {
        // Arrange
        var exception = new GraphQLException("Test error", "TEST_ERROR");
        exception.AddExtension("errorCode", "TEST_ERROR");

        // Act
        var act = () => exception.EnsureValidForExtensions();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValidForExtensions_WithInvalidException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new GraphQLException("Test error");
        exception.ErrorCode = null;

        // Act & Assert
        var result = Assert.Throws<ArgumentException>(() => exception.EnsureValidForExtensions());
        result.Message.Should().Contain("GraphQLException validation for extension methods failed");
        result.Message.Should().Contain("ErrorCode must not be null or whitespace");
    }

    [Fact]
    public void EnsureValidForExtensions_WithMultipleValidationErrors_ThrowsArgumentExceptionWithAllErrors()
    {
        // Arrange
        var exception = new GraphQLException("Test error");
        exception.ErrorCode = null;
        exception.Extensions = null!;

        // Act & Assert
        var result = Assert.Throws<ArgumentException>(() => exception.EnsureValidForExtensions());
        result.Message.Should().Contain("GraphQLException validation for extension methods failed");
        result.Message.Should().Contain("ErrorCode must not be null or whitespace");
        result.Message.Should().Contain("Extensions dictionary must not be null");
    }

    [Fact]
    public void ValidateExtensionCompatibility_WithSubclassException_ReturnsValid()
    {
        // Arrange
        var exception = new ExecutionException("Execution failed", "users.id", 42);
        exception.AddExtension("errorCode", "EXECUTION_ERROR");

        // Act
        var result = exception.ValidateExtensionCompatibility();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void IsValidForExtensions_WithSubclassException_ReturnsTrue()
    {
        // Arrange
        var exception = new ValidationException("Validation failed", ["error1", "error2"]);
        exception.AddExtension("errorCode", "VALIDATION_ERROR");

        // Act
        var result = exception.IsValidForExtensions();

        // Assert
        result.Should().BeTrue();
    }
}
