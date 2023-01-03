# QueryExecutionBenchmarks
The `QueryExecutionBenchmarks` type is designed to provide a set of methods for benchmarking the execution of GraphQL queries. It allows developers to test the performance of various query types, including simple, nested, complex, and large queries, as well as introspection and multiple simple queries. This type also provides methods for setting up and cleaning up the benchmarking environment, creating a schema, and registering resolvers.

## API
The `QueryExecutionBenchmarks` type has the following public members:
* `Setup`: Sets up the benchmarking environment. This method does not take any parameters and does not return a value. It should be called before running any benchmarking tests.
* `Cleanup`: Cleans up the benchmarking environment. This method does not take any parameters and does not return a value. It should be called after running all benchmarking tests.
* `SimpleQuery`: Executes a simple GraphQL query. This method is asynchronous and does not take any parameters. It does not return a value.
* `NestedQuery`: Executes a nested GraphQL query. This method is asynchronous and does not take any parameters. It does not return a value.
* `ComplexQuery`: Executes a complex GraphQL query. This method is asynchronous and does not take any parameters. It does not return a value.
* `LargeQuery`: Executes a large GraphQL query. This method is asynchronous and does not take any parameters. It does not return a value.
* `IntrospectionQuery`: Executes an introspection GraphQL query. This method is asynchronous and does not take any parameters. It does not return a value.
* `MultipleSimpleQueries`: Executes multiple simple GraphQL queries. This method is asynchronous and does not take any parameters. It does not return a value.
* `CreateSchema`: Creates a GraphQL schema. This method does not take any parameters and does not return a value.
* `RegisterResolver`: Registers a GraphQL resolver. This method does not take any parameters and does not return a value.
* `QueryWithArguments`: Executes a GraphQL query with arguments. This method is asynchronous and does not take any parameters. It does not return a value.

## Usage
Here are two examples of using the `QueryExecutionBenchmarks` type:
```csharp
// Example 1: Running a simple query benchmark
var benchmarks = new QueryExecutionBenchmarks();
benchmarks.Setup();
await benchmarks.SimpleQuery();
benchmarks.Cleanup();

// Example 2: Running multiple query benchmarks
var benchmarks = new QueryExecutionBenchmarks();
benchmarks.Setup();
await benchmarks.SimpleQuery();
await benchmarks.NestedQuery();
await benchmarks.ComplexQuery();
benchmarks.Cleanup();
```

## Notes
When using the `QueryExecutionBenchmarks` type, it is essential to call `Setup` before running any benchmarking tests and `Cleanup` after running all tests to ensure that the environment is properly set up and cleaned up. The asynchronous methods (`SimpleQuery`, `NestedQuery`, `ComplexQuery`, `LargeQuery`, `IntrospectionQuery`, `MultipleSimpleQueries`, and `QueryWithArguments`) should be awaited to ensure that the queries are executed correctly. Additionally, the `CreateSchema` and `RegisterResolver` methods should be called before running any query benchmarks to ensure that the schema and resolvers are properly set up. The `QueryExecutionBenchmarks` type is not thread-safe, and its methods should not be called concurrently from multiple threads.
