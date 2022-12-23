# Query Complexity Analysis Guide

## Introduction

Query complexity analysis is a crucial feature for GraphQL servers, designed to prevent abuse and ensure service stability. It helps you manage the resources consumed by incoming GraphQL queries by assigning a "complexity score" to each query and rejecting those that exceed a predefined limit. This protects your backend from overly complex or deeply nested queries that could lead to performance degradation or denial-of-service attacks.

The `dotnet-graphql-engine` provides a built-in query complexity analysis service that calculates a score based on the query's structure and depth, allowing you to set maximum allowable limits.

## Configuration

Query complexity analysis is configured via the `GraphQLEngineOptions` when you add the GraphQL engine services to your `IServiceCollection`.

Here are the key options related to query complexity:

| Option             | Type | Default Value | Description                                                                                                                                                                                                                                                                                                                                                                                        |
| :----------------- | :--- | :------------ | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `MaxQueryComplexity` | `int`| `5000`        | The maximum allowed total complexity score for any incoming GraphQL query. If a query's calculated complexity exceeds this value, it will be rejected. This value should be tuned based on your application's performance characteristics and expected query patterns. |
| `MaxQueryDepth`    | `int`| `10`          | The maximum allowed depth of nested selections in a GraphQL query. Queries exceeding this depth will be rejected. This helps prevent excessively deep, recursive queries.                                                                                                                                                                                                                                   |

**Example Configuration:**

To set custom complexity limits, you can provide an action to the `AddGraphQLEngine` method:

```csharp
using GraphQLEngine.Configuration;

public void ConfigureServices(IServiceCollection services)
{
    services.AddGraphQLEngine(options =>
    {
        options.MaxQueryComplexity = 2000; // Set a custom max complexity score
        options.MaxQueryDepth = 7;         // Set a custom max query depth
        options.EnableIntrospection = false; // Example: Disable introspection for production
    });

    // ... other service registrations
}
```

## How Complexity is Calculated

The `QueryAnalysisService` calculates the complexity score by traversing the parsed GraphQL query structure.

*   **Base Complexity per Field**: Currently, each selected field in the query (including those within nested selections and fragments) contributes a base complexity of `1`.
*   **Query Depth**: The maximum nesting level of fields in the query is determined.
*   **Field Count**: The total number of unique fields selected in the query is counted.

The `QueryAnalysisService` specifically handles:

*   **Nested Selections**: Fields within sub-selections are correctly included in the total field count and contribute to the query depth.
*   **Inline Fragments and Union Types**: The fields selected within inline fragments (e.g., `... on ConcreteType { expensiveField }`) on union or interface types are now correctly accounted for in the complexity calculation. This prevents clients from bypassing complexity limits by using fragments.

The total complexity score is an aggregate of these factors.

## Complexity Levels and Warnings

The `QueryAnalysisService` assigns a complexity level to each analyzed query and can add warnings based on predefined thresholds:

*   **`Low`**: The query is simple and poses no threat.
*   **`Medium`**: The query has moderate complexity.
*   **`High`**: The query is complex and might warrant optimization. A warning is typically issued.
*   **`Critical`**: The query is excessively complex and likely to be rejected if it exceeds `MaxQueryComplexity`. A critical warning is issued.

These warnings are logged by the `QueryAnalysisService` and can be retrieved as part of the `QueryComplexity` analysis result.

## Choosing Appropriate Limits for Production

Setting the right `MaxQueryComplexity` and `MaxQueryDepth` is crucial for production environments.

1.  **Start with Monitoring**: Begin by deploying your application with logging for query complexity enabled. Monitor the complexity scores of typical queries under normal load.
2.  **Analyze Peak Usage**: Identify the complexity scores of your most demanding (but legitimate) queries.
3.  **Set Conservative Limits**: Start with limits slightly above your observed legitimate peak usage. For example, if your most complex legitimate query scores 1500, set `MaxQueryComplexity` to 2000-2500.
4.  **Iterate and Refine**: Gradually adjust the limits based on real-world usage and performance metrics. Be prepared to refine these values as your application evolves.
5.  **Consider Introspection**: If `EnableIntrospection` is true, be aware that introspection queries can be quite complex. You might need to set `MaxQueryComplexity` higher to accommodate them, or disable introspection in production if not needed.

## Future Enhancements

Currently, the complexity calculation uses a uniform base score of `1` for every field. Future enhancements could include:

*   **Per-Field Complexity Multipliers**: Allowing specific fields to have higher complexity scores (e.g., `largeListField(first: 100)` might have a multiplier of 100).
*   **Custom `IComplexityCalculator` Implementations**: Providing an extensibility point for users to define their own sophisticated complexity calculation logic.

If these advanced features are critical for your use case, consider contributing to the project or extending the `QueryAnalysisService` locally.
