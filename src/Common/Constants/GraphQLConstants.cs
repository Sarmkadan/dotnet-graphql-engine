#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Common.Constants;

/// <summary>
/// GraphQL engine constants and configuration defaults
/// </summary>
public static class GraphQLConstants
{
    // Query limits
    public const int DefaultMaxQueryDepth = 10;
    public const int DefaultMaxQueryComplexity = 5000;
    public const int DefaultMaxQueryLength = 100000;
    public const int DefaultMaxSelectionSetSize = 1000;

    // Performance
    public const int DefaultQueryTimeoutMs = 30000;
    public const int DefaultDataLoaderBatchSize = 100;
    public const int DefaultDataLoaderDelayMs = 10;

    // Subscriptions
    public const int DefaultMaxSubscriptionConnections = 1000;
    public const int DefaultSubscriptionTimeoutMs = 300000;
    public const int DefaultHeartbeatIntervalMs = 5000;

    // Schema
    public const int DefaultMaxSchemaSize = 1000000; // 1MB
    public const string DefaultQueryTypeName = "Query";
    public const string DefaultMutationTypeName = "Mutation";
    public const string DefaultSubscriptionTypeName = "Subscription";

    // Built-in scalar types
    public const string ScalarString = "String";
    public const string ScalarInt = "Int";
    public const string ScalarFloat = "Float";
    public const string ScalarBoolean = "Boolean";
    public const string ScalarID = "ID";

    // Directives
    public const string DirectiveSkip = "skip";
    public const string DirectiveInclude = "include";
    public const string DirectiveDeprecated = "deprecated";

    // Error codes
    public const string ErrorCodeSyntax = "GRAPHQL_SYNTAX_ERROR";
    public const string ErrorCodeValidation = "GRAPHQL_VALIDATION_ERROR";
    public const string ErrorCodeExecution = "GRAPHQL_EXECUTION_ERROR";
    public const string ErrorCodeFieldNotFound = "FIELD_NOT_FOUND";
    public const string ErrorCodeTypeNotFound = "TYPE_NOT_FOUND";
    public const string ErrorCodeQueryTooComplex = "QUERY_TOO_COMPLEX";

    // Introspection
    public const string IntrospectionQuery = "__type";
    public const string IntrospectionSchema = "__schema";
    public const string IntrospectionTypeName = "__typename";

    // Caching
    public const int DefaultCacheTTLSeconds = 300;
    public const int DefaultCacheMaxSize = 1000;

    // Logging
    public const string LogCategoryCore = "GraphQLEngine.Core";
    public const string LogCategorySchema = "GraphQLEngine.Schema";
    public const string LogCategoryExecution = "GraphQLEngine.Execution";
    public const string LogCategoryDataLoader = "GraphQLEngine.DataLoader";
    public const string LogCategorySubscription = "GraphQLEngine.Subscription";
}

/// <summary>
/// HTTP status codes for GraphQL responses
/// </summary>
public static class GraphQLHttpStatus
{
    public const int Success = 200;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int InternalError = 500;
}

/// <summary>
/// Header names for GraphQL requests
/// </summary>
public static class GraphQLHeaders
{
    public const string ContentType = "Content-Type";
    public const string ContentTypeValue = "application/json";
    public const string Authorization = "Authorization";
    public const string XGraphQLComplexity = "X-GraphQL-Complexity";
    public const string XGraphQLDepth = "X-GraphQL-Depth";
    public const string XRequestId = "X-Request-ID";
}
