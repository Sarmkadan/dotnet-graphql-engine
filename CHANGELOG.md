# Changelog

All notable changes to dotnet-graphql-engine are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-03-16
### Fixed
- Fix N+1 query in nested resolver when DataLoader batch is not flushed
- Added regression test for the fix

## [2.0.1]
### Security
- Added input validation and length limits
- Added request timeout configuration
- Added security policy and vulnerability reporting

## [2.0.0] - 2026-03-15

### Added
- Add federation support with entity resolution and schema composition
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

## [1.0.0] - 2025-10-27

### Added
- **Persisted Queries** - Store and execute queries by hash; reduces bandwidth and enables server-side query allowlisting
- **PersistedQueryService** - Hash-based query store with retrieval and registration APIs
- **Schema Documentation Formatter** - Export schema as human-readable HTML or Markdown documentation
- **CSV Output Formatter** - Stream flat query results as CSV for analytics pipelines

### Changed
- Promoted all beta APIs to stable; no further breaking changes planned for this major line
- Improved XML doc coverage on all public types for IntelliSense completeness

### Fixed
- Persisted query lookup returned stale entry after schema reload
- CSV formatter emitted extra trailing newline on empty result sets

## [0.9.0] - 2025-09-15

### Added
- **Rate Limiting Middleware** - Per-IP and per-token request throttling with configurable windows
- **CLI Argument Parser** - Override `appsettings.json` configuration via command-line flags at startup
- **ReflectionHelper** - Utility for scanning types and resolvers at startup to auto-register fields
- **EnumHelper** - Bi-directional enum ↔ string conversion used in type-kind serialisation

### Changed
- `GraphQLEngineOptions` now exposes four preset profiles: `Default`, `Strict`, `Permissive`, `HighPerformance`
- Logging middleware writes structured JSON entries with correlation IDs

### Fixed
- Race condition in `SubscriptionService` when two clients unsubscribed simultaneously
- Incorrect HTTP 200 returned on partial errors; now returns 200 with `errors` array per spec

### Performance
- Hot-path schema lookup now O(1) via pre-built dictionary instead of linear scan

## [0.8.0] - 2025-08-04

### Added
- **Schema Stitching** - Fetch and merge a remote GraphQL schema at startup via `SchemaStitchingConfig`
- **ExternalApiIntegration** - Typed HTTP client for calling stitched upstream schemas with retry and timeout
- **WebhookHandler** - Inbound webhook receiver that publishes events to the internal `EventBus`
- **HttpClientFactory** - Centralised factory with connection pooling for all outbound HTTP calls

### Changed
- `SchemaService.StitchSchemaAsync` now validates field type compatibility before merging
- `SubscriptionConfig` value object updated with `HeartbeatIntervalSeconds` field

### Fixed
- Schema stitching silently ignored remote types whose names clashed with local types
- `HttpClientFactory` leaked sockets when remote endpoint was unreachable

## [0.7.0] - 2025-06-23

### Added
- **Real-Time Subscriptions** - WebSocket-based subscriptions with `SubscriptionService` and `SubscriptionConnection`
- **EventBus** - In-process pub/sub; subscribers receive typed payloads via `Func<object, Task>` handlers
- **EventPublisher** - Thin wrapper over `EventBus` for DI-friendly injection into domain services
- **HealthCheckBackgroundService** - Periodic self-check that writes status to the `/health` endpoint
- **CacheMaintenanceBackgroundService** - Evicts expired LRU entries on a configurable schedule

### Changed
- `GraphQLController` now upgrades WebSocket connections for subscription queries automatically
- `ErrorHandlingMiddleware` catches `GraphQLException` and returns structured `errors` JSON

### Fixed
- Subscription handler leaked memory when client disconnected without sending `connection_terminate`
- Background services did not observe `CancellationToken` on host shutdown

## [0.6.0] - 2025-05-12

### Added
- **DistributedCacheService** - Pluggable cache backend with in-memory default and `IDistributedCache` adapter
- **CacheKeyBuilder** - Deterministic cache key construction from query string and variable map
- **CacheService** (GraphQL layer) - LRU query result cache with per-entry TTL and max-size eviction
- **QueryAnalysisService** - Depth-first traversal computing `TotalComplexity`, `MaxDepth`, and `FieldCount`
- `QueryComplexity` entity with `ComplexityLevel` enum (`LOW`, `MEDIUM`, `HIGH`, `CRITICAL`)

### Changed
- `GraphQLExecutionService` now checks complexity before executing and short-circuits with `GraphQLException` on breach
- `GraphQLEngineOptions` adds `EnableCaching`, `CacheTtlSeconds`, and `CacheMaxSizeBytes` fields

### Fixed
- Query result was cached even when the execution produced errors
- Complexity analyser double-counted inline fragments

## [0.5.0] - 2025-04-07

