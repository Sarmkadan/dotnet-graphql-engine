# CLAUDE.md

Self-contained, code-first GraphQL engine for .NET 8 (parser, execution, complexity analysis, DataLoader, caching, persisted queries, subscriptions) with no dependency on ASP.NET Core or any GraphQL library. Packed as NuGet `Zaiets.dotnet.graphql.engine`; `Program.cs` doubles as a console demo.

## Build

```bash
dotnet restore
dotnet build -c Release            # or: make build
./build.sh                         # restore + build + test (used by task-factory)
dotnet run -c Release              # run the console demo
dotnet publish -c Release -o ./publish
```

Solution file is `dotnet-graphql-engine.slnx` (main project + tests). Main csproj compiles `**/*.cs` explicitly, excluding `benchmarks/`, `examples/`, `tests/`.

## Test

```bash
dotnet test -c Release --no-build              # or: make test
dotnet test --filter "FullyQualifiedName~QueryComplexityTests"
```

- xUnit 2.9 + FluentAssertions 8 + Moq, project `tests/dotnet-graphql-engine.Tests` (net10.0, references main csproj).
- CI (`.github/workflows/ci.yml`) runs restore/build/test on .NET 8.0.x and 10.0.x matrix.

## Lint / Format

```bash
dotnet format                                   # make format
dotnet build -c Release -warnaserror            # make lint
dotnet list package --vulnerable                # make security-scan
```

Style rules live in `.editorconfig` (4-space indent, braces required, `var` only when type is apparent, nullable enabled).

## Key directories

- `Program.cs` - entry point / demo: builds `ServiceCollection`, calls `AddGraphQLEngine(...)`, executes sample queries.
- `src/Configuration/` - `GraphQLEngineOptions`, `DependencyInjection.AddGraphQLEngine` (DI entry point), option validators.
- `src/Services/GraphQL/GraphQLExecutionService.cs` - the engine core: hand-rolled tokenizer/parser, resolver registry, `ExecuteAsync`.
- `src/Services/` - `Schema`, `QueryAnalysis` (complexity/depth), `DataLoader`, `Caching` (`ICacheStore`, LRU, distributed), `Subscriptions`, `Events`, `BackgroundServices`.
- `src/Domain/Entities` - `GraphQLSchema`, `GraphQLQuery`, `QueryField`, `ExecutionContext` (shadows `System.Threading.ExecutionContext` - alias it), etc.
- `src/Api/` - framework-free "controllers" and HTTP-shaped middleware (auth, rate limiting, logging, error handling).
- `src/Common/Utilities` - extension/helper classes; `src/Exceptions` - `GraphQLException`; `src/Formatters` - JSON/CSV/schema docs output.
- `benchmarks/` - BenchmarkDotNet project; `examples/` - runnable samples; `docs/ARCHITECTURE.md` - full design description.

## Conventions

- Root namespace `GraphQLEngine`, folders map to namespaces (`GraphQLEngine.Services.GraphQL`, `GraphQLEngine.Domain.Entities`). Tests use `GraphQLEngine.Tests.*`.
- Every file starts with `#nullable enable` and the author header comment block.
- Companion-file pattern: `Foo.cs` + `FooExtensions.cs` + `FooJsonExtensions.cs` + `FooValidation.cs` (static classes for extension methods, JSON (de)serialization, and validation).
- Test classes are `sealed public class FooTests`, one file per source class; method names `Method_WhenCondition_ExpectedResult`. Use FluentAssertions, mock `ILogger<T>` with Moq.
- Options are validated via dedicated `*Validation` classes; services registered through `AddGraphQLEngine` only.
- Commits follow Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`).
- Do not commit `bin/`, `obj/`, `.aider*` files.
