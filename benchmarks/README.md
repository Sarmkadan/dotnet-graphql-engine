# Performance Benchmarks

This directory contains performance benchmarks for the .NET GraphQL Engine using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## Running Benchmarks

### Prerequisites

- .NET 9.0 SDK or later
- Git (for cloning the repository)

### Running All Benchmarks

```bash
cd benchmarks/dotnet-graphql-engine.Benchmarks

# Run benchmarks and generate report
dotnet run -c Release -- --filter *

# Run benchmarks with memory diagnostics
dotnet run -c Release -- --filter * --memory
```

### Running Specific Benchmark Categories

```bash
# Run only query execution benchmarks
dotnet run -c Release -- --filter *Query*

# Run only schema operation benchmarks
dotnet run -c Release -- --filter *Schema*
```

### Exporting Results

To export results to a file:

```bash
dotnet run -c Release -- --join --memory --exporters json --output ./results
```

## Available Benchmarks

### Query Execution
- **SimpleQuery**: Measures performance of a simple single-field query
- **NestedQuery**: Measures performance of nested queries with relationships
- **ComplexQuery**: Measures performance of complex queries with multiple root fields
- **MultipleSimpleQueries**: Measures throughput of executing multiple queries

### Schema Operations
- **CreateSchema**: Measures schema creation performance

### Resolver Registration
- **RegisterResolver**: Measures resolver registration performance

## Configuration

Benchmarks are configured in `QueryExecutionBenchmarks.cs` with:
- Memory diagnostics enabled
- Ranking column to show relative performance
- Ordered from fastest to slowest

## Interpreting Results

BenchmarkDotNet provides detailed metrics including:
- **Mean**: Average execution time
- **Error**: Standard error
- **StdDev**: Standard deviation
- **Gen 0/1/2**: Garbage collection generations
- **Allocated**: Memory allocated per operation

## Adding New Benchmarks

To add new benchmarks:

1. Create a new benchmark class in the benchmarks project
2. Add `[MemoryDiagnoser]` and `[Benchmark]` attributes
3. Implement setup in a `[GlobalSetup]` method
4. Add cleanup in a `[GlobalCleanup]` method
5. Run benchmarks to verify they work correctly

## CI Integration

Benchmarks can be run in CI using:

```bash
cd benchmarks/dotnet-graphql-engine.Benchmarks
make benchmarks
```

See the main `Makefile` for additional benchmark commands.
