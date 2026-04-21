# Docker Guide for dotnet-graphql-engine

This guide provides comprehensive instructions for deploying dotnet-graphql-engine using Docker and Docker Compose in production and development environments.

## Table of Contents

- [Quick Start with Docker](#quick-start-with-docker)
- [Docker Compose Usage](#docker-compose-usage)
  - [Development Setup](#development-setup)
  - [Production Setup](#production-setup)
  - [Custom Configuration](#custom-configuration)
- [Environment Variables Reference](#environment-variables-reference)
- [Production Deployment Checklist](#production-deployment-checklist)
- [Advanced Configuration](#advanced-configuration)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)

## Quick Start with Docker

### Prerequisites

- Docker 20.10+ or Docker Desktop 4.0+
- Docker Compose 2.0+ (for multi-container setups)
- 512MB+ RAM (1GB+ recommended for production)
- 2 CPU cores minimum

### 1-Minute Setup

```bash
# Pull the image (or build locally)
docker pull vladyslavzaiets/dotnet-graphql-engine:latest

# Run the container
# Maps port 8080 (container) to 8080 (host)
docker run -d -p 8080:8080 --name graphql-engine vladyslavzaiets/dotnet-graphql-engine:latest

# Test it
curl -X POST http://localhost:8080/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "{ __typename }"}'
```

Your GraphQL API is now available at `http://localhost:8080`

### Using docker-compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/vladyslavzaiets/dotnet-graphql-engine.git
cd dotnet-graphql-engine

# Start the services
docker-compose up -d

# Test the API
curl -X POST http://localhost:8080/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "{ hello }"}'
```

## Docker Compose Usage

The repository includes production-ready Docker Compose files:

### Development Setup

```bash
docker-compose -f docker-compose.dev.yml up -d
```

Features:
- Hot reload for code changes
- Debug ports exposed
- Development configuration with relaxed limits
- Redis for caching (optional)

### Production Setup

```bash
docker-compose -f docker-compose.yml up -d
```

Features:
- Multi-stage build for smaller image
- Production configuration with strict defaults
- Health checks for orchestration
- Redis dependency with health checks
- Port 8080 (production standard)

### Custom Configuration

Create a custom `docker-compose.override.yml` file:

```yaml
version: '3'
services:
  app:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - GraphQL__MaxQueryComplexity=10000
      - GraphQL__EnableCaching=true
    ports:
      - "8080:8080"
    volumes:
      - ./appsettings.Production.json:/app/appsettings.Production.json
```

Then run:
```bash
docker-compose up -d
```

## Environment Variables Reference

The engine supports configuration via environment variables. All options can be set using the `GraphQL__` prefix.

### Core Configuration

| Environment Variable | Type | Default | Description |
|--------------------|------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | string | Production | ASP.NET Core environment |
| `GraphQL__ServiceName` | string | null | Name of this GraphQL service (for federation) |
| `GraphQL__EnableFederation` | bool | false | Enable GraphQL Federation support |
| `GraphQL__EnableSchemaStitching` | bool | true | Enable schema stitching |
| `GraphQL__EnableSubscriptions` | bool | true | Enable WebSocket subscriptions |
| `GraphQL__EnableCaching` | bool | true | Enable query result caching |

### Complexity & Performance

| Environment Variable | Type | Default | Description |
|--------------------|------|---------|-------------|
| `GraphQL__MaxQueryComplexity` | int | 5000 | Maximum allowed query complexity |
| `GraphQL__MaxQueryDepth` | int | 15 | Maximum query nesting depth |
| `GraphQL__MaxQueryFields` | int | 200 | Maximum fields per query |
| `GraphQL__QueryTimeoutMs` | int | 30000 | Query execution timeout (ms) |
| `GraphQL__CacheTtlSeconds` | int | 300 | Query result cache TTL (seconds) |
| `GraphQL__CacheMaxSizeBytes` | int | 104857600 | Query result cache max size (bytes) |

### Federation Configuration

| Environment Variable | Type | Default | Description |
|--------------------|------|---------|-------------|
| `GraphQL__EnableFederation` | bool | false | Enable GraphQL Federation |
| `GraphQL__FederationDiscoveryEndpoint` | string | `/.well-known/federation` | Federation discovery endpoint |
| `GraphQL__FederationTimeout` | int | 30000 | Federation service timeout (ms) |
| `GraphQL__EntityCacheTtlSeconds` | int | 300 | Entity cache TTL (seconds) |
| `GraphQL__EntityCacheMaxSize` | int | 10000 | Entity cache max size |

### Schema Stitching Configuration

| Environment Variable | Type | Default | Description |
|--------------------|------|---------|-------------|
| `GraphQL__EnableSchemaStitching` | bool | true | Enable schema stitching |
| `GraphQL__RemoteSchemaTimeout` | int | 30000 | Remote schema timeout (ms) |
| `GraphQL__StitchingBaseUrl` | string | null | Base URL for schema stitching |

### Caching Configuration

| Environment Variable | Type | Default | Description |
|--------------------|------|---------|-------------|
| `GraphQL__EnableCaching` | bool | true | Enable caching |
| `GraphQL__CacheTtlSeconds` | int | 300 | Cache TTL (seconds) |
| `GraphQL__CacheMaxSizeBytes` | int | 104857600 | Max cache size (100 MB) |

### Subscription Configuration

| Environment Variable | Type | Default | Description |
|--------------------|------|---------|-------------|
| `GraphQL__EnableSubscriptions` | bool | true | Enable subscriptions |
| `GraphQL__SubscriptionKeepAliveInterval` | int | 30000 | Keep-alive interval (ms) |

### Error Handling

| Environment Variable | Type | Default | Description |
|--------------------|------|---------|-------------|
| `GraphQL__IncludeDetailedErrorMessages` | bool | false | Include detailed error messages |
| `GraphQL__LogInternalErrors` | bool | true | Log internal errors |

### Example: Production Configuration

```bash
export ASPNETCORE_ENVIRONMENT=Production
export GraphQL__MaxQueryComplexity=10000
export GraphQL__MaxQueryDepth=20
export GraphQL__EnableCaching=true
export GraphQL__CacheTtlSeconds=600
export GraphQL__CacheMaxSizeBytes=209715200
```

Then run:
```bash
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e GraphQL__MaxQueryComplexity=10000 \
  -e GraphQL__EnableCaching=true \
  --name graphql-engine \
  vladyslavzaiets/dotnet-graphql-engine:latest
```

## Production Deployment Checklist

### ✅ Before Deploying to Production

- [ ] **Port Configuration**: Ensure port 8080 is exposed and accessible
- [ ] **Health Checks**: Verify `/health` endpoint returns 200 OK
- [ ] **Resource Limits**: Set appropriate CPU and memory limits
- [ ] **Logging**: Configure centralized logging (ELK, Datadog, etc.)
- [ ] **Monitoring**: Set up Prometheus/Grafana or similar
- [ ] **Security**: Configure HTTPS/TLS termination
- [ ] **Authentication**: Set up authentication middleware if needed
- [ ] **Rate Limiting**: Configure rate limiting to prevent abuse
- [ ] **Backup**: Set up database backups if using external storage
- [ ] **Scaling**: Configure auto-scaling rules


### 📋 Deployment Steps

1. **Build the image** (or pull from registry):
```bash
docker build -t dotnet-graphql-engine:production .
# OR
docker pull vladyslavzaiets/dotnet-graphql-engine:latest
```

2. **Tag and push** (if using private registry):
```bash
docker tag dotnet-graphql-engine:production registry.example.com/graphql-engine:2.0
docker push registry.example.com/graphql-engine:2.0
```

3. **Deploy** using your orchestrator:

**Docker Swarm:**
```bash
docker stack deploy -c docker-compose.yml graphql
```

**Kubernetes:**
```bash
kubectl apply -f k8s-deployment.yaml
kubectl apply -f k8s-service.yaml
```

**Nomad:**
```hcl
job "graphql-engine" {
  datacenters = ["dc1"]
  group "graphql" {
    count = 3
    
    network {
      mode = "host"
      port "http" {
        to = 8080
      }
    }
    
    service {
      name = "graphql-engine"
      port = "http"
      check {
        type = "http"
        path = "/health"
        interval = "30s"
        timeout = "5s"
      }
    }
    
    task "app" {
      driver = "docker"
      config {
        image = "vladyslavzaiets/dotnet-graphql-engine:latest"
        ports = ["http"]
      }
      env {
        ASPNETCORE_ENVIRONMENT = "Production"
        GraphQL__MaxQueryComplexity = "10000"
      }
    }
  }
}
```

4. **Verify deployment**:
```bash
# Check container status
docker ps | grep graphql-engine

# Test health check
curl -i http://localhost:8080/health

# Test GraphQL endpoint
curl -X POST http://localhost:8080/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "{ __typename }"}'
```

5. **Configure monitoring**:
- Set up Prometheus metrics endpoint (`/metrics`)
- Configure Grafana dashboard
- Set up alerts for:
  - High latency
  - Error rates
  - Memory usage
  - Container restarts

### 🔧 Post-Deployment Configuration

**Enable HTTPS:**
```bash
# Use a reverse proxy like nginx or Traefik
# Example nginx configuration:

server {
  listen 443 ssl;
  server_name graphql.example.com;

  ssl_certificate /path/to/cert.pem;
  ssl_certificate_key /path/to/key.pem;

  location / {
    proxy_pass http://localhost:8080;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
  }
}
```

**Configure Authentication:**
```csharp
// In Program.cs
app.UseAuthentication();
app.UseAuthorization();

// Configure your auth provider
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options => {
    options.Authority = "https://auth.example.com";
    options.Audience = "graphql-engine";
  });
```

**Set up Rate Limiting:**
```csharp
// In Program.cs
builder.Services.AddRateLimiting(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

## Advanced Configuration

### Custom Dockerfile

Create a custom `Dockerfile` in your project:

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "dotnet-graphql-engine.csproj"
RUN dotnet build "dotnet-graphql-engine.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "dotnet-graphql-engine.csproj" -c Release -o /app/publish \
    --no-restore \
    --no-build

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dotnet-graphql-engine.dll"]
```

### Multi-Container Setup with Federation

**docker-compose.yml:**
```yaml
version: '3'
services:
  users-service:
    build:
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - GraphQL__ServiceName=UsersService
      - GraphQL__EnableFederation=true
    ports:
      - "8081:8080"
    depends_on:
      - redis
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  posts-service:
    build:
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - GraphQL__ServiceName=PostsService
      - GraphQL__EnableFederation=true
    ports:
      - "8082:8080"
    depends_on:
      - redis
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  gateway:
    build:
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - GraphQL__EnableFederation=true
      - GraphQL__EnableSchemaComposition=true
      - GraphQL__CompositionTimeout=30000
    ports:
      - "8080:8080"
    depends_on:
      users-service:
        condition: service_healthy
      posts-service:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  redis:
    image: redis:alpine
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5
```

### Using Environment Files

Create `.env` file:
```
ASPNETCORE_ENVIRONMENT=Production
GraphQL__MaxQueryComplexity=10000
GraphQL__EnableCaching=true
GraphQL__CacheTtlSeconds=600
```

Then run:
```bash
docker-compose --env-file .env up -d
```

### Custom Entrypoint

For custom initialization:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

# Add custom initialization script
COPY scripts/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
CMD ["dotnet", "dotnet-graphql-engine.dll"]
```

**scripts/entrypoint.sh:**
```bash
#!/bin/sh
set -e

# Run migrations if needed
if [ -f "/app/migrations/run.sh" ]; then
  /app/migrations/run.sh
fi

# Start the application
exec "$@"
```

## Troubleshooting

### Common Issues

#### ❌ Container fails to start

**Symptoms:** Container exits immediately with error

**Solutions:**
```bash
# Check logs
docker logs <container-name>

# Common fixes:
- Ensure port 8080 is available
- Check environment variables are correct
- Verify Docker image was built correctly
```

#### ❌ Health check fails

**Symptoms:** `/health` endpoint returns non-200 status

**Solutions:**
```bash
# Check health check configuration
curl -i http://localhost:8080/health

# Increase start period if services need more time
# Edit docker-compose.yml healthcheck section
```

#### ❌ Port already in use

**Symptoms:** `docker: Error response from daemon: Ports are not available`

**Solutions:**
```bash
# Change host port mapping
docker run -p 8081:8080 ...

# Or find and kill the process using the port
lsof -i :8080
kill -9 <PID>
```

#### ❌ High memory usage

**Symptoms:** Container OOM killed or high memory usage

**Solutions:**
```bash
# Set memory limits
docker run -m 1g ...

# Reduce cache size
# Set GraphQL__CacheMaxSizeBytes=52428800 (50MB)
```

#### ❌ Slow queries

**Symptoms:** Queries taking longer than expected

**Solutions:**
```bash
# Increase timeout
# Set GraphQL__QueryTimeoutMs=60000 (60 seconds)

# Enable caching
# Set GraphQL__EnableCaching=true

# Check query complexity
curl -X POST http://localhost:8080/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "query { __schema { types { name } } }"}'
```

### Debugging Tips

**Enable debug logging:**
```bash
docker run -e ASPNETCORE_ENVIRONMENT=Development -e DOTNET_ROOT=/app ...
```

**Inspect container:**
```bash
docker exec -it <container-name> sh
# Then explore the filesystem
```

**Check network connectivity:**
```bash
# From inside container
apk add curl  # Alpine
apt-get update && apt-get install -y curl  # Debian

curl -v http://<service-name>:8080/health
```

## Best Practices

### 📦 Image Optimization

1. **Use multi-stage builds** (already configured in the repository)
2. **Minimize layers** - Combine related RUN commands
3. **Use .dockerignore** - Exclude unnecessary files
4. **Pin base image versions** - Avoid `latest` tags

### 🔒 Security

1. **Run as non-root user:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN adduser -D appuser
USER appuser
```

2. **Scan for vulnerabilities:**
```bash
docker scan vladyslavzaiets/dotnet-graphql-engine:latest
```

3. **Use secrets for sensitive data:**
```yaml
# docker-compose.yml
services:
  app:
    secrets:
      - db_password

secrets:
  db_password:
    file: ./secrets/db_password.txt
```

### 🚀 Performance

1. **Set resource limits:**
```yaml
services:
  app:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

2. **Enable caching:**
```bash
export GraphQL__EnableCaching=true
export GraphQL__CacheTtlSeconds=300
```

3. **Configure connection pooling:**
```csharp
// In your services
services.AddHttpClient("FederationClient")
  .SetHandlerLifetime(TimeSpan.FromMinutes(5));
```

### 📊 Monitoring

1. **Expose metrics endpoint:**
```csharp
// In Program.cs
builder.Services.AddMetrics();
app.MapMetrics();
```

2. **Configure health checks:**
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 30s
```

3. **Set up centralized logging:**
```bash
# Use EFK stack (Elasticsearch, Fluentd, Kibana)
# Or send logs to your logging provider
```

### 🔄 Updates

1. **Use version tags:**
```bash
docker pull vladyslavzaiets/dotnet-graphql-engine:2.0.1
```

2. **Test updates in staging:**
```bash
# Deploy to staging first
# Run integration tests
# Monitor for issues
# Then deploy to production
```

3. **Rollback strategy:**
```bash
# Keep previous version available
docker tag vladyslavzaiets/dotnet-graphql-engine:2.0.0 vladyslavzaiets/dotnet-graphql-engine:previous

# Quick rollback
kubectl rollout undo deployment/graphql-engine
```

## Examples

### Example 1: Simple Production Deployment

**docker-compose.yml:**
```yaml
version: '3'
services:
  graphql-engine:
    image: vladyslavzaiets/dotnet-graphql-engine:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - GraphQL__MaxQueryComplexity=10000
      - GraphQL__EnableCaching=true
    ports:
      - "8080:8080"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

### Example 2: Development with Hot Reload

**docker-compose.dev.yml:**
```yaml
version: '3'
services:
  app:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - GraphQL__MaxQueryComplexity=5000
    ports:
      - "8080:8080"
      - "5678:5678"  # Debug port
    volumes:
      - .:/app
      - ~/.nuget:/root/.nuget
    command: dotnet watch run --no-launch-profile
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

### Example 3: Kubernetes Deployment

**k8s-deployment.yaml:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: graphql-engine
spec:
  replicas: 3
  selector:
    matchLabels:
      app: graphql-engine
  template:
    metadata:
      labels:
        app: graphql-engine
    spec:
      containers:
      - name: graphql-engine
        image: vladyslavzaiets/dotnet-graphql-engine:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: GraphQL__MaxQueryComplexity
          value: "10000"
        - name: GraphQL__EnableCaching
          value: "true"
        resources:
          requests:
            cpu: "500m"
            memory: "512Mi"
          limits:
            cpu: "1000m"
            memory: "1Gi"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: graphql-engine
spec:
  selector:
    app: graphql-engine
  ports:
    - protocol: TCP
      port: 80
      targetPort: 8080
  type: ClusterIP
```

## Resources

- [Main Documentation](../README.md)
- [Getting Started Guide](../docs/getting-started.md)
- [Configuration Reference](../docs/api-reference.md)
- [Federation Guide](../docs/architecture.md#federation)
- [Troubleshooting Guide](../docs/faq.md)
