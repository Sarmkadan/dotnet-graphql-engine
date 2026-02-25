# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Multi-stage build for dotnet-graphql-engine v2.0

# ---------------------------------------------------------------------------
# Stage 1: Restore & Build
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

# Copy project files first for layer caching
COPY ["dotnet-graphql-engine.csproj", "./"]
COPY ["dotnet-graphql-engine.slnx", "./"]
COPY ["tests/dotnet-graphql-engine.Tests/dotnet-graphql-engine.Tests.csproj", "./tests/dotnet-graphql-engine.Tests/"]
RUN dotnet restore "dotnet-graphql-engine.csproj"

# Copy everything and build
COPY . .
RUN dotnet build -c Release --no-restore

# Run tests
RUN dotnet test tests/dotnet-graphql-engine.Tests \
    -c Release \
    --no-build \
    --logger "console;verbosity=minimal"

# ---------------------------------------------------------------------------
# Stage 2: Publish
# ---------------------------------------------------------------------------
FROM builder AS publish
RUN dotnet publish "dotnet-graphql-engine.csproj" \
    -c Release \
    --no-build \
    -o /app/publish \
    /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Stage 3: Runtime
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=publish /app/publish .

# Create non-root user
RUN useradd -m -u 1001 graphql \
    && chown -R graphql:graphql /app
USER graphql

# Port configuration - v2.0 uses 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Metadata
LABEL maintainer="Vladyslav Zaiets <https://sarmkadan.com>"
LABEL description="dotnet-graphql-engine - Code-first GraphQL server for .NET"
LABEL version="2.0.0"

ENTRYPOINT ["dotnet", "dotnet-graphql-engine.dll"]
