namespace benchmarks.dotnet_graphql_engine.Benchmarks;

public static class QueryExecutionBenchmarksValidation
{
    /// <summary>
    /// Validates the specified <paramref name="value"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="QueryExecutionBenchmarks"/> instance to validate.</param>
    /// <returns>A list of problems with the <paramref name="value"/>.</returns>
    public static IReadOnlyList<string> Validate(this QueryExecutionBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.Setup))
        {
            problems.Add("Setup method is null or empty.");
        }

        if (string.IsNullOrEmpty(value.Cleanup))
        {
            problems.Add("Cleanup method is null or empty.");
        }

        if (value.SimpleQuery == null)
        {
            problems.Add("SimpleQuery method is null.");
        }

        if (value.NestedQuery == null)
        {
            problems.Add("NestedQuery method is null.");
        }

        if (value.ComplexQuery == null)
        {
            problems.Add("ComplexQuery method is null.");
        }

        if (value.LargeQuery == null)
        {
            problems.Add("LargeQuery method is null.");
        }

        if (value.IntrospectionQuery == null)
        {
            problems.Add("IntrospectionQuery method is null.");
        }

        if (value.MultipleSimpleQueries == null)
        {
            problems.Add("MultipleSimpleQueries method is null.");
        }

        if (value.CreateSchema == null)
        {
            problems.Add("CreateSchema method is null.");
        }

        if (value.RegisterResolver == null)
        {
            problems.Add("RegisterResolver method is null.");
        }

        if (value.QueryWithArguments == null)
        {
            problems.Add("QueryWithArguments method is null.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The <see cref="QueryExecutionBenchmarks"/> instance to validate.</param>
    /// <returns><c>true</c> if the <paramref name="value"/> is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this QueryExecutionBenchmarks? value) => !Validate(value).Any();

    /// <summary>
    /// Ensures that the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The <see cref="QueryExecutionBenchmarks"/> instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this QueryExecutionBenchmarks? value)
    {
        var problems = Validate(value);
        if (problems.Any())
        {
            throw new ArgumentException($"Invalid QueryExecutionBenchmarks: {string.Join(Environment.NewLine, problems)}", nameof(value));
        }
    }
}
