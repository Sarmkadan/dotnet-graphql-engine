#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GraphQLEngine.Configuration;
using Xunit;

namespace dotnet_graphql_engine.Tests;

public sealed class DependencyInjectionValidationTests
{
    // ------------------------------------------------------------------------
    // Helper validator used for IServiceCollection tests
    // ------------------------------------------------------------------------
    private sealed class DummyValidator<T> : IValidateOptions<T> where T : class
    {
        public ValidateOptionsResult Validate(string? name, T options) => ValidateOptionsResult.Success;
    }

    // ------------------------------------------------------------------------
    // GraphQLEngineOptions extension methods
    // ------------------------------------------------------------------------
    [Fact]
    public void Validate_GraphQLEngineOptions_ReturnsEmptyWhenValid()
    {
        var options = new GraphQLEngineOptions(); // default instance assumed valid
        var result = options.Validate();

        Assert.Empty(result);
    }

    [Fact]
    public void Validate_GraphQLEngineOptions_ThrowsArgumentNullException_WhenNull()
    {
        GraphQLEngineOptions? options = null;
        Assert.Throws<ArgumentNullException>(() => options!.Validate());
    }

    [Fact]
    public void IsValid_GraphQLEngineOptions_ReturnsTrueWhenValid()
    {
        var options = new GraphQLEngineOptions();
        Assert.True(options.IsValid());
    }

    [Fact]
    public void EnsureValid_GraphQLEngineOptions_DoesNotThrowWhenValid()
    {
        var options = new GraphQLEngineOptions();
        var exception = Record.Exception(() => options.EnsureValid());
        Assert.Null(exception);
    }

    // ------------------------------------------------------------------------
    // DotnetGraphqlEngineOptions extension methods
    // ------------------------------------------------------------------------
    [Fact]
    public void Validate_DotnetGraphqlEngineOptions_ReturnsEmptyWhenValid()
    {
        var options = new DotnetGraphqlEngineOptions(); // default instance assumed valid
        var result = options.Validate();

        Assert.Empty(result);
    }

    [Fact]
    public void Validate_DotnetGraphqlEngineOptions_ThrowsArgumentNullException_WhenNull()
    {
        DotnetGraphqlEngineOptions? options = null;
        Assert.Throws<ArgumentNullException>(() => options!.Validate());
    }

    [Fact]
    public void IsValid_DotnetGraphqlEngineOptions_ReturnsTrueWhenValid()
    {
        var options = new DotnetGraphqlEngineOptions();
        Assert.True(options.IsValid());
    }

    [Fact]
    public void EnsureValid_DotnetGraphqlEngineOptions_DoesNotThrowWhenValid()
    {
        var options = new DotnetGraphqlEngineOptions();
        var exception = Record.Exception(() => options.EnsureValid());
        Assert.Null(exception);
    }

    // ------------------------------------------------------------------------
    // IServiceCollection extension methods
    // ------------------------------------------------------------------------
    [Fact]
    public void Validate_ServiceCollection_ReturnsErrorsWhenRequiredRegistrationsMissing()
    {
        var services = new ServiceCollection(); // empty collection
        var errors = services.Validate();

        Assert.Contains("IOptions<GraphQLEngineOptions> is not registered. Call AddGraphQLEngine() first.", errors);
        Assert.Contains("IValidateOptions<GraphQLEngineOptions> is not registered. Call AddGraphQLEngine() first.", errors);
        Assert.Contains("IValidateOptions<DotnetGraphqlEngineOptions> is not registered. Call AddGraphQLEngine() first.", errors);
        Assert.Equal(3, errors.Count);
    }

    [Fact]
    public void Validate_ServiceCollection_ReturnsEmptyWhenAllRegistrationsPresent()
    {
        var services = new ServiceCollection();

        // Register IOptions<GraphQLEngineOptions>
        services.AddSingleton<IOptions<GraphQLEngineOptions>>(
            new OptionsWrapper<GraphQLEngineOptions>(new GraphQLEngineOptions()));

        // Register validators
        services.AddSingleton<IValidateOptions<GraphQLEngineOptions>, DummyValidator<GraphQLEngineOptions>>();
        services.AddSingleton<IValidateOptions<DotnetGraphqlEngineOptions>, DummyValidator<DotnetGraphqlEngineOptions>>();

        var errors = services.Validate();

        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_ServiceCollection_ReturnsFalseWhenInvalid()
    {
        var services = new ServiceCollection(); // missing registrations
        Assert.False(services.IsValid());
    }

    [Fact]
    public void IsValid_ServiceCollection_ReturnsTrueWhenValid()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IOptions<GraphQLEngineOptions>>(
            new OptionsWrapper<GraphQLEngineOptions>(new GraphQLEngineOptions()));
        services.AddSingleton<IValidateOptions<GraphQLEngineOptions>, DummyValidator<GraphQLEngineOptions>>();
        services.AddSingleton<IValidateOptions<DotnetGraphqlEngineOptions>, DummyValidator<DotnetGraphqlEngineOptions>>();

        Assert.True(services.IsValid());
    }

    [Fact]
    public void EnsureValid_ServiceCollection_ThrowsArgumentExceptionWhenInvalid()
    {
        var services = new ServiceCollection(); // empty -> invalid
        var ex = Assert.Throws<ArgumentException>(() => services.EnsureValid());

        Assert.Contains("IServiceCollection validation failed:", ex.Message);
        Assert.Contains("IOptions<GraphQLEngineOptions> is not registered", ex.Message);
    }

    [Fact]
    public void EnsureValid_ServiceCollection_DoesNotThrowWhenValid()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IOptions<GraphQLEngineOptions>>(
            new OptionsWrapper<GraphQLEngineOptions>(new GraphQLEngineOptions()));
        services.AddSingleton<IValidateOptions<GraphQLEngineOptions>, DummyValidator<GraphQLEngineOptions>>();
        services.AddSingleton<IValidateOptions<DotnetGraphqlEngineOptions>, DummyValidator<DotnetGraphqlEngineOptions>>();

        var exception = Record.Exception(() => services.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_ServiceCollection_ThrowsArgumentNullException_WhenNull()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(() => services!.Validate());
    }

    [Fact]
    public void IsValid_ServiceCollection_ThrowsArgumentNullException_WhenNull()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(() => services!.IsValid());
    }

    [Fact]
    public void EnsureValid_ServiceCollection_ThrowsArgumentNullException_WhenNull()
    {
        IServiceCollection? services = null;
        Assert.Throws<ArgumentNullException>(() => services!.EnsureValid());
    }
}
