using System;
using GraphQLEngine.Exceptions;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class GraphQLExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidException_ReturnsJsonString()
        {
            // Arrange
            var exception = new GraphQLException("Test error");

            // Act
            var json = GraphQLExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            // The default serializer uses camelCase naming, so "message" should appear.
            Assert.Contains("\"message\":\"Test error\"", json);
            // Default is non‑indented.
            Assert.DoesNotContain("\n", json);
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsIndentedJson()
        {
            // Arrange
            var exception = new GraphQLException("Indented error");

            // Act
            var json = GraphQLExceptionJsonExtensions.ToJson(exception, indented: true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\n", json); // indented JSON contains line breaks
        }

        [Fact]
        public void ToJson_NullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => GraphQLExceptionJsonExtensions.ToJson(null!));
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsException()
        {
            // Arrange
            var json = "{\"message\":\"Deserialized error\"}";

            // Act
            var exception = GraphQLExceptionJsonExtensions.FromJson(json);

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
            var exception = GraphQLExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => GraphQLExceptionJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndException()
        {
            // Arrange
            var json = "{\"message\":\"TryFromJson success\"}";

            // Act
            var result = GraphQLExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            Assert.True(result);
            Assert.NotNull(exception);
            Assert.Equal("TryFromJson success", exception!.Message);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "not a json string";

            // Act
            var result = GraphQLExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            Assert.False(result);
            Assert.Null(exception);
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => GraphQLExceptionJsonExtensions.TryFromJson(null!, out _));
        }
    }
}
