// ... rest of the original README content ...

## PersistedQueryExtensions

The `PersistedQueryExtensions` class provides a set of utility methods for configuring and validating persisted queries. It allows you to add persisted queries to the service collection, enforce hash verification, set the maximum index size, and configure other settings.

### Usage Example

```csharp
using GraphQLEngine.Configuration;

// Add persisted queries to the service collection
var services = new ServiceCollection();
services.AddPersistedQueries();

// Configure persisted query settings
services.AddPersistedQueries(options =>
{
    options.EnforceHashVerification = true;
    options.MaxIndexSize = 1024;
    options.AllowlistOnly = false;
    options.ReturnNotFoundError = true;
    options.Validate = true;
});
```

// ... rest of the original README content ...
