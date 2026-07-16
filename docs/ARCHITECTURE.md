# Architecture

This document describes how dotnet-graphql-engine is actually put together - the layout,
the moving parts, why they are shaped the way they are, and where the sharp edges live.

## Overview

dotnet-graphql-engine is a self-contained, code-first GraphQL engine for .NET 8. It has
**no dependency on ASP.NET Core or on any existing GraphQL library** - parsing, execution,
complexity analysis, batching, caching and subscriptions are all implemented inside this
repository on top of only `Microsoft.Extensions.*` (DI, Options, Logging, Configuration).

The repo builds a single project (`dotnet-graphql-engine.csproj`) that doubles as:

- a **library** (packed as `Zaiets.dotnet.graphql.engine`) - consumers call
  `services.AddGraphQLEngine(...)` and use the services directly;
- a **console demo** (`Program.cs`) that spins up a `ServiceCollection`, builds a sample
  schema and executes a few queries end to end.

Benchmarks (`benchmarks/`, BenchmarkDotNet), tests (`tests/`, xUnit + Moq) and runnable
examples (`examples/`) live next to it and are excluded from the main compile via the
explicit `<Compile Include>` glob in the csproj.

## Source layout

```
src/
  Api/            "Controllers" and HTTP-shaped middleware (framework-free, see below)
  Common/         Utility extensions, helpers, constants
  Configuration/  Options classes, validators, AddGraphQLEngine DI entry point
  Data/           IRepository<T> abstraction + InMemoryRepository<T>
  Domain/         Entities (GraphQLSchema, GraphQLQuery, ExecutionContext, ...) and value objects
  Exceptions/     GraphQLException + extension helpers
  Formatters/     JSON / CSV / schema-documentation output formatters
  Integration/    Outbound concerns: HttpClientFactory, webhooks, external API adapter
  Middleware/     QueryDepthLimiter (engine-level, not HTTP)
  Services/       The engine proper - execution, schema, analysis, dataloader,
                  subscriptions, caching, persisted queries, events, background services
```

## Component breakdown

### Domain (`src/Domain`)

Plain entities with no behaviorally-loaded base classes: `GraphQLSchema`, `GraphQLType`,
`GraphQLField`, `GraphQLQuery`, `GraphQLMutation`, `GraphQLSubscription`, `QueryComplexity`,
`PersistedQuery`, `DataLoaderRequest` and `ExecutionContext` (note: this shadows
`System.Threading.ExecutionContext`, so files that need it alias it explicitly).
`ValueObjects/` holds config-shaped records such as `SubscriptionConfig` and
`SchemaStitchingConfig`.

### Execution pipeline (`src/Services/GraphQL`)

`GraphQLExecutionService` is the heart of the engine. It contains a hand-rolled
tokenizer/parser (`QueryTokenizer`, `QueryParser`, private nested classes) rather than a
full GraphQL grammar implementation. Resolvers are registered at runtime via
`RegisterResolver(fieldPath, resolver)` into a `Dictionary<string, object>` and invoked
during `ExecuteAsync(GraphQLQuery)`, which returns an `ExecutionContext` carrying results
and errors.

**Trade-off:** the parser covers the common query/mutation shape, not the full GraphQL
spec (no fragments-on-interfaces edge cases, directives are limited). That was a
deliberate scope cut - the goal is a dependency-free engine that is easy to read end to
end, not spec-completeness parity with Hot Chocolate.

Supporting services in the same folder:

- `CacheService` - per-engine in-memory query result cache (lock-guarded `Dictionary`),
  gated by `GraphQLEngineOptions.EnableCaching`.
- `ErrorFormattingService` - shapes `ExecutionError`s for client responses; detailed vs
  sanitized output is options-driven so production deployments do not leak internals.
- `PersistedQueryService` - implements the Automatic Persisted Queries (APQ) protocol:
  SHA-256 hash registration on first contact, hash-only requests afterwards. Uses a
  `ConcurrentDictionary` hash index for O(1) lookups with `IRepository<PersistedQuery>`
  as the durable source of truth; `AllowlistOnly` mode rejects ad-hoc documents.

### Schema (`src/Services/Schema`)

`SchemaService` builds and stores schemas (backed by `IRepository<GraphQLSchema>`).
Schema stitching is configured via a singleton `SchemaStitchingConfig` registered in DI.

### Query analysis (`src/Services/QueryAnalysis` + `src/Middleware`)

`QueryAnalysisService` computes a `QueryComplexity` score per query and keeps analyses
in-memory. `QueryDepthLimiter` (in `src/Middleware`) does a cheap textual depth check
(`Check(string query)` returning `DepthCheckResult`) so obviously-abusive queries can be
rejected *before* paying for parsing. Both limits default from `GraphQLConstants` and
are tunable via options (`MaxQueryComplexity`, `MaxQueryDepth`).

### DataLoader (`src/Services/DataLoader`)

