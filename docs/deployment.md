# Deployment Guide

Production deployment strategies for dotnet-graphql-engine.

## Prerequisites

- .NET 10 runtime
- 1GB RAM minimum (2GB recommended)
- Docker (for containerized deployment)
- Kubernetes (optional, for orchestration)

## Standalone Deployment

### 1. Publish Release Build

```bash
dotnet publish -c Release -o ./publish
```

### 2. Configure Environment

Create `appsettings.Production.json`:

```json
{
  "GraphQL": {
    "MaxQueryComplexity": 5000,
    "MaxQueryDepth": 10,
    "QueryTimeoutMs": 30000,
    "EnableCaching": true,
    "CacheTtlSeconds": 300,
    "CacheMaxSizeBytes": 52428800,
    "EnableSubscriptions": true,
    "IncludeDetailedErrorMessages": false,
    "LogLevel": "Information"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### 3. Run Server

```bash
cd publish
ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://0.0.0.0:5000 \
  dotnet dotnet-graphql-engine.dll
```

## Docker Deployment

### Single Container

**Build Image:**

```bash
docker build -t myregistry/graphql-engine:1.0.0 .
```

**Run Container:**

```bash
docker run -d \
  --name graphql-engine \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  myregistry/graphql-engine:1.0.0
```

### Docker Compose

Entire stack with dependencies:

```bash
docker-compose up -d
```

## Kubernetes Deployment

### 1. Create Namespace

```bash
kubectl create namespace graphql-engine
```

### 2. Create ConfigMap for Settings

```bash
kubectl create configmap graphql-config \
  --from-file=appsettings.Production.json \
  -n graphql-engine
```

### 3. Deploy Application

Create `k8s-deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: graphql-engine
  namespace: graphql-engine
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
        image: myregistry/graphql-engine:1.0.0
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        resources:
          requests:
            memory: "1Gi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
```

Deploy:

```bash
kubectl apply -f k8s-deployment.yaml
```

### 4. Create Service

```yaml
apiVersion: v1
kind: Service
metadata:
  name: graphql-engine-service
  namespace: graphql-engine
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 5000
  selector:
    app: graphql-engine
```

### 5. Monitor Deployment

```bash
kubectl get pods -n graphql-engine
kubectl logs -f deployment/graphql-engine -n graphql-engine
```

## Cloud Deployment

### Azure App Service

```bash
# Login to Azure
az login

# Create resource group
az group create --name myResourceGroup --location eastus

# Create App Service plan
az appservice plan create \
  --name myAppServicePlan \
  --resource-group myResourceGroup \
  --sku B2

# Create web app
az webapp create \
  --resource-group myResourceGroup \
  --plan myAppServicePlan \
  --name myGraphQLEngine

# Deploy
dotnet publish -c Release
az webapp deployment user set --user-name <deployment-user> --password <password>
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
az webapp deployment source config-zip \
  --resource-group myResourceGroup \
  --name myGraphQLEngine \
  --src ../app.zip
```

### AWS Elastic Beanstalk

```bash
# Install AWS CLI
pip install awsebcli

# Initialize EB application
eb init -p "Docker running on 64bit Amazon Linux 2" graphql-engine

# Create environment
eb create graphql-engine-env

# Deploy
eb deploy

# View logs
eb logs
```

### Google Cloud Run

```bash
# Authenticate with Google Cloud
gcloud auth login
gcloud config set project YOUR_PROJECT_ID

# Build and push image
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/graphql-engine

# Deploy to Cloud Run
gcloud run deploy graphql-engine \
  --image gcr.io/YOUR_PROJECT_ID/graphql-engine \
  --platform managed \
  --region us-central1 \
  --memory 2Gi \
  --cpu 2 \
  --allow-unauthenticated
```

## SSL/TLS Configuration

### Self-Signed Certificate (Development)

```bash
# Generate certificate
dotnet dev-certs https --clean
dotnet dev-certs https

# Export certificate
dotnet dev-certs https -ep certificate.pfx -p <password>
```

### Production Certificate

Use Let's Encrypt with Certbot:

```bash
# Install Certbot
sudo apt-get install certbot python3-certbot-nginx

# Generate certificate
sudo certbot certonly --standalone -d yourdomain.com

# Configure in appsettings
dotnet user-secrets set "Kestrel:Certificates:Default:Path" "/etc/letsencrypt/live/yourdomain.com/fullchain.pem"
dotnet user-secrets set "Kestrel:Certificates:Default:KeyPath" "/etc/letsencrypt/live/yourdomain.com/privkey.pem"
```

## Reverse Proxy Configuration

### Nginx

```nginx
upstream graphql_backend {
    server localhost:5000;
}

