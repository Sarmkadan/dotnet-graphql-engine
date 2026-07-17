# DependencyInjectionValidation

Provides static methods to validate dependency injection container configurations, ensuring all required services and their dependency graphs are correctly registered and resolvable before the application starts. This type is designed for use during startup or in test scenarios to catch misconfigurations early.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(IServiceCollection services)
public static IReadOnlyList<string> Validate(IServiceProvider provider)
public static IReadOnlyList<string> Validate(IServiceCollection services, Action<ValidationOptions> configure)
```

Validates the service registrations or an already-built provider and returns a list of error messages describing any missing or unresolvable dependencies. The overload accepting `IServiceCollection` examines registrations without building a provider. The overload accepting `IServiceProvider` validates against an already-built container. The overload with `configure` allows customization of validation behavior through `ValidationOptions`.

**Parameters:**
- `services`: The `IServiceCollection` containing service registrations to validate.
- `provider`: An already-built `IServiceProvider` to validate.
- `configure`: A delegate to configure `ValidationOptions` (e.g., strict mode, ignored types).

**Returns:** A read-only list of strings, each describing a validation failure. An empty list indicates no issues were found.

**Throws:** `ArgumentNullException` if `services`, `provider`, or `configure` is `null`.

### IsValid

```csharp
public static bool IsValid(IServiceCollection services)
public static bool IsValid(IServiceProvider provider)
public static bool IsValid(IServiceCollection services, Action<ValidationOptions> configure)
```

Convenience methods that return `true` if validation produces no errors, `false` otherwise. Equivalent to calling `Validate` and checking whether the returned list is empty.

**Parameters:** Same as the corresponding `Validate` overloads.

**Returns:** `true` if all registered services are resolvable; `false` if any dependency is missing or unresolvable.

**Throws:** `ArgumentNullException` if `services`, `provider`, or `configure` is `null`.

### EnsureValid

```csharp
public static void EnsureValid(IServiceCollection services)
public static void EnsureValid(IServiceProvider provider)
public static void EnsureValid(IServiceCollection services, Action<ValidationOptions> configure)
```

Performs validation and throws an aggregate exception containing all validation errors if any are found. Use this at application startup to fail fast when the DI container is misconfigured.

**Parameters:** Same as the corresponding `Validate` overloads.

**Throws:**
- `ArgumentNullException` if `services`, `provider`, or `configure` is `null`.
- `AggregateException` (or a derived exception type) containing individual error messages when validation fails.

## Usage

### Example 1: Startup validation with fail-fast

```csharp
var services = new ServiceCollection();
services.AddGraphQLEngine();
services.AddSingleton<IMyService, MyService>();

// Fail immediately if any required dependency is missing
DependencyInjectionValidation.EnsureValid(services);

var provider = services.BuildServiceProvider();
// Application continues knowing the container is correctly configured
```

### Example 2: Conditional validation with custom options

```csharp
var services = new ServiceCollection();
services.AddGraphQLEngine();
// Intentionally omit an optional service for a specific environment

bool isValid = DependencyInjectionValidation.IsValid(services, options =>
{
    options.IgnoreType<IOptionalService>();
    options.StrictMode = false;
});

if (!isValid)
{
    var errors = DependencyInjectionValidation.Validate(services, options =>
    {
        options.IgnoreType<IOptionalService>();
    });

    foreach (var error in errors)
    {
        Console.WriteLine($"DI Warning: {error}");
    }
}
```

## Notes

- Validation against `IServiceCollection` does not build a service provider, avoiding side effects from service construction during validation.
- Validation against `IServiceProvider` may resolve transient services, which could have side effects; prefer collection-based validation where possible.
- All methods are thread-safe when validating an `IServiceCollection` that is not being mutated concurrently. The caller must ensure the collection is not modified during validation.
- `EnsureValid` throws an aggregate exception; catch it at the application entry point to log details and terminate startup cleanly.
- `ValidationOptions` configuration is applied per call and does not persist across invocations.
- Open generics and factory-registered services are included in validation where their resolution can be statically analyzed.