`DataLoaderService` implements the batching/deduplication pattern to avoid N+1 resolver
fan-out. `GraphQLExecutionService` takes it as a constructor dependency, so batching is
available inside resolvers rather than bolted on afterwards.

### Subscriptions (`src/Services/Subscriptions`)

`SubscriptionService` manages `SubscriptionConnection`s and pushes `SubscriptionUpdate`s
through `AsyncEventHandler<T>`. Connection ceiling, timeout and heartbeat come from
`SubscriptionConfig`, which DI materializes from `GraphQLEngineOptions` - one options
object at the edge, purpose-built config objects inside.

### Caching (`src/Services/Caching`)

Separate from the query-result `CacheService`: `DistributedCacheService` exposes a
distributed-cache-shaped API (with `CacheStatistics`, TTL options) but is backed by an
in-memory store, and `CacheKeyBuilder` produces stable cache keys. The interface shape is
distributed-ready on purpose - swapping in Redis later means replacing one class, not
rewriting call sites.

### Events and background services (`src/Services/Events`, `src/Services/BackgroundServices`)

`EventBus`/`EventPublisher` provide in-process pub/sub with a bounded event log.
`CacheMaintenanceBackgroundService` and `HealthCheckBackgroundService` are periodic
workers (cache eviction sweeps, health snapshots).

### Data access (`src/Data`)

A single generic abstraction, `IRepository<T>`, with one implementation,
`InMemoryRepository<T>`, registered both as an open generic and as closed registrations
for the core entities. Everything stateful in the engine goes through this seam.

**Rationale:** the engine has no persistence opinion. Anyone embedding it can register
their own `IRepository<T>` (EF Core, Dapper, whatever) and the services will not notice.

### API layer (`src/Api`)

The `Controllers` (`GraphQLController`, `SchemaController`, `HealthCheckController`) and
`Middleware` (authentication, error handling, logging, rate limiting) are **plain classes,
not ASP.NET Core controllers/middleware** - there is no `Microsoft.AspNetCore` reference
in the project. They model the request/response shapes (`GraphQLHttpRequest`,
`GraphQLResponse`, `GraphQLError`) so that a host application can adapt them to whatever
HTTP framework it uses. Keeping the package framework-free was the driver; the cost is
that hosts must write the thin HTTP adapter themselves.

### Configuration (`src/Configuration`)

`DependencyInjection.AddGraphQLEngine(options => ...)` is the single composition root:
options + `IValidateOptions<>` validators, repositories, all engine services, subscription
and stitching config. `CreateTestServiceProvider` gives tests a ready-made container.

There are currently **two parallel options classes** - `GraphQLEngineOptions` and
`DotnetGraphqlEngineOptions` - with overlapping properties and separate validators. The
services consume `GraphQLEngineOptions`; the second class exists as a newer alternative
and both are registered/validated. This duplication is a known wart (see limitations).

## Data flow

```
client query (string or APQ hash)
    -> [PersistedQueryService]      resolve hash -> full document (optional)
    -> [QueryDepthLimiter]          cheap textual depth gate
    -> [QueryAnalysisService]       complexity scoring vs MaxQueryComplexity
    -> [GraphQLExecutionService]    tokenize -> parse -> resolve fields
           |-> registered resolvers (with DataLoaderService batching)
           |-> CacheService         result cache when EnableCaching
    -> [ErrorFormattingService]     shape errors (detailed vs sanitized)
    -> ExecutionContext             results + errors back to the caller
```

Subscriptions run beside this: `SubscriptionService` holds connections and the
`EventBus` fans events out to them.

## Extension points

- **Resolvers** - `GraphQLExecutionService.RegisterResolver(fieldPath, resolver)`.
- **Persistence** - register your own `IRepository<T>` before/after `AddGraphQLEngine`.
- **Options** - everything tunable goes through `GraphQLEngineOptions` and the
  `IOptions` pattern, validated at startup by `IValidateOptions<>` implementations.
- **HTTP hosting** - wrap the `src/Api` controllers with your framework of choice;
  they are transport-agnostic by design.
- **Output** - `Formatters/` (JSON, CSV, schema docs) for alternate response encodings.

## Known limitations

- **Not spec-complete.** The custom parser handles the mainstream query shape; exotic
  GraphQL constructs will not parse. Validate against your query corpus before adopting.
- **In-memory everything by default.** Repositories, caches and the persisted-query
  index all reset on process restart; multi-instance deployments need a real
  `IRepository<T>` and a real distributed cache behind `DistributedCacheService`.
- **Duplicate options classes.** `GraphQLEngineOptions` vs `DotnetGraphqlEngineOptions`
  should converge on one; until then prefer `GraphQLEngineOptions`, which is what the
  services actually read.
- **Generated companion files.** Many `*JsonExtensions.cs` / `*Validation.cs` files are
  mechanical per-type helpers; they inflate the file count but carry no architecture.
- **No transport included.** There is intentionally no HTTP server; the console
  `Program.cs` is a demo, not a host.
