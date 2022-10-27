# Migration Guide: v1.x to v2.0

This document covers all breaking changes introduced in v2.0.0 and provides step-by-step upgrade instructions.

## Overview

v2.0 focuses on production hardening: improved Docker support, stricter defaults, and better alignment with ASP.NET Core conventions. Most applications will require only configuration changes.

## Breaking Changes

### 1. Default Port Changed: 5000 -> 8080

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

The Dockerfile now uses a three-stage build (build -> publish -> runtime) instead of two stages. This produces a smaller final image.

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

## Step-by-Step Upgrade

1. **Update the NuGet package** (or pull the latest source):
   ```bash
   dotnet add package Zaiets.dotnet.graphql.engine --version 2.0.0
   ```

2. **Update port references** in your deployment configuration:
   - Docker: change `-p 5000:5000` to `-p 8080:8080`
   - Reverse proxy (nginx/Caddy): update upstream to port 8080
   - Kubernetes: update container port and service targetPort

3. **Review `appsettings.json`** for any options where you relied on old defaults (see table above). Set them explicitly if needed.

4. **Rebuild Docker image:**
   ```bash
   docker build -t dotnet-graphql-engine:2.0 .
   ```

5. **Test health check:**
   ```bash
   curl http://localhost:8080/health
   ```

6. **Run integration tests** to verify no regressions in query execution, subscriptions, or schema stitching.

## Rollback

If you need to roll back to v1.x:
1. Revert the package version to 1.0.0
2. Restore port 5000 in your Docker/compose configuration
3. The API surface is backward compatible - no code changes required for rollback

## Questions

Open an issue at [github.com/sarmkadan/dotnet-graphql-engine](https://github.com/sarmkadan/dotnet-graphql-engine) or see the [FAQ](faq.md).
