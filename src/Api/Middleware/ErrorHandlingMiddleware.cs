// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Exceptions;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Api.Middleware;

/// <summary>
/// Error handling middleware that catches and formats exceptions
/// Converts various exception types to appropriate error responses
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly ErrorHandlingOptions _options;

    public ErrorHandlingMiddleware(
        ILogger<ErrorHandlingMiddleware> logger,
        ErrorHandlingOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new ErrorHandlingOptions();
    }

    /// <summary>
    /// Handles an exception and returns a formatted error response
    /// </summary>
    public ErrorResponse HandleException(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {ExceptionType}", exception.GetType().Name);

        var response = exception switch
        {
            GraphQLException ex => HandleGraphQLException(ex),
            ArgumentException ex => HandleArgumentException(ex),
            InvalidOperationException ex => HandleInvalidOperationException(ex),
            TimeoutException ex => HandleTimeoutException(ex),
            _ => HandleGeneralException(exception)
        };

        // Add correlation ID for tracking
        response.CorrelationId = Guid.NewGuid().ToString();
        response.Timestamp = DateTime.UtcNow;

        return response;
    }

    /// <summary>
    /// Handles GraphQL-specific exceptions
    /// </summary>
    private ErrorResponse HandleGraphQLException(GraphQLException ex)
    {
        _logger.LogWarning("GraphQL exception: {Message}", ex.Message);

        return new ErrorResponse
        {
            StatusCode = 400,
            Code = ex.ErrorCode ?? "GRAPHQL_ERROR",
            Message = ex.Message,
            Details = ex.Message,
            IsUserFacingError = true,
            Severity = "warning"
        };
    }

    /// <summary>
    /// Handles argument validation exceptions
    /// </summary>
    private ErrorResponse HandleArgumentException(ArgumentException ex)
    {
        _logger.LogWarning("Validation error: {Message}", ex.Message);

        return new ErrorResponse
        {
            StatusCode = 400,
            Code = "VALIDATION_ERROR",
            Message = "Invalid request parameters",
            Details = ex.Message,
            IsUserFacingError = true,
            Severity = "warning"
        };
    }

    /// <summary>
    /// Handles invalid operation exceptions
    /// </summary>
    private ErrorResponse HandleInvalidOperationException(InvalidOperationException ex)
    {
        _logger.LogWarning("Invalid operation: {Message}", ex.Message);

        return new ErrorResponse
        {
            StatusCode = 409,
            Code = "INVALID_OPERATION",
            Message = "The requested operation is invalid",
            Details = _options.IncludeDetailedErrorMessages ? ex.Message : null,
            IsUserFacingError = true,
            Severity = "warning"
        };
    }

    /// <summary>
    /// Handles timeout exceptions
    /// </summary>
    private ErrorResponse HandleTimeoutException(TimeoutException ex)
    {
        _logger.LogWarning("Request timeout: {Message}", ex.Message);

        return new ErrorResponse
        {
            StatusCode = 408,
            Code = "REQUEST_TIMEOUT",
            Message = "The request took too long to complete",
            Details = "Please try again with a simpler query",
            IsUserFacingError = true,
            Severity = "info"
        };
    }

    /// <summary>
    /// Handles general, unspecified exceptions
    /// </summary>
    private ErrorResponse HandleGeneralException(Exception ex)
    {
        _logger.LogError(ex, "Unexpected error occurred");

        return new ErrorResponse
        {
            StatusCode = 500,
            Code = "INTERNAL_ERROR",
            Message = "An unexpected error occurred",
            Details = _options.IncludeDetailedErrorMessages ? ex.Message : null,
            IsUserFacingError = false,
            Severity = "error"
        };
    }
}

/// <summary>
/// Error handling configuration options
/// </summary>
public class ErrorHandlingOptions
{
    public bool IncludeDetailedErrorMessages { get; set; } = false;
    public bool LogStackTraces { get; set; } = true;
    public bool IncludeStackTraceInResponse { get; set; } = false;
    public string? ErrorTrackingService { get; set; }
}

/// <summary>
/// Formatted error response
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; } = 500;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
    public bool IsUserFacingError { get; set; }
    public string Severity { get; set; } = "error";
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }

    public string ToJson()
    {
        var dict = new Dictionary<string, object>
        {
            { "error", new {
                code = Code,
                message = Message,
                details = Details,
                correlationId = CorrelationId,
                timestamp = Timestamp
            }}
        };

        return System.Text.Json.JsonSerializer.Serialize(dict);
    }
}

/// <summary>
/// Represents a validation error for a specific field
/// </summary>
public class FieldErrorDto
{
    public string FieldName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}

/// <summary>
/// Validation error response with field-level errors
/// </summary>
public class ValidationErrorResponse
{
    public int StatusCode { get; set; } = 400;
    public string Code { get; set; } = "VALIDATION_ERROR";
    public string Message { get; set; } = "Validation failed";
    public List<FieldErrorDto> FieldErrors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
