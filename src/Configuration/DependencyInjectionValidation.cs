#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Validation helpers for dependency injection configuration
/// </summary>
public static class DependencyInjectionValidation
{
    /// <summary>
    /// Validates a GraphQLEngineOptions instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The options to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this GraphQLEngineOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Validate();
    }

    /// <summary>
    /// Validates a DotnetGraphqlEngineOptions instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The options to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this DotnetGraphqlEngineOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Validate();
    }

    /// <summary>
    /// Checks if a GraphQLEngineOptions instance is valid
    /// </summary>
    /// <param name="value">The options to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
public static bool IsValid(this GraphQLEngineOptions value)
{
    ArgumentNullException.ThrowIfNull(value);
    return value.Validate().Count == 0;
}

    /// <summary>
    /// Checks if a DotnetGraphqlEngineOptions instance is valid
    /// </summary>
    /// <param name="value">The options to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
public static bool IsValid(this DotnetGraphqlEngineOptions value)
{
    ArgumentNullException.ThrowIfNull(value);
    return value.Validate().Count == 0;
}

    /// <summary>
    /// Validates a GraphQLEngineOptions instance and throws ArgumentException if invalid
    /// </summary>
    /// <param name="value">The options to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, with detailed error messages</exception>
    public static void EnsureValid(this GraphQLEngineOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "GraphQLEngineOptions validation failed:\n" + string.Join("\n", errors),
                nameof(value)
            );
        }
    }

    /// <summary>
    /// Validates a DotnetGraphqlEngineOptions instance and throws ArgumentException if invalid
    /// </summary>
    /// <param name="value">The options to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, with detailed error messages</exception>
    public static void EnsureValid(this DotnetGraphqlEngineOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "DotnetGraphqlEngineOptions validation failed:\n" + string.Join("\n", errors),
                nameof(value)
            );
        }
    }

    /// <summary>
    /// Validates a service collection configuration for GraphQL engine services
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is null</exception>
    public static IReadOnlyList<string> Validate(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var errors = new List<string>();

        // Check if required services are registered
        bool hasOptionsRegistration = false;
        bool hasGraphQLEngineOptionsValidator = false;
        bool hasDotnetGraphqlEngineOptionsValidator = false;

        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == typeof(Microsoft.Extensions.Options.IOptions<GraphQLEngineOptions>))
            {
                hasOptionsRegistration = true;
            }
            else if (descriptor.ServiceType == typeof(Microsoft.Extensions.Options.IValidateOptions<GraphQLEngineOptions>))
            {
                hasGraphQLEngineOptionsValidator = true;
            }
            else if (descriptor.ServiceType == typeof(Microsoft.Extensions.Options.IValidateOptions<DotnetGraphqlEngineOptions>))
            {
                hasDotnetGraphqlEngineOptionsValidator = true;
            }
        }

        if (!hasOptionsRegistration)
        {
            errors.Add("IOptions<GraphQLEngineOptions> is not registered. Call AddGraphQLEngine() first.");
        }

        if (!hasGraphQLEngineOptionsValidator)
        {
            errors.Add("IValidateOptions<GraphQLEngineOptions> is not registered. Call AddGraphQLEngine() first.");
        }

        if (!hasDotnetGraphqlEngineOptionsValidator)
        {
            errors.Add("IValidateOptions<DotnetGraphqlEngineOptions> is not registered. Call AddGraphQLEngine() first.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a service collection configuration is valid
    /// </summary>
    /// <param name="services">The service collection to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is null</exception>
public static bool IsValid(this IServiceCollection services)
{
    ArgumentNullException.ThrowIfNull(services);
    return services.Validate().Count == 0;
}

    /// <summary>
    /// Validates a service collection configuration and throws ArgumentException if invalid
    /// </summary>
    /// <param name="services">The service collection to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, with detailed error messages</exception>
    public static void EnsureValid(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var errors = services.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "IServiceCollection validation failed:\n" + string.Join("\n", errors),
                nameof(services)
            );
        }
    }
}