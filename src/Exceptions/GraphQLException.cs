#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Exceptions;

/// <summary>
/// Base exception for GraphQL engine operations
/// </summary>
sealed public class GraphQLException : Exception
{
    public string? ErrorCode { get; set; }
    public Dictionary<string, object> Extensions { get; set; } = new();

    public GraphQLException(string message) : base(message)
    {
    }

    public GraphQLException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public GraphQLException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public void AddExtension(string key, object value)
    {
        Extensions[key] = value;
    }
}

/// <summary>
/// Exception thrown during schema operations
/// </summary>
sealed public class SchemaException : GraphQLException
{
    public SchemaException(string message) : base(message, "SCHEMA_ERROR") { }

    public SchemaException(string message, Exception innerException)
        : base(message, innerException) { ErrorCode = "SCHEMA_ERROR"; }
}

/// <summary>
/// Exception thrown during query execution
/// </summary>
sealed public class ExecutionException : GraphQLException
{
    public string? FieldPath { get; set; }
    public int? LineNumber { get; set; }

    public ExecutionException(string message) : base(message, "EXECUTION_ERROR") { }

    public ExecutionException(string message, string fieldPath, int? line = null)
        : base(message, "EXECUTION_ERROR")
    {
        FieldPath = fieldPath;
        LineNumber = line;
    }
}

/// <summary>
/// Exception thrown during query complexity analysis
/// </summary>
sealed public class QueryComplexityException : GraphQLException
{
    public int ActualScore { get; set; }
    public int MaxScore { get; set; }

    public QueryComplexityException(string message, int actual, int max)
        : base(message, "QUERY_COMPLEXITY_EXCEEDED")
    {
        ActualScore = actual;
        MaxScore = max;
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
sealed public class ValidationException : GraphQLException
{
    public List<string> ValidationErrors { get; set; } = new();

    public ValidationException(string message, List<string> errors)
        : base(message, "VALIDATION_ERROR")
    {
        ValidationErrors = errors;
    }
}

/// <summary>
/// Exception thrown during data loading
/// </summary>
sealed public class DataLoaderException : GraphQLException
{
    public string LoaderName { get; set; } = string.Empty;

    public DataLoaderException(string message, string loaderName)
        : base(message, "DATA_LOADER_ERROR")
    {
        LoaderName = loaderName;
    }
}

/// <summary>
/// Exception thrown during subscription operations
/// </summary>
sealed public class SubscriptionException : GraphQLException
{
    public string? ClientId { get; set; }

    public SubscriptionException(string message) : base(message, "SUBSCRIPTION_ERROR") { }

    public SubscriptionException(string message, string clientId)
        : base(message, "SUBSCRIPTION_ERROR")
    {
        ClientId = clientId;
    }
}
