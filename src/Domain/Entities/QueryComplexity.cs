#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents the complexity analysis of a GraphQL query
/// </summary>
sealed public class QueryComplexity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string QueryId { get; set; } = string.Empty;
    public int TotalScore { get; set; } = 0;
    public int FieldCount { get; set; } = 0;
    public int MaxDepth { get; set; } = 0;
    public int MaxBreadth { get; set; } = 0;
    public QueryComplexityLevel Level { get; set; } = QueryComplexityLevel.Low;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    private readonly Dictionary<string, int> _fieldComplexities = new();
    public IReadOnlyDictionary<string, int> FieldComplexities => _fieldComplexities.AsReadOnly();

    private readonly List<string> _warnings = new();
    public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();

    public QueryComplexity()
    {
    }

    public QueryComplexity(string queryId)
    {
        QueryId = queryId ?? throw new ArgumentNullException(nameof(queryId));
    }

    /// <summary>
    /// Records the complexity of a field
    /// </summary>
    public void RecordFieldComplexity(string fieldName, int complexity)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentException("Field name cannot be empty", nameof(fieldName));

        if (complexity < 0)
            throw new ArgumentException("Complexity cannot be negative", nameof(complexity));

        _fieldComplexities[fieldName] = complexity;
        TotalScore += complexity;
        FieldCount++;
    }

    /// <summary>
    /// Adds a complexity warning
    /// </summary>
    public void AddWarning(string warning)
    {
        if (string.IsNullOrEmpty(warning)) return;

        _warnings.Add(warning);
    }

    /// <summary>
    /// Determines if the query complexity is acceptable
    /// </summary>
    public bool IsAcceptable(int maxScore = 5000)
    {
        return TotalScore <= maxScore && Level != QueryComplexityLevel.Critical;
    }

    /// <summary>
    /// Calculates the complexity level based on score
    /// </summary>
    public void CalculateLevel()
    {
        Level = TotalScore switch
        {
            <= 100 => QueryComplexityLevel.Low,
            <= 500 => QueryComplexityLevel.Medium,
            <= 2000 => QueryComplexityLevel.High,
            _ => QueryComplexityLevel.Critical
        };
    }

    /// <summary>
    /// Gets the top N most complex fields
    /// </summary>
    public IEnumerable<KeyValuePair<string, int>> GetTopComplexFields(int count = 5)
    {
        return _fieldComplexities
            .OrderByDescending(kv => kv.Value)
            .Take(count);
    }

    /// <summary>
    /// Gets detailed complexity report
    /// </summary>
    public string GetReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine($"Query Complexity Analysis (ID: {QueryId})");
        report.AppendLine($"Total Score: {TotalScore}");
        report.AppendLine($"Level: {Level}");
        report.AppendLine($"Field Count: {FieldCount}");
        report.AppendLine($"Max Depth: {MaxDepth}");
        report.AppendLine($"Max Breadth: {MaxBreadth}");

        if (_warnings.Any())
        {
            report.AppendLine("Warnings:");
            foreach (var warning in _warnings)
                report.AppendLine($"  - {warning}");
        }

        var topFields = GetTopComplexFields(10).ToList();
        if (topFields.Any())
        {
            report.AppendLine("Top Complex Fields:");
            foreach (var field in topFields)
                report.AppendLine($"  - {field.Key}: {field.Value}");
        }

        return report.ToString();
    }

    /// <summary>
    /// Clears all recorded complexities and resets the analysis
    /// </summary>
    public void Reset()
    {
        TotalScore = 0;
        FieldCount = 0;
        MaxDepth = 0;
        MaxBreadth = 0;
        Level = QueryComplexityLevel.Low;
        _fieldComplexities.Clear();
        _warnings.Clear();
    }
}

/// <summary>
/// Enumeration of query complexity levels
/// </summary>
public enum QueryComplexityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
