using System;
using System.Collections.Generic;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests;

public class TypeConverterTests
{
    [Fact]
    public void Convert_Generic_StringToInt_ReturnsParsedValue()
    {
        // Act
        var result = TypeConverter.Convert<int>("42");

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Convert_Generic_NullValue_ReturnsDefault()
    {
        // Act
        var result = TypeConverter.Convert<int>(null);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_Generic_InvalidConversion_ReturnsDefault()
    {
        // Act
        var result = TypeConverter.Convert<int>("not-a-number");

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_NonGeneric_WithType_ConvertsValue()
    {
        // Act
        var result = TypeConverter.Convert("123", typeof(long));

        // Assert
        Assert.Equal(123L, result);
    }

    [Fact]
    public void Convert_NonGeneric_NullValueForValueType_ReturnsDefaultInstance()
    {
        // Act
        var result = TypeConverter.Convert(null, typeof(int));

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryConvert_ValidValue_ReturnsTrueAndConvertedResult()
    {
        // Act
        var success = TypeConverter.TryConvert<double>("3.14", out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(3.14, result);
    }

    [Fact]
    public void CanConvert_PrimitiveTargetType_ReturnsTrue()
    {
        // Act
        var canConvert = TypeConverter.CanConvert(typeof(string), typeof(int));

        // Assert
        Assert.True(canConvert);
    }

    [Fact]
    public void CanConvert_NullTypes_ReturnsFalse()
    {
        // Act
        var canConvert = TypeConverter.CanConvert(null!, typeof(int));

        // Assert
        Assert.False(canConvert);
    }

    [Fact]
    public void GetDefaultValue_ValueType_ReturnsDefaultInstance()
    {
        // Act
        var result = TypeConverter.GetDefaultValue(typeof(int));

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetDefaultValue_ReferenceType_ReturnsNull()
    {
        // Act
        var result = TypeConverter.GetDefaultValue(typeof(string));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDefaultValue_NullType_ThrowsArgumentNullException()
    {
        // Act / Assert
        Assert.Throws<ArgumentNullException>(() => TypeConverter.GetDefaultValue(null!));
    }

    [Fact]
    public void ConvertList_NullInput_ReturnsEmptyList()
    {
        // Act
        var result = TypeConverter.ConvertList<int>(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ConvertList_MixedValues_ConvertsEachElement()
    {
        // Arrange
        var values = new List<object?> { "1", "2", "3" };

        // Act
        var result = TypeConverter.ConvertList<int>(values);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0]);
        Assert.Equal(2, result[1]);
        Assert.Equal(3, result[2]);
    }

    [Fact]
    public void ToJsonCompatible_NullValue_ReturnsNull()
    {
        // Act
        var result = TypeConverter.ToJsonCompatible(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToJsonCompatible_PrimitiveValue_ReturnsSameValue()
    {
        // Act
        var result = TypeConverter.ToJsonCompatible(42);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void ToJsonCompatible_Collection_ReturnsListOfConvertedItems()
    {
        // Arrange
        var values = new List<int> { 1, 2, 3 };

        // Act
        var result = TypeConverter.ToJsonCompatible(values) as List<object?>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result!.Count);
        Assert.Equal(1, result[0]);
        Assert.Equal(2, result[1]);
        Assert.Equal(3, result[2]);
    }
}
