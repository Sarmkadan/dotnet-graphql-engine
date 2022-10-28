# Migration Guide: v1.x to v2.0

This document covers all breaking changes introduced in v2.0.0 and provides step-by-step upgrade instructions.

## Overview

v2.0 introduces **federation support**, **production hardening improvements**, and **better alignment with ASP.NET Core conventions**. Most applications will require minimal changes, primarily configuration updates.

## What's New in v2.0

### 🚀 Federation Support

The biggest new feature in v2.0 is **GraphQL Federation** support with:

- **Entity Resolution** - Resolve entities across multiple GraphQL services
- **Schema Composition** - Automatically compose schemas from multiple services
- **Type Extensions** - Extend types from other services
- **@key directive support** - Define entity keys for cross-service resolution

### 🔧 Enhanced Configuration

- **Automatic Federation Discovery** - Services can discover and compose federated schemas automatically
- **Entity Cache** - Improved performance for entity resolution with intelligent caching
- **Federation Health Checks** - Monitor federation endpoints and entity resolution

### 📦 Production Improvements

- **Better Docker Support** - Multi-stage builds, smaller images, health checks
- **Stricter Defaults** - Production-safe configuration by default
- **Improved Error Messages** - More actionable error reporting
- **Enhanced Logging** - Better structured logging for debugging

## Breaking Changes

### 1. Default Port Changed: 5000 → 8080

The default listening port has been changed from `5000` to `8080` to align with ASP.NET Core 10 container defaults and avoid conflicts with macOS AirPlay Receiver.

**Before (v1.x):**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://+:5000" }
    }
  }
}
```

**After (v2.0):**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://+:8080" }
    }
  }
}
```

If you set `ASPNETCORE_URLS` explicitly, update it to `http://+:8080`. If you rely on the default, no change is needed - the Dockerfile and docker-compose.yml already use 8080.

### 2. Docker Image Restructured

The Dockerfile now uses a three-stage build (build → publish → runtime) instead of two stages. This produces a smaller final image.

**Actions required:**
- Update any CI/CD scripts that reference Docker stage names (`builder` stage remains, `publish` stage is new, `runtime` replaces the old unnamed runtime stage)
- Update port mappings in deployment configs from `5000` to `8080`
- If you use `docker-compose.yml`, the updated file handles this automatically

### 3. GraphQLEngineOptions Defaults

Several default values have changed for production safety:

| Option | v1.x Default | v2.0 Default |
|--------|-------------|-------------|
| `MaxQueryDepth` | 10 | 15 |
| `MaxQueryFields` | 100 | 200 |
| `QueryTimeoutMs` | 10000 | 30000 |
| `CacheMaxSizeBytes` | 52428800 (50 MB) | 104857600 (100 MB) |

If your application depends on the old defaults, set them explicitly in `appsettings.json` or environment variables.

### 4. Health Check Endpoint

The health check timing has been adjusted:
- `start-period` reduced from 40s to 30s (faster container orchestration feedback)
- No URL path changes - `/health` remains the endpoint

### 5. Redis Dependency in docker-compose

The `graphql-engine` service now declares `depends_on` with `condition: service_healthy` for Redis. This means the engine container waits for Redis to be healthy before starting. If you run without Redis, remove or comment out the `depends_on` block.

### 6. New Federation Configuration Options

v2.0 introduces new configuration options for federation support:

```csharp
services.AddGraphQLEngine(options => {
    // Enable federation
    options.EnableFederation = true;
    
    // Federation configuration
    options.FederationDiscoveryEndpoint = "/.well-known/federation";
    options.FederationTimeout = TimeSpan.FromSeconds(30);
    
    // Entity cache settings
    options.EntityCacheTtlSeconds = 300;
    options.EntityCacheMaxSize = 10000;
});
```

If you're not using federation, these options have sensible defaults and won't affect your application.

## Step-by-Step Upgrade

### For Existing v1.x Applications

1. **Update the NuGet package** (or pull the latest source):
```bash
dotnet add package Zaiets.dotnet.graphql.engine --version 2.0.0
```

2. **Update port references** in your deployment configuration:
- Docker: change `-p 5000:5000` to `-p 8080:8080`
- Reverse proxy (nginx/Caddy): update upstream to port 8080
- Kubernetes: update container port and service targetPort

3. **Review `appsettings.json`** for any options where you relied on old defaults (see table above). Set them explicitly if needed.

4. **Update federation configuration** (if using federation):
```json
{
  "GraphQL": {
    "EnableFederation": true,
    "FederationDiscoveryEndpoint": "/.well-known/federation",
    "FederationTimeout": 30000
  }
}
```

5. **Rebuild Docker image:**
```bash
docker build -t dotnet-graphql-engine:2.0 .
```

6. **Test health check:**
```bash
curl http://localhost:8080/health
```

7. **Run integration tests** to verify no regressions in query execution, subscriptions, or schema stitching.

### For New Applications

If starting fresh with v2.0, you can skip the migration steps and use the new features directly:

```csharp
// Enable federation from the start
services.AddGraphQLEngine(options => {
    options.EnableFederation = true;
    options.EnableSchemaStitching = true;
    options.EnableSubscriptions = true;
    options.MaxQueryComplexity = 5000;
});
```

## Federation Migration Guide

