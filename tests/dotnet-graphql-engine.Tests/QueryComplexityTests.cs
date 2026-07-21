#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.QueryAnalysis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GraphQLEngine.Tests.Domain;

sealed public class QueryComplexityTests
{
	[Fact]
	public void RecordFieldComplexity_WhenCalled_AccumulatesTotalScoreAndFieldCount()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query");

		// Act
		complexity.RecordFieldComplexity("users", 30);
		complexity.RecordFieldComplexity("posts", 70);

		// Assert
		complexity.TotalScore.Should().Be(100);
		complexity.FieldCount.Should().Be(2);
		complexity.FieldComplexities.Should().ContainKey("users").WhoseValue.Should().Be(30);
	}

	[Fact]
	public void RecordFieldComplexity_WithNegativeValue_ThrowsArgumentException()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query");

		// Act
		Action act = () => complexity.RecordFieldComplexity("users", -5);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("*negative*");
	}

	[Fact]
	public void CalculateLevel_WhenTotalScoreExceedsCriticalThreshold_SetsCriticalLevel()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 2500 };

		// Act
		complexity.CalculateLevel();

		// Assert
		complexity.Level.Should().Be(QueryComplexityLevel.Critical);
	}

	[Fact]
	public void IsAcceptable_WhenLevelIsCritical_ReturnsFalseRegardlessOfMaxScore()
	{
		// Arrange – score is within a generous max, but Critical level must still block it
		var complexity = new QueryComplexity("test-query") { TotalScore = 2001 };
		complexity.CalculateLevel();

		// Act
		var result = complexity.IsAcceptable(maxScore: 9999);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void GetReport_WhenCalled_ReturnsDetailedReport()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query")
		{
			TotalScore = 150,
			FieldCount = 5,
			MaxDepth = 3,
			MaxBreadth = 2,
			Level = QueryComplexityLevel.Medium
		};
		complexity.AddWarning("Test warning");

		// Act
		var report = complexity.GetReport();

		// Assert
		report.Should().NotBeNullOrEmpty();
		report.Should().Contain("Query Complexity Analysis");
		report.Should().Contain("Total Score: 150");
		report.Should().Contain("Level: Medium");
		report.Should().Contain("Field Count: 5");
		report.Should().Contain("Max Depth: 3");
		report.Should().Contain("Max Breadth: 2");
		report.Should().Contain("Test warning");
	}

	[Fact]
	public void Reset_WhenCalled_ClearsAllData()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query");
		complexity.RecordFieldComplexity("users", 50);
		complexity.RecordFieldComplexity("posts", 100);
		complexity.AddWarning("Test warning");
		complexity.MaxDepth = 5;
		complexity.MaxBreadth = 3;

		// Verify setup
		complexity.TotalScore.Should().Be(150);
		complexity.FieldCount.Should().Be(2);

		// Act
		complexity.Reset();

		// Assert
		complexity.TotalScore.Should().Be(0);
		complexity.FieldCount.Should().Be(0);
		complexity.MaxDepth.Should().Be(0);
		complexity.MaxBreadth.Should().Be(0);
		complexity.Level.Should().Be(QueryComplexityLevel.Low);
		complexity.FieldComplexities.Should().BeEmpty();
		complexity.Warnings.Should().BeEmpty();
	}

	[Fact]
	public void GetTopComplexFields_WhenCalled_ReturnsFieldsInDescendingOrder()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query");
		complexity.RecordFieldComplexity("users", 30);
		complexity.RecordFieldComplexity("posts", 100);
		complexity.RecordFieldComplexity("comments", 75);
		complexity.RecordFieldComplexity("likes", 200);

		// Act
		var topFields = complexity.GetTopComplexFields(2).ToList();

		// Assert
		complexity.TotalScore.Should().Be(405);
		topFields.Should().HaveCount(2);
		topFields[0].Key.Should().Be("likes");
		topFields[0].Value.Should().Be(200);
		topFields[1].Key.Should().Be("posts");
		topFields[1].Value.Should().Be(100);
	}

	[Fact]
	public void IsAcceptable_WhenScoreWithinLimit_ReturnsTrue()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 1000 };

		// Act
		var result = complexity.IsAcceptable(maxScore: 5000);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsAcceptable_WhenScoreExceedsLimit_ReturnsFalse()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 6000 };

		// Act
		var result = complexity.IsAcceptable(maxScore: 5000);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void CalculateLevel_WhenScoreIsZero_ReturnsLowLevel()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 0 };

		// Act
		complexity.CalculateLevel();

		// Assert
		complexity.Level.Should().Be(QueryComplexityLevel.Low);
	}

	[Fact]
	public void CalculateLevel_WhenScoreIs100_ReturnsLowLevel()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 100 };

		// Act
		complexity.CalculateLevel();

		// Assert
		complexity.Level.Should().Be(QueryComplexityLevel.Low);
	}

	[Fact]
	public void CalculateLevel_WhenScoreIs101_ReturnsMediumLevel()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 101 };

		// Act
		complexity.CalculateLevel();

		// Assert
		complexity.Level.Should().Be(QueryComplexityLevel.Medium);
	}

	[Fact]
	public void CalculateLevel_WhenScoreIs500_ReturnsMediumLevel()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 500 };

		// Act
		complexity.CalculateLevel();

		// Assert
		complexity.Level.Should().Be(QueryComplexityLevel.Medium);
	}

	[Fact]
	public void CalculateLevel_WhenScoreIs501_ReturnsHighLevel()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 501 };

		// Act
		complexity.CalculateLevel();

		// Assert
		complexity.Level.Should().Be(QueryComplexityLevel.High);
	}

	[Fact]
	public void CalculateLevel_WhenScoreIs2000_ReturnsHighLevel()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 2000 };

		// Act
		complexity.CalculateLevel();

		// Assert
		complexity.Level.Should().Be(QueryComplexityLevel.High);
	}

	[Fact]
	public void CalculateLevel_WhenScoreIs2001_ReturnsCriticalLevel()
	{
		// Arrange
		var complexity = new QueryComplexity("test-query") { TotalScore = 2001 };

		// Act
		complexity.CalculateLevel();

		// Assert
		complexity.Level.Should().Be(QueryComplexityLevel.Critical);
	}
}

