using System;
using System.Security.Claims;
using GraphQLEngine.Api.Middleware;
using GraphQLEngine.Exceptions;
using Xunit;

namespace GraphQLEngine.Tests;

public class AuthenticationResultExtensionsTests
{
    [Fact]
    public void Match_Success_ReturnsOkResult()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user123")
        }));
        var result = new AuthenticationResult
        {
            Success = true,
            Principal = principal
        };

        // Act
        var value = result.Match(
            ok: ctx => ctx.UserId,
            failed: err => err);

        // Assert
        Assert.Equal("user123", value);
    }

    [Fact]
    public void Match_Failure_ReturnsFailedResult()
    {
        // Arrange
        var result = new AuthenticationResult
        {
            Success = false,
            Error = "invalid credentials"
        };

        // Act
        var value = result.Match(
            ok: ctx => "should not be called",
            failed: err => err);

        // Assert
        Assert.Equal("invalid credentials", value);
    }

    [Fact]
    public void ThrowIfFailed_Success_DoesNotThrow()
    {
        // Arrange
        var result = new AuthenticationResult { Success = true };

        // Act / Assert
        var exception = Record.Exception(() => result.ThrowIfFailed());
        Assert.Null(exception);
    }

    [Fact]
    public void ThrowIfFailed_Failure_ThrowsGraphQLException()
    {
        // Arrange
        var result = new AuthenticationResult
        {
            Success = false,
            Error = "bad auth"
        };

        // Act / Assert
        Assert.Throws<GraphQLException>(() => result.ThrowIfFailed());
    }

    [Fact]
    public void IsAuthenticated_ReturnsCorrectValues()
    {
        // Arrange
        var successResult = new AuthenticationResult { Success = true };
        var failureResult = new AuthenticationResult { Success = false };

        // Act / Assert
        Assert.True(successResult.IsAuthenticated());
        Assert.False(failureResult.IsAuthenticated());
    }
}
