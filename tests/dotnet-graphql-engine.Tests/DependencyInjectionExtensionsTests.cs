#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using GraphQLEngine.Configuration;
using GraphQLEngine.Data.Repositories;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

using ExecutionContext = GraphQLEngine.Domain.Entities.ExecutionContext;

namespace GraphQLEngine.Tests.Configuration;

sealed public class DependencyInjectionExtensionsTests
{
    [Fact]
    public void AddGraphQLEngineWithLogging_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        Action act = () => services!.AddGraphQLEngineWithLogging();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddGraphQLEngineWithLogging_WithNullConfigureLogging_ConfiguresConsoleLogging()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGraphQLEngineWithLogging(configureLogging: null);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().ContainSingle(s => s.ServiceType == typeof(ILoggerFactory));
        var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
        loggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddGraphQLEngineWithLogging_WithCustomLoggingConfiguration_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<ILoggingBuilder> configureLogging = builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddFilter("System", LogLevel.Warning);
        };

        // Act
        var result = services.AddGraphQLEngineWithLogging(configureLogging);

        // Assert
        result.Should().BeSameAs(services);
        var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
        loggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddGraphQLEngineWithLogging_WithConfigureOptions_ConfiguresGraphQLEngineOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<GraphQLEngineOptions> configureOptions = options =>
        {
            options.MaxQueryComplexity = 5000;
            options.EnableDetailedErrorMessages = true;
        };

        // Act
        var result = services.AddGraphQLEngineWithLogging(configureOptions: configureOptions);

        // Assert
        result.Should().BeSameAs(services);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<GraphQLEngineOptions>>().Value;
        options.MaxQueryComplexity.Should().Be(5000);
        options.EnableDetailedErrorMessages.Should().BeTrue();
    }

    [Fact]
    public void AddGraphQLEngineWithValidation_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        Action act = () => services!.AddGraphQLEngineWithValidation();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddGraphQLEngineWithValidation_WithDefaultConfiguration_SetsDefaultValues()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGraphQLEngineWithValidation();

        // Assert
        result.Should().BeSameAs(services);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<GraphQLEngineOptions>>().Value;
        options.MaxQueryComplexity.Should().Be(1000);
        options.MaxQueryDepth.Should().Be(5);
        options.QueryTimeoutMs.Should().Be(10000);
        options.EnableDetailedErrorMessages.Should().BeFalse();
    }

    [Fact]
    public void AddGraphQLEngineWithValidation_WithCustomConfiguration_AppliesCustomValues()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<GraphQLEngineOptions> configureOptions = options =>
        {
            options.MaxQueryComplexity = 2000;
            options.MaxQueryDepth = 10;
            options.QueryTimeoutMs = 30000;
            options.EnableDetailedErrorMessages = true;
        };

        // Act
        var result = services.AddGraphQLEngineWithValidation(configureOptions);

        // Assert
        result.Should().BeSameAs(services);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<GraphQLEngineOptions>>().Value;
        options.MaxQueryComplexity.Should().Be(2000);
        options.MaxQueryDepth.Should().Be(10);
        options.QueryTimeoutMs.Should().Be(30000);
        options.EnableDetailedErrorMessages.Should().BeTrue();
    }

    [Fact]
    public void AddGraphQLEngineWithValidation_WithEmptyConfiguration_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<GraphQLEngineOptions>? configureOptions = null;

        // Act
        var result = services.AddGraphQLEngineWithValidation(configureOptions);

        // Assert
        result.Should().BeSameAs(services);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<GraphQLEngineOptions>>().Value;
        options.MaxQueryComplexity.Should().Be(1000);
        options.MaxQueryDepth.Should().Be(5);
        options.QueryTimeoutMs.Should().Be(10000);
        options.EnableDetailedErrorMessages.Should().BeFalse();
    }

    [Fact]
    public void AddGraphQLEngineWithRepositoryLifetime_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        Action act = () => services!.AddGraphQLEngineWithRepositoryLifetime();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddGraphQLEngineWithRepositoryLifetime_WithDefaultSingletonLifetime_RegistersAllRepositoriesAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGraphQLEngineWithRepositoryLifetime();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLSchema>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLType>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLQuery>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<ExecutionContext>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<DataLoaderRequest>) && s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddGraphQLEngineWithRepositoryLifetime_WithScopedLifetime_RegistersAllRepositoriesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGraphQLEngineWithRepositoryLifetime(ServiceLifetime.Scoped);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<>) && s.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLSchema>) && s.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLType>) && s.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLQuery>) && s.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<ExecutionContext>) && s.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<DataLoaderRequest>) && s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddGraphQLEngineWithRepositoryLifetime_WithTransientLifetime_RegistersAllRepositoriesAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGraphQLEngineWithRepositoryLifetime(ServiceLifetime.Transient);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<>) && s.Lifetime == ServiceLifetime.Transient);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLSchema>) && s.Lifetime == ServiceLifetime.Transient);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLType>) && s.Lifetime == ServiceLifetime.Transient);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLQuery>) && s.Lifetime == ServiceLifetime.Transient);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<ExecutionContext>) && s.Lifetime == ServiceLifetime.Transient);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<DataLoaderRequest>) && s.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void AddGraphQLEngineWithRepositoryLifetime_WithExplicitSingletonLifetime_RegistersAllRepositoriesAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGraphQLEngineWithRepositoryLifetime(ServiceLifetime.Singleton);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLSchema>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLType>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<GraphQLQuery>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<ExecutionContext>) && s.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(s => s.ServiceType == typeof(IRepository<DataLoaderRequest>) && s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddGraphQLEngineWithSchemaStitching_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act
        Action act = () => services!.AddGraphQLEngineWithSchemaStitching();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddGraphQLEngineWithSchemaStitching_WithNullSchemaName_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        string? schemaName = null;

        // Act
        Action act = () => services.AddGraphQLEngineWithSchemaStitching(schemaName);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddGraphQLEngineWithSchemaStitching_WithDefaultSchemaName_RegistersSchemaStitchingWithDefaultName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddGraphQLEngineWithSchemaStitching();

        // Assert
        result.Should().BeSameAs(services);
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<SchemaStitchingConfig>();
        config.Name.Should().Be("default");
    }

    [Fact]
    public void AddGraphQLEngineWithSchemaStitching_WithCustomSchemaName_RegistersSchemaStitchingWithCustomName()
    {
        // Arrange
        var services = new ServiceCollection();
        var customName = "my-custom-schema";

        // Act
        var result = services.AddGraphQLEngineWithSchemaStitching(customName);

        // Assert
        result.Should().BeSameAs(services);
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<SchemaStitchingConfig>();
        config.Name.Should().Be(customName);
    }

    [Fact]
    public void AddGraphQLEngineWithSchemaStitching_WithEmptySchemaName_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var emptyName = string.Empty;

        // Act
        Action act = () => services.AddGraphQLEngineWithSchemaStitching(emptyName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGraphQLEngineWithSchemaStitching_WithWhitespaceSchemaName_RegistersSchemaStitchingWithTrimmedName()
    {
        // Arrange
        var services = new ServiceCollection();
        var whitespaceName = "  trimmed-schema  ";

        // Act
        var result = services.AddGraphQLEngineWithSchemaStitching(whitespaceName);

        // Assert
        result.Should().BeSameAs(services);
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<SchemaStitchingConfig>();
        config.Name.Should().Be("trimmed-schema");
    }
}