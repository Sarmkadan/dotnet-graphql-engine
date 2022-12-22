#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Linq; // Added for LINQ extensions

namespace GraphQLEngine.Services.QueryAnalysis;

/// <summary>
/// Service for analyzing GraphQL query complexity
/// </summary>
sealed public class QueryAnalysisService
{
    private readonly ILogger<QueryAnalysisService> _logger;
    private readonly Dictionary<string, QueryComplexity> _analyses = new();

    public QueryAnalysisService(ILogger<QueryAnalysisService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes the complexity of a GraphQL query
    /// </summary>
    public QueryComplexity AnalyzeQuery(GraphQLQuery query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        var analysis = new QueryComplexity(query.Id);

        try
        {
            // Calculate max depth and field count from the structured query fields
            analysis.MaxDepth = CalculateMaxDepth(query.RootSelectedFields);
            analysis.FieldCount = CalculateFieldCount(query.RootSelectedFields);

            // Calculate complexity score using the hierarchical structure
            CalculateComplexityScore(query.RootSelectedFields, analysis, 1);

            // Determine complexity level
            analysis.CalculateLevel();

            // Add warnings for high complexity
            if (analysis.Level == QueryComplexityLevel.Critical)
            {
                analysis.AddWarning("Query complexity is critical. Consider breaking it into smaller queries.");
            }
            else if (analysis.Level == QueryComplexityLevel.High)
            {
                analysis.AddWarning("Query complexity is high. Consider optimizing.");
            }

            _analyses[query.Id] = analysis;

            _logger.LogInformation("Query analysis completed: {QueryId}, Score: {Score}, Level: {Level}",
                query.Id, analysis.TotalScore, analysis.Level);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query: {QueryId}", query.Id);
            analysis.AddWarning($"Analysis failed: {ex.Message}");
            return analysis;
        }
    }

    /// <summary>
    /// Recursively calculates the complexity score for a list of QueryFields.
    /// </summary>
    /// <param name="fields">The list of query fields to analyze.</param>
    /// <param name="analysis">The QueryComplexity instance to update.</param>
    /// <param name="depth">The current depth in the query tree.</param>
    private void CalculateComplexityScore(IReadOnlyList<QueryField> fields, QueryComplexity analysis, int depth)
    {
        foreach (var field in fields)
        {
            // Base complexity for each field
            var fieldComplexity = 1; 

            // Record complexity for the current field
            analysis.RecordFieldComplexity(field.Name, fieldComplexity);

            // Recursively calculate complexity for nested fields
            if (field.Fields.Any())
            {
                CalculateComplexityScore(field.Fields, analysis, depth + 1);
            }
        }
    }

    /// <summary>
    /// Recursively calculates the maximum depth of the query.
    /// </summary>
    private int CalculateMaxDepth(IReadOnlyList<QueryField> fields, int currentDepth = 0)
    {
        if (!fields.Any())
        {
            return currentDepth;
        }

        return fields.Max(f => CalculateMaxDepth(f.Fields, currentDepth + 1));
    }

    /// <summary>
    /// Recursively calculates the total number of selected fields.
    /// </summary>
    private int CalculateFieldCount(IReadOnlyList<QueryField> fields)
    {
        if (!fields.Any())
        {
            return 0;
        }

        return fields.Count + fields.Sum(f => CalculateFieldCount(f.Fields));
    }

    /// <summary>
    /// Gets a previous analysis by query ID
    /// </summary>
    public QueryComplexity? GetAnalysis(string queryId)
    {
        _analyses.TryGetValue(queryId, out var analysis);
        return analysis;
    }

    /// <summary>
    /// Checks if a query should be allowed based on complexity
    /// </summary>
    public bool IsQueryAllowed(GraphQLQuery query, int maxComplexityScore = 5000)
    {
        var analysis = AnalyzeQuery(query);
        return analysis.IsAcceptable(maxComplexityScore);
    }

    /// <summary>
    /// Gets a detailed complexity report
    /// </summary>
    public string GetComplexityReport(string queryId)
    {
        var analysis = GetAnalysis(queryId);
        if (analysis is null)
            return $"No analysis found for query: {queryId}";

        return analysis.GetReport();
    }

    /// <summary>
    /// Gets statistics for all analyzed queries
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        var totalScore = _analyses.Values.Sum(a => a.TotalScore);
        var avgScore = _analyses.Count > 0 ? totalScore / _analyses.Count : 0;

        var complexityDistribution = new Dictionary<string, int>
        {
            { "Low", _analyses.Values.Count(a => a.Level == QueryComplexityLevel.Low) },
            { "Medium", _analyses.Values.Count(a => a.Level == QueryComplexityLevel.Medium) },
            { "High", _analyses.Values.Count(a => a.Level == QueryComplexityLevel.High) },
            { "Critical", _analyses.Values.Count(a => a.Level == QueryComplexityLevel.Critical) }
        };

        return new Dictionary<string, object>
        {
            { "TotalQueriesAnalyzed", _analyses.Count },
            { "AverageComplexityScore", avgScore },
            { "MaxComplexityScore", _analyses.Count > 0 ? _analyses.Values.Max(a => a.TotalScore) : 0 },
            { "ComplexityDistribution", complexityDistribution },
            { "Timestamp", DateTime.UtcNow }
        };
    }

    /// <summary>
    /// Clears old analyses (older than specified days)
    /// </summary>
    public int ClearOldAnalyses(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var keysToRemove = _analyses
            .Where(kv => kv.Value.AnalyzedAt < cutoffDate)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in keysToRemove)
            _analyses.Remove(key);

        _logger.LogInformation("Cleared {Count} old query analyses", keysToRemove.Count);
        return keysToRemove.Count;
    }
}
