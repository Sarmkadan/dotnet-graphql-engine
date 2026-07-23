using System;
using System.Collections.Generic;
using System.Text.Json;
using GraphQLEngine.Exceptions;
using Xunit;

namespace dotnet_graphql_engine.Tests;

public class GraphQLExceptionExtensionsTests
{
    [Fact]
    public void AddExtensions_HappyPath_AddsAllExtensions()
    {
        var ex = new GraphQLException("test");
        var extensions = new Dictionary<string, object>
        {
            { "code", 123 },
            { "detail", "some detail" }
        };

        ex.AddExtensions(extensions);

        Assert.Equal(2, ex.Extensions.Count);
        Assert.Equal(123, ex.Extensions["code"]);
        Assert.Equal("some detail", ex.Extensions["detail"]);
    }

    [Fact]
    public void AddExtensions_NullException_ThrowsArgumentNullException()
    {
        var extensions = new Dictionary<string, object>();
        Assert.Throws<ArgumentNullException>(() => ((GraphQLException)null!).AddExtensions(extensions));
    }

    [Fact]
    public void AddExtensions_NullExtensions_ThrowsArgumentNullException()
    {
        var ex = new GraphQLException("test");
        Assert.Throws<ArgumentNullException>(() => ex.AddExtensions(null!));
    }

    [Fact]
    public void SerializeExtensions_HappyPath_ReturnsJson()
    {
        var ex = new GraphQLException("test");
        ex.AddExtension("key1", "value1");
        ex.AddExtension("key2", 42);

        var json = ex.SerializeExtensions();

        Assert.False(string.IsNullOrEmpty(json));
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        Assert.NotNull(dict);
        Assert.Equal("value1", dict!["key1"]);
        Assert.Equal(42L, dict!["key2"]);
    }

    [Fact]
    public void SerializeExtensions_NoExtensions_ReturnsEmptyString()
    {
        var ex = new GraphQLException("test");
        var json = ex.SerializeExtensions();
        Assert.Empty(json);
    }

    [Fact]
    public void GetErrorCodeOrDefault_WithErrorCode_ReturnsIt()
    {
        var ex = new GraphQLException("test") { ErrorCode = "CUSTOM_ERROR" };
        var code = ex.GetErrorCodeOrDefault();
        Assert.Equal("CUSTOM_ERROR", code);
    }

    [Fact]
    public void GetErrorCodeOrDefault_WithoutErrorCode_ReturnsDefault()
    {
        var ex = new GraphQLException("test");
        var code = ex.GetErrorCodeOrDefault("DEFAULT");
        Assert.Equal("DEFAULT", code);
    }

    [Fact]
    public void WithContext_HappyPath_AppendsContext()
    {
        var ex = new GraphQLException("original");
        ex.AddExtension("foo", "bar");
        ex.ErrorCode = "ERR";

        var newEx = ex.WithContext("additional context");

        Assert.NotSame(ex, newEx);
        Assert.Contains("original | Context: additional context", newEx.Message);
        Assert.Equal("ERR", newEx.ErrorCode);
        Assert.Equal("bar", newEx.Extensions["foo"]);
    }

    [Fact]
    public void WithContext_NullContext_ReturnsSameException()
    {
        var ex = new GraphQLException("original");
        var result = ex.WithContext(null!);
        Assert.Same(ex, result);
    }

    [Fact]
    public void GetExtension_Typed_ReturnsCorrectValue()
    {
        var ex = new GraphQLException("test");
        ex.AddExtension("intKey", 99);
        ex.AddExtension("strKey", "hello");

        var intVal = ex.GetExtension<int>("intKey");
        var strVal = ex.GetExtension<string>("strKey");
        var missing = ex.GetExtension<double>("missing", -1.0);

        Assert.Equal(99, intVal);
        Assert.Equal("hello", strVal);
        Assert.Equal(-1.0, missing);
    }

    [Fact]
    public void GetExtension_NonExisting_ReturnsDefault()
    {
        var ex = new GraphQLException("test");
        var val = ex.GetExtension<string>("nonexistent", "fallback");
        Assert.Equal("fallback", val);
    }

    [Fact]
    public void AddFormattedErrorCode_HappyPath_AddsFormattedCode()
    {
        var ex = new GraphQLException("test") { ErrorCode = "MYCODE" };
        ex.AddFormattedErrorCode("SCHEMA");

        Assert.True(ex.Extensions.ContainsKey("errorCode"));
        Assert.Equal("SCHEMA_MYCODE", ex.Extensions["errorCode"]);
    }

    [Fact]
    public void AddFormattedErrorCode_NullPrefix_ThrowsArgumentException()
    {
        var ex = new GraphQLException("test");
        Assert.Throws<ArgumentException>(() => ex.AddFormattedErrorCode(null!));
    }

    [Fact]
    public void AddFormattedErrorCode_EmptyPrefix_ThrowsArgumentException()
    {
        var ex = new GraphQLException("test");
        Assert.Throws<ArgumentException>(() => ex.AddFormattedErrorCode(""));
    }

    [Fact]
    public void AddFormattedErrorCode_NullException_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((GraphQLException)null!).AddFormattedErrorCode("PREFIX"));
    }
}