sealed public class QueryAnalysisServiceTests
{
	private readonly QueryAnalysisService _analysisService;

	public QueryAnalysisServiceTests()
	{
		var logger = new Mock<ILogger<QueryAnalysisService>>();
		_analysisService = new QueryAnalysisService(logger.Object);
	}

	[Fact]
	public void AnalyzeQuery_WithSimpleQuery_CalculatesCorrectComplexity()
	{
		// Arrange
		var query = new GraphQLQuery("simple-query")
		{
			QueryString = "{ user(id: \"1\") { id name email } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(3); // user, id, name, email
		analysis.FieldCount.Should().Be(3);
		analysis.MaxDepth.Should().Be(1);
		analysis.Level.Should().Be(QueryComplexityLevel.Low);
	}

	[Fact]
	public void AnalyzeQuery_WithNestedQuery_CalculatesCorrectComplexity()
	{
		// Arrange
		var query = new GraphQLQuery("nested-query")
		{
			QueryString = "{ user(id: \"1\") { id name posts { id title } } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(6); // user, id, name, posts, id, title
		analysis.FieldCount.Should().Be(6);
		analysis.MaxDepth.Should().Be(2);
		analysis.Level.Should().Be(QueryComplexityLevel.Low);
	}

	[Fact]
	public void AnalyzeQuery_WithMultipleFields_CalculatesCorrectComplexity()
	{
		// Arrange
		var query = new GraphQLQuery("multi-field-query")
		{
			QueryString = "{ user(id: \"1\") { id name posts { id title comments { id text } } } users { id name } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(12); // user, id, name, posts, id, title, comments, id, text, users, id, name
		analysis.FieldCount.Should().Be(12);
		analysis.MaxDepth.Should().Be(3);
		analysis.Level.Should().Be(QueryComplexityLevel.Low);
	}

	[Fact]
	public void AnalyzeQuery_WithAliases_HandlesCorrectly()
	{
		// Arrange - aliases should be counted separately from field names
		var query = new GraphQLQuery("alias-query")
		{
			QueryString = "{ user: user(id: \"1\") { id name: username } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(3); // user: user, id, name: username
		analysis.FieldCount.Should().Be(3);
		analysis.FieldComplexities.Should().ContainKey("user");
		analysis.FieldComplexities.Should().ContainKey("id");
		analysis.FieldComplexities.Should().ContainKey("name");
	}

	[Fact]
	public void AnalyzeQuery_WithInlineFragment_HandlesCorrectly()
	{
		// Arrange
		var query = new GraphQLQuery("fragment-query")
		{
			QueryString = "{ user(id: \"1\") { ... on User { id name } posts { ... on Post { id title } } } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(6); // user, id, name, posts, id, title
		analysis.FieldCount.Should().Be(6);
		analysis.MaxDepth.Should().Be(3);
	}

	[Fact]
	public void AnalyzeQuery_WithNamedFragmentSpread_HandlesCorrectly()
	{
		// Arrange
		var query = new GraphQLQuery("named-fragment-query")
		{
			QueryString = "{ user(id: \"1\") { id name ...UserDetails } } fragment UserDetails on User { posts { id title } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(6); // user, id, name, posts, id, title
		analysis.FieldCount.Should().Be(6);
		analysis.MaxDepth.Should().Be(2);
	}

	[Fact]
	public void AnalyzeQuery_WithIntrospectionQuery_HandlesCorrectly()
	{
		// Arrange - introspection queries should be analyzed normally
		var query = new GraphQLQuery("introspection-query")
		{
			QueryString = "{ __schema { types { name } queryType { name } mutationType { name } subscriptionType { name } } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().BeGreaterThan(0);
		analysis.FieldCount.Should().BeGreaterThan(0);
		analysis.MaxDepth.Should().Be(3);
	}

	[Fact]
	public void AnalyzeQuery_WithEmptyQuery_ReturnsEmptyAnalysis()
	{
		// Arrange
		var query = new GraphQLQuery("empty-query")
		{
			QueryString = "{}"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(0);
		analysis.FieldCount.Should().Be(0);
		analysis.MaxDepth.Should().Be(0);
	}

	[Fact]
	public void AnalyzeQuery_WithDirectives_HandlesCorrectly()
	{
		// Arrange
		var query = new GraphQLQuery("directive-query")
		{
			QueryString = "{ user(id: \"1\", include: true) @include(if: true) { id name @skip(if: false) } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(3); // user, id, name
		analysis.FieldCount.Should().Be(3);
	}

	[Fact]
	public void AnalyzeQuery_WithArguments_HandlesCorrectly()
	{
		// Arrange
		var query = new GraphQLQuery("argument-query")
		{
			QueryString = "{ users(limit: 10, offset: 0) { id name posts(limit: 5) { id title } } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis.Should().NotBeNull();
		analysis.TotalScore.Should().Be(7); // users, id, name, posts, id, title, limit, offset
		analysis.FieldCount.Should().Be(7);
	}

	[Fact]
	public void IsQueryAllowed_WithAcceptableQuery_ReturnsTrue()
	{
		// Arrange
		var query = new GraphQLQuery("allowed-query")
		{
			QueryString = "{ user(id: \"1\") { id name } }"
		};

		// Act
		var result = _analysisService.IsQueryAllowed(query, maxComplexityScore: 5000);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsQueryAllowed_WithHighComplexityQuery_ReturnsFalse()
	{
		// Arrange - create a deeply nested query
		var query = new GraphQLQuery("deep-query")
		{
			QueryString = "{ user(id: \"1\") { posts { comments { author { friends { posts { comments { text } } } } } } }"
		};

		// Act
		var result = _analysisService.IsQueryAllowed(query, maxComplexityScore: 100);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void GetAnalysis_AfterAnalyzingQuery_ReturnsAnalysis()
	{
		// Arrange
		var query = new GraphQLQuery("test-analysis-query")
		{
			QueryString = "{ user(id: \"1\") { id name } }"
		};

		// Act - first analyze
		var analysis1 = _analysisService.AnalyzeQuery(query);

		// Assert
		analysis1.Should().NotBeNull();
		_analysisService.GetAnalysis(query.Id).Should().BeSameAs(analysis1);
	}

	[Fact]
	public void GetComplexityReport_WhenAnalysisExists_ReturnsReport()
	{
		// Arrange
		var query = new GraphQLQuery("report-query")
		{
			QueryString = "{ user(id: \"1\") { id name } }"
		};

		// Act
		var analysis = _analysisService.AnalyzeQuery(query);
		var report = _analysisService.GetComplexityReport(query.Id);

		// Assert
		report.Should().NotBeNullOrEmpty();
		report.Should().Contain("Query Complexity Analysis");
		report.Should().Contain("Total Score:");
	}

	[Fact]
	public void GetStatistics_WhenQueriesAnalyzed_ReturnsStatistics()
	{
		// Arrange
		var query1 = new GraphQLQuery("stats-query-1")
		{
			QueryString = "{ user(id: \"1\") { id } }"
		};
		var query2 = new GraphQLQuery("stats-query-2")
		{
			QueryString = "{ users { id name } }"
		};

		// Act
		_analysisService.AnalyzeQuery(query1);
		_analysisService.AnalyzeQuery(query2);
		var stats = _analysisService.GetStatistics();

		// Assert
		stats.Should().NotBeNull();
		stats["TotalQueriesAnalyzed"].Should().Be(2);
		stats["AverageComplexityScore"].Should().BeOfType<int>();
		stats["MaxComplexityScore"].Should().BeOfType<int>();
		stats["ComplexityDistribution"].Should().BeOfType<Dictionary<string, int>>();
	}
}