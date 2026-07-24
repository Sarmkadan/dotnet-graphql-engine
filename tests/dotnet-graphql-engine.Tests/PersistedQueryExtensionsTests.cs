using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using GraphQLEngine.Configuration;
using GraphQLEngine.Data.Repositories;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.GraphQL;

namespace GraphQLEngine.Tests;

public class PersistedQueryExtensionsTests
{
    [Fact]
    public void AddPersistedQueries_WithoutOptions_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddPersistedQueries();

        // Assert
        Assert.Same(services, result);
        Assert.Contains(services, s => s.ServiceType == typeof(IRepository<PersistedQuery>) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(PersistedQueryService) && s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPersistedQueries_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddPersistedQueries());
    }

    [Fact]
    public void AddPersistedQueries_WithConfigureOptions_RegistersServicesWithOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configureCalled = false;

        // Act
        var result = services.AddPersistedQueries(options =>
        {
            configureCalled = true;
            options.EnforceHashVerification = false;
            options.MaxIndexSize = 5000;
            options.AllowlistOnly = true;
            options.ReturnNotFoundError = false;
        });

        // Assert
        Assert.True(configureCalled);
        Assert.Same(services, result);
        Assert.Contains(services, s => s.ServiceType == typeof(PersistedQueryOptions));
        Assert.Contains(services, s => s.ServiceType == typeof(IRepository<PersistedQuery>) && s.Lifetime == ServiceLifetime.Singleton);
        Assert.Contains(services, s => s.ServiceType == typeof(PersistedQueryService) && s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPersistedQueries_WithNullConfigureOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<PersistedQueryOptions> configure = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddPersistedQueries(configure));
    }

    [Fact]
    public void AddPersistedQueries_WithNullServicesAndConfigureOptions_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        Action<PersistedQueryOptions> configure = _ => { };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddPersistedQueries(configure));
    }

    [Fact]
    public void AddPersistedQueries_WithInvalidOptions_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddPersistedQueries(options => options.MaxIndexSize = 0));

        Assert.Contains("MaxIndexSize must be greater than 0", exception.Message);
    }
}

public class PersistedQueryOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new PersistedQueryOptions();

        // Assert
        Assert.True(options.EnforceHashVerification);
        Assert.Equal(10_000, options.MaxIndexSize);
        Assert.False(options.AllowlistOnly);
        Assert.True(options.ReturnNotFoundError);
        Assert.True(options.Validate(out _));
    }

    [Fact]
    public void EnforceHashVerification_CanBeModified()
    {
        // Arrange
        var options = new PersistedQueryOptions();

        // Act
        options.EnforceHashVerification = false;

        // Assert
        Assert.False(options.EnforceHashVerification);
    }

    [Fact]
    public void MaxIndexSize_CanBeModified()
    {
        // Arrange
        var options = new PersistedQueryOptions();

        // Act
        options.MaxIndexSize = 5000;

        // Assert
        Assert.Equal(5000, options.MaxIndexSize);
    }

    [Fact]
    public void AllowlistOnly_CanBeModified()
    {
        // Arrange
        var options = new PersistedQueryOptions();

        // Act
        options.AllowlistOnly = true;

        // Assert
        Assert.True(options.AllowlistOnly);
    }

    [Fact]
    public void ReturnNotFoundError_CanBeModified()
    {
        // Arrange
        var options = new PersistedQueryOptions();

        // Act
        options.ReturnNotFoundError = false;

        // Assert
        Assert.False(options.ReturnNotFoundError);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidMaxIndexSize_ReturnsFalseAndErrors(int invalidSize)
    {
        // Arrange
        var options = new PersistedQueryOptions { MaxIndexSize = invalidSize };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Single(errors);
        Assert.Contains("MaxIndexSize must be greater than 0", errors[0]);
    }

    [Fact]
    public void Validate_WithValidOptions_ReturnsTrueAndEmptyErrors()
    {
        // Arrange
        var options = new PersistedQueryOptions { MaxIndexSize = 100 };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithZeroMaxIndexSize_AddsError()
    {
        // Arrange
        var options = new PersistedQueryOptions { MaxIndexSize = 0 };

        // Act
        var isValid = options.Validate(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Single(errors);
        Assert.Equal("MaxIndexSize must be greater than 0", errors[0]);
    }
}