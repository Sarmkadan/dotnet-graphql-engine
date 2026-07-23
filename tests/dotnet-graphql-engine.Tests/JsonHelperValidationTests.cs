using System;
using System.Collections.Generic;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class JsonHelperValidationTests
    {
        [Fact]
        public void Validate_ValidJson_ReturnsEmptyList()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";

            // Act
            var problems = JsonHelperValidation.Validate(json);

            // Assert
            Assert.Empty(problems);
        }

        [Fact]
        public void Validate_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => JsonHelperValidation.Validate(null!));
        }

        [Fact]
        public void Validate_EmptyString_ReturnsProblem()
        {
            // Arrange
            var json = "";

            // Act
            var problems = JsonHelperValidation.Validate(json);

            // Assert
            Assert.Single(problems);
            Assert.Contains("JSON string cannot be null, empty, or whitespace.", problems[0]);
        }

        [Fact]
        public void Validate_Whitespace_ReturnsProblem()
        {
            // Arrange
            var json = "   ";

            // Act
            var problems = JsonHelperValidation.Validate(json);

            // Assert
            Assert.Single(problems);
            Assert.Contains("JSON string cannot be null, empty, or whitespace.", problems[0]);
        }

        [Fact]
        public void Validate_InvalidJson_ReturnsProblem()
        {
            // Arrange
            var json = "not a json";

            // Act
            var problems = JsonHelperValidation.Validate(json);

            // Assert
            Assert.Single(problems);
            Assert.Contains("The provided string is not a valid JSON.", problems[0]);
        }

        [Fact]
        public void IsValid_ValidJson_ReturnsTrue()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";

            // Act
            var result = JsonHelperValidation.IsValid(json);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => JsonHelperValidation.IsValid(null!));
        }

        [Fact]
        public void IsValid_EmptyString_ReturnsFalse()
        {
            // Arrange
            var json = "";

            // Act
            var result = JsonHelperValidation.IsValid(json);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var json = "invalid json";

            // Act
            var result = JsonHelperValidation.IsValid(json);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_ValidJson_DoesNotThrow()
        {
            // Arrange
            var json = "{\"key\":\"value\"}";

            // Act & Assert
            var exception = Record.Exception(() => JsonHelperValidation.EnsureValid(json));
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => JsonHelperValidation.EnsureValid(null!));
        }

        [Fact]
        public void EnsureValid_EmptyString_ThrowsArgumentException()
        {
            // Arrange
            var json = "";

            // Act
            var exception = Assert.Throws<ArgumentException>(() => JsonHelperValidation.EnsureValid(json));

            // Assert
            Assert.Contains("JSON string cannot be null, empty, or whitespace.", exception.Message);
        }

        [Fact]
        public void EnsureValid_InvalidJson_ThrowsArgumentException()
        {
            // Arrange
            var json = "invalid json";

            // Act
            var exception = Assert.Throws<ArgumentException>(() => JsonHelperValidation.EnsureValid(json));

            // Assert
            Assert.Contains("The provided string is not a valid JSON.", exception.Message);
        }
    }
}