server {
    listen 80;
    server_name yourdomain.com;

    location / {
        proxy_pass http://graphql_backend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # WebSocket support
        proxy_read_timeout 86400;
    }
}
```

### Apache

```apache
ProxyPreserveHost On
ProxyPass / http://localhost:5000/
ProxyPassReverse / http://localhost:5000/

# WebSocket support
RewriteEngine On
RewriteCond %{HTTP:Upgrade} websocket [NC]
RewriteCond %{HTTP:Connection} upgrade [NC]
RewriteRule ^/?(.*) "ws://localhost:5000/$1" [P,L]
```

## Performance Tuning

### Memory Management

```csharp
// In appsettings.Production.json
{
  "GraphQL": {
    "CacheMaxSizeBytes": 104857600,  // 100 MB
    "CacheTtlSeconds": 600,           // 10 minutes
    "MaxBatchSize": 200
  }
}
```

### Connection Pooling

```csharp
services.AddHttpClient<ExternalApiIntegration>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigureHttpMessageHandlerBuilder(builder =>
    {
        builder.PrimaryHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        };
    });
```

### Query Complexity

Set appropriate limits based on load:

```csharp
options.MaxQueryComplexity = 5000;  // Adjust based on your hardware
options.MaxQueryDepth = 10;
options.QueryTimeoutMs = 30000;
```

## Monitoring & Logging

### Application Insights (Azure)

```csharp
services.AddApplicationInsightsTelemetry();
```

### ELK Stack (Elasticsearch, Logstash, Kibana)

Configure structured logging:

```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
    // Add ELK configuration
});
```

### Health Checks

```bash
# Check service health
curl http://localhost:5000/health
```

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2026-05-04T10:30:00Z",
  "components": {
    "cache": "Healthy",
    "database": "Healthy"
  }
}
```

## Backup & Recovery

### Database Backups

Implement regular backups of your data store:

```bash
# PostgreSQL example
pg_dump -h localhost -U user database_name | gzip > backup.sql.gz

# Restore
gunzip < backup.sql.gz | psql -h localhost -U user database_name
```

### Configuration Backups

```bash
# Backup appsettings
tar czf config-backup-$(date +%Y%m%d).tar.gz appsettings*.json

# Backup database
pg_dump mydb > mydb-$(date +%Y%m%d).sql
```

## Scaling

### Horizontal Scaling

Run multiple instances behind a load balancer:

```
┌─────────────┐
│ Load        │
│ Balancer    │
└─────────────┘
      │
    ┌─┴──┬──────┬──────┐
    │    │      │      │
┌───▼┐ ┌─▼──┐ ┌─▼──┐ ┌─▼──┐
│App1│ │App2│ │App3│ │App4│
└────┘ └────┘ └────┘ └────┘
```

### Vertical Scaling

Increase server resources:
- RAM: 1GB → 2GB → 4GB
- CPU: Single core → Multi-core
- Storage: SSD for cache

## Security Considerations

### Environment Variables

Never commit secrets to version control:

```bash
# Use environment variables
export GRAPHQL__ENABLEMETRICS=true
export CONNECTIONSTRINGS__DEFAULT=...

# Or use user secrets
dotnet user-secrets set "GraphQL:MaxQueryComplexity" "5000"
```

### Rate Limiting

Enable rate limiting middleware:

```csharp
services.AddRateLimiting(options =>
{
    options.GlobalLimitPolicy = "default";
    options.AddPolicy("default", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

### Authentication

Implement proper authentication:

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-auth-provider.com";
        options.Audience = "your-api";
    });
```

## Troubleshooting

### Port Already in Use

```bash
# Find process using port
lsof -i :5000

# Kill process
kill -9 <PID>

# Or use different port
dotnet run --urls http://localhost:5001
```

### High Memory Usage

```bash
# Check memory usage
ps aux | grep dotnet

# Adjust cache size
options.CacheMaxSizeBytes = 26214400;  // 25 MB instead of 50 MB
```

### Slow Query Performance

```bash
# Enable query logging
options.EnableMetrics = true;

# Reduce complexity limits
options.MaxQueryComplexity = 3000;
```

## Checklist

- [ ] Environment variables configured
- [ ] Secrets managed securely
- [ ] SSL/TLS configured
- [ ] Logging configured
- [ ] Health checks enabled
- [ ] Rate limiting configured
- [ ] Backup strategy implemented
- [ ] Monitoring tools configured
- [ ] Load testing completed
- [ ] Security audit completed
- [ ] Documentation updated
- [ ] Runbooks created