### Added
- **DataLoaderService** - Batch-loading registry; resolvers call `LoadAsync` per key, batches are flushed automatically
- `DataLoaderRequest` entity tracking batch-function name, key, and result promise
- **CollectionExtensions** - `Chunk`, `DistinctBy`, and `ToHashSet` helpers for batch grouping logic
- **TypeConverter** - Safe `Convert.ChangeType` wrapper used by DataLoader result mapping

### Changed
- `GraphQLExecutionService` flushes all registered DataLoader batches before returning execution context
- `InMemoryRepository` upgraded to `ConcurrentDictionary` for thread-safe batch inserts

### Fixed
- DataLoader batch was flushed before all `LoadAsync` calls from a single resolver were enqueued
- Duplicate keys in the same batch produced duplicate SQL parameters in downstream query

## [0.4.0] - 2025-03-10

### Added
- **Authentication Middleware** - Validates `Authorization: Bearer <token>` header; injects claims into `ExecutionContext`
- **ErrorFormattingService** - Converts internal exceptions to spec-compliant `{ message, locations, path, extensions }` objects
- **GraphQLController** - ASP.NET Core minimal-API controller exposing `POST /graphql`
- **SchemaController** - `GET /graphql/schema` returns SDL export of the active schema
- **HealthCheckController** - `GET /health` returns uptime, version, and service status

### Changed
- `ExecutionContext` now carries `Headers`, `User`, and `Data` dictionaries for use in resolvers
- `GraphQLEngineOptions` gains `IncludeDetailedErrorMessages` and `LogInternalErrors` flags

### Fixed
- Middleware pipeline swallowed `OperationCanceledException` without returning 408
- `SchemaController` returned 404 when schema name contained uppercase letters

## [0.3.0] - 2025-02-17

### Added
- **SchemaService** - Create named schemas, register types, export as SDL, and run introspection
- **GraphQLSchema** / **GraphQLType** / **GraphQLField** entities forming the core type graph
- **GraphQLQuery**, **GraphQLMutation**, **GraphQLSubscription** - Typed wrappers for operation documents
- **DependencyInjection** - `AddGraphQLEngine(options)` extension wiring all services into the DI container
- `GraphQLEngineOptions` with `MaxQueryComplexity`, `MaxQueryDepth`, `MaxQueryFields`, `QueryTimeoutMs`

### Changed
- `IRepository<T>` contract extended with `FindAsync(predicate)` for filtered reads
- `InMemoryRepository` stores entities in a `ConcurrentDictionary` keyed by string ID

### Fixed
- Schema type registry allowed duplicate type names without error

## [0.2.0] - 2025-01-27

### Added
- **GraphQLExecutionService** - Execute query documents against a registered schema; returns `ExecutionContext`
- **ExecutionContext** - Tracks `ExecutionId`, `StartTime`, `Errors`, `Variables`, and resolved `Data`
- **GraphQLException** - Domain exception carrying `Message`, `Code`, and optional `Extensions` dictionary
- **ValidationHelper** - Null-check, range, and email validators used across the service layer
- **StringExtensions** - `ToCamelCase`, `ToSnakeCase`, `Truncate`, `IsNullOrWhiteSpace` wrappers
- **DateTimeExtensions** - UTC normalisation and ISO-8601 formatting helpers
- **JsonHelper** - `Serialize` / `Deserialize` wrappers with consistent `JsonSerializerOptions`
- **LoggingMiddleware** - Logs request method, path, status code, and elapsed time

### Changed
- Project structure reorganised into `src/Api`, `src/Domain`, `src/Services`, `src/Data`, `src/Common`
- `.editorconfig` added enforcing 4-space indentation and UTF-8 for all C# files

### Fixed
- `ExecutionContext.Duration` returned negative value when `EndTime` was not set

## [0.1.0] - 2025-01-06

### Added
- Initial project scaffold targeting .NET 10
- `IRepository<T>` and `InMemoryRepository<T>` for generic in-memory data storage
- `GraphQLConstants` with standard HTTP header names and content-type values
- `PersistedQuery` entity for future persisted-query support
- Solution file (`dotnet-graphql-engine.slnx`) with main project and test project
- `Dockerfile` and `docker-compose.yml` for containerised local development
- `Makefile` with `build`, `test`, `run`, and `docker-build` targets
- `.github/workflows/build.yml` CI pipeline (restore → build → test)
- `.github/workflows/codeql.yml` weekly static analysis
- `.github/dependabot.yml` weekly NuGet and Actions update checks
- MIT `LICENSE`, `CODE_OF_CONDUCT.md`, `CONTRIBUTING.md`, `SECURITY.md`
- `appsettings.example.json` with annotated default configuration

---

## Contributors

- Vladyslav Zaiets - Creator & Maintainer
- Community contributors welcome — see CONTRIBUTING.md

## License

MIT License — See LICENSE file for details
