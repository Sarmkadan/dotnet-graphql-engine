using System;
using System.Collections.Generic;
using GraphQLEngine.Exceptions;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class GraphQLExceptionTests
    {
        [Fact]
        public void GraphQLException_Constructor_WithErrorCode_SetsProperties()
        {
            // Arrange & Act
            var ex = new GraphQLException("Test error", "TEST_CODE");

            // Assert
            Assert.Equal("Test error", ex.Message);
            Assert.Equal("TEST_CODE", ex.ErrorCode);
            Assert.NotNull(ex.Extensions);
            Assert.Empty(ex.Extensions);
        }

        [Fact]
        public void GraphQLException_AddExtension_AddsItem()
        {
            // Arrange
            var ex = new GraphQLException("Test");

            // Act
            ex.AddExtension("key", "value");

            // Assert
            Assert.Single(ex.Extensions);
            Assert.True(ex.Extensions.ContainsKey("key"));
            Assert.Equal("value", ex.Extensions["key"]);
        }

        [Fact]
        public void SchemaException_Constructor_SetsErrorCode()
        {
            // Arrange & Act
            var ex = new SchemaException("Schema issue");

            // Assert
            Assert.Equal("SCHEMA_ERROR", ex.ErrorCode);
            Assert.Equal("Schema issue", ex.Message);
        }

        [Fact]
        public void ExecutionException_Constructor_SetsFieldAndLine()
        {
            // Arrange & Act
            var ex = new ExecutionException("Exec failed", "user.name", 10);

            // Assert
            Assert.Equal("EXECUTION_ERROR", ex.ErrorCode);
            Assert.Equal("user.name", ex.FieldPath);
            Assert.Equal(10, ex.LineNumber);
        }

        [Fact]
        public void QueryComplexityException_Constructor_SetsScoresAndExtensions()
        {
            // Arrange & Act
            var ex = new QueryComplexityException("Too complex", 100, 50);

            // Assert
            Assert.Equal(100, ex.ActualScore);
            Assert.Equal(50, ex.MaxScore);
            Assert.True(ex.Extensions.ContainsKey("actualScore"));
            Assert.Equal(100, ex.Extensions["actualScore"]);
        }

        [Fact]
        public void ValidationException_Constructor_StoresErrors()
        {
            // Arrange
            var errors = new List<string> { "err1", "err2" };

            // Act
            var ex = new ValidationException("Validation failed", errors);

            // Assert
            Assert.Equal(2, ex.ValidationErrors.Count);
            Assert.Contains("err1", ex.ValidationErrors);
        }
    }
}
