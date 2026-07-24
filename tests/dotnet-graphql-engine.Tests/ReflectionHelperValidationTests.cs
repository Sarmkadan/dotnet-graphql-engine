using System;
using System.Collections.Generic;
using Xunit;
using dotnet_graphql_engine.Common.Utilities;

namespace dotnet_graphql_engine.Tests;

public class ReflectionHelperValidationTests
{
    private class Dummy { }

    [Fact]
    public void Validate_ReturnsEmptyList_ForValidType()
    {
        // Act
        IReadOnlyList<string> result = ReflectionHelperValidation.Validate(typeof(Dummy));

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidType()
    {
        // Act
        bool isValid = ReflectionHelperValidation.IsValid(typeof(Dummy));

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidType()
    {
        // Act & Assert
        Exception? ex = Record.Exception(() => ReflectionHelperValidation.EnsureValid(typeof(Dummy)));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_ForNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ReflectionHelperValidation.Validate(null!));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        // Act
        bool isValid = ReflectionHelperValidation.IsValid(null);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_ForNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ReflectionHelperValidation.EnsureValid(null!));
    }
}