### What is Federation?

GraphQL Federation allows you to split your GraphQL schema across multiple services while still presenting a unified API to clients. Each service can own its own types, and the gateway automatically composes them into a single schema.

### Before (Non-Federated)

```csharp
// Single schema definition
var schema = graphqlEngine.CreateSchema("Main");
graphqlEngine.AddType("Main", userType);
graphqlEngine.AddType("Main", postType);
```

### After (Federated)

**Service 1 - Users Service:**
```csharp
services.AddGraphQLEngine(options => {
    options.EnableFederation = true;
    options.ServiceName = "UsersService";
});

// Define your types as usual
var userType = new GraphQLType {
    Name = "User",
    Fields = new List<GraphQLField> {
        new() { Name = "id", Type = "ID!" },
        new() { Name = "name", Type = "String!" }
    }
};

graphqlEngine.AddType("UsersService", userType);
```

**Service 2 - Posts Service:**
```csharp
services.AddGraphQLEngine(options => {
    options.EnableFederation = true;
    options.ServiceName = "PostsService";
});

// Define Post type that extends User
var postType = new GraphQLType {
    Name = "Post",
    Fields = new List<GraphQLField> {
        new() { Name = "id", Type = "ID!" },
        new() { Name = "title", Type = "String!" },
        new() { Name = "author", Type = "User! @external" } // Extends User type
    }
};

graphqlEngine.AddType("PostsService", postType);

// Define User type with key for entity resolution
var userType = new GraphQLType {
    Name = "User",
    KeyFields = new List<string> { "id" },
    Fields = new List<GraphQLField> {
        new() { Name = "id", Type = "ID! @key" },
        new() { Name = "posts", Type = "[Post!]!" }
    }
};

graphqlEngine.AddType("PostsService", userType);
```

**Gateway Service:**
```csharp
services.AddGraphQLEngine(options => {
    options.EnableFederation = true;
    options.EnableSchemaComposition = true;
    options.CompositionTimeout = TimeSpan.FromSeconds(30);
});

// The gateway will automatically compose schemas from all services
var schemaService = serviceProvider.GetRequiredService<SchemaService>();
var composedSchema = await schemaService.ComposeFederatedSchemaAsync();
```

## Rollback

If you need to roll back to v1.x:
1. Revert the package version to 1.0.0
2. Restore port 5000 in your Docker/compose configuration
3. The API surface is backward compatible - no code changes required for rollback

## Common Migration Scenarios

### Scenario 1: Simple API without Federation

**Before:**
```csharp
builder.Services.AddGraphQLEngine(options => {
    options.MaxQueryComplexity = 5000;
});
```

**After:**
```csharp
builder.Services.AddGraphQLEngine(options => {
    options.MaxQueryComplexity = 5000;
    // Port change handled by Docker defaults
});
```

### Scenario 2: API with Schema Stitching

**Before:**
```csharp
services.AddGraphQLEngine(options => {
    options.EnableSchemaStitching = true;
    options.RemoteSchemaTimeout = TimeSpan.FromSeconds(30);
});
```

**After:**
```csharp
services.AddGraphQLEngine(options => {
    options.EnableSchemaStitching = true;
    options.EnableFederation = false; // Keep disabled if not using
    options.RemoteSchemaTimeout = TimeSpan.FromSeconds(30);
});
```

### Scenario 3: API with Custom Configuration File

**Before:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://+:5000" }
    }
  },
  "GraphQL": {
    "MaxQueryComplexity": 5000
  }
}
```

**After:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://+:8080" }
    }
  },
  "GraphQL": {
    "MaxQueryComplexity": 5000,
    "EnableFederation": false
  }
}
```

## Testing Your Migration

### 1. Health Check Test
```bash
# Should return 200 OK
curl -i http://localhost:8080/health
```

### 2. Query Execution Test
```bash
# Should return valid GraphQL response
curl -X POST http://localhost:8080/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "{ __schema { types { name } } }"}'
```

### 3. Federation Test (if using federation)
```bash
# Check federation endpoint
curl http://localhost:8080/.well-known/federation

# Should return JSON with service information
```

### 4. Performance Regression Test
```bash
# Run your existing performance tests
# Expected: slightly improved performance due to better defaults
```

## Frequently Asked Questions

### Q: Do I need to migrate to federation?
**A:** No! Federation is completely optional. If you're happy with your current setup, you can keep using v2.0 without enabling federation.

### Q: Will my existing queries break?
**A:** No. The GraphQL query language and execution model remain unchanged. All your existing queries will work exactly as before.

### Q: What if I don't update my port?
**A:** If you're using Docker or docker-compose, the updated files already use port 8080. If you're running directly, you'll need to update your configuration or use the new default.

### Q: How do I enable federation after migrating?
**A:** Set `EnableFederation = true` in your GraphQLEngineOptions and configure your services with `ServiceName`. See the Federation Migration Guide section above.

### Q: Can I mix federated and non-federated services?
**A:** Yes! The gateway can compose both federated and non-federated schemas together.

## Resources

- [Federation Documentation](architecture.md#federation)
- [Docker Guide](docker-guide.md)
- [Configuration Reference](api-reference.md#graphqlengineoptions)
- [Examples](../examples/)
- [API Reference](../README.md#api-reference)
