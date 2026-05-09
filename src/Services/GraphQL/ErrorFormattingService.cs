// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Configuration;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Exceptions;
using Microsoft.Extensions.Logging;
using ExecutionContext = GraphQLEngine.Domain.Entities.ExecutionContext;

namespace GraphQLEngine.Services.GraphQL;

/// <summary>
/// Service for formatting and standardizing GraphQL errors
/// </summary>
public class ErrorFormattingService
{
    private readonly ILogger<ErrorFormattingService> _logger;
    private readonly GraphQLEngineOptions _options;

    public ErrorFormattingService(ILogger<ErrorFormattingService> logger,
        GraphQLEngineOptions options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Formats an execution error for client response
    /// </summary>
    public Dictionary<string, object> FormatError(ExecutionError error)
    {
        if (error == null) throw new ArgumentNullException(nameof(error));

        var response = new Dictionary<string, object>
        {
            { "message", error.Message }
        };

        // Add location information if available
        if (!string.IsNullOrEmpty(error.Field))
        {
            response["locations"] = new[] { new { field = error.Field, line = error.LineNumber } };
        }

        // Add extensions for development
        if (_options.EnableDetailedErrorMessages && !string.IsNullOrEmpty(error.StackTrace))
        {
            response["extensions"] = new
            {
                code = "INTERNAL_ERROR",
                stacktrace = error.StackTrace
            };
        }

        return response;
    }

    /// <summary>
    /// Formats a GraphQL exception
    /// </summary>
    public Dictionary<string, object> FormatException(GraphQLException ex)
    {
        if (ex == null) throw new ArgumentNullException(nameof(ex));

        var response = new Dictionary<string, object>
        {
            { "message", ex.Message },
            { "errorCode", ex.ErrorCode ?? "UNKNOWN_ERROR" }
        };

        // Add extensions
        if (ex.Extensions.Count > 0)
        {
            response["extensions"] = ex.Extensions;
        }

        // Add stack trace for development
        if (_options.EnableDetailedErrorMessages && !string.IsNullOrEmpty(ex.StackTrace))
        {
            response["stacktrace"] = ex.StackTrace;
        }

        return response;
    }

    /// <summary>
    /// Formats a validation exception
    /// </summary>
    public Dictionary<string, object> FormatValidationException(ValidationException ex)
    {
        if (ex == null) throw new ArgumentNullException(nameof(ex));

        return new Dictionary<string, object>
        {
            { "message", ex.Message },
            { "errorCode", ex.ErrorCode ?? "VALIDATION_ERROR" },
            { "errors", ex.ValidationErrors },
            { "errorCount", ex.ValidationErrors.Count }
        };
    }

    /// <summary>
    /// Formats a query complexity exception
    /// </summary>
    public Dictionary<string, object> FormatQueryComplexityException(
        QueryComplexityException ex)
    {
        if (ex == null) throw new ArgumentNullException(nameof(ex));

        return new Dictionary<string, object>
        {
            { "message", ex.Message },
            { "errorCode", ex.ErrorCode ?? "QUERY_COMPLEXITY_EXCEEDED" },
            { "actualScore", ex.ActualScore },
            { "maxScore", ex.MaxScore },
            { "severity", ex.ActualScore > ex.MaxScore * 2 ? "critical" : "warning" }
        };
    }

    /// <summary>
    /// Formats execution errors from a context
    /// </summary>
    public Dictionary<string, object> FormatExecutionErrors(ExecutionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var errors = context.Errors
            .Select(FormatError)
            .ToList();

        return new Dictionary<string, object>
        {
            { "errors", errors },
            { "errorCount", errors.Count },
            { "executionId", context.Id },
            { "state", context.State.ToString() }
        };
    }

    /// <summary>
    /// Formats a general exception
    /// </summary>
    public Dictionary<string, object> FormatGeneralException(Exception ex)
    {
        if (ex == null) throw new ArgumentNullException(nameof(ex));

        _logger.LogError(ex, "Unhandled exception");

        var response = new Dictionary<string, object>
        {
            { "message", "An internal error occurred" },
            { "errorCode", "INTERNAL_ERROR" }
        };

        // Add details for development
        if (_options.EnableDetailedErrorMessages)
        {
            response["actualMessage"] = ex.Message;
            response["exceptionType"] = ex.GetType().Name;
            response["stacktrace"] = ex.StackTrace ?? string.Empty;
        }

        return response;
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    public Dictionary<string, object> CreateErrorResponse(
        string message,
        string? errorCode = null,
        Dictionary<string, object>? extensions = null)
    {
        var response = new Dictionary<string, object>
        {
            { "message", message },
            { "errorCode", errorCode ?? "UNKNOWN_ERROR" },
            { "timestamp", DateTime.UtcNow }
        };

        if (extensions != null && extensions.Count > 0)
        {
            response["extensions"] = extensions;
        }

        return response;
    }

    /// <summary>
    /// Sanitizes error message based on configuration
    /// </summary>
    public string SanitizeErrorMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return "An error occurred";

        if (_options.EnableDetailedErrorMessages)
            return message;

        // Return generic message for production
        return "An error occurred while processing your request";
    }
}
