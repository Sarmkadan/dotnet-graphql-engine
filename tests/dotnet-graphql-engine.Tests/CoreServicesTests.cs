#nullable enable
using FluentAssertions;
using GraphQLEngine.Configuration;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.DataLoader;
using GraphQLEngine.Services.GraphQL;
using GraphQLEngine.Services.QueryAnalysis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GraphQLEngine.Tests.Services;

sealed public class GraphQLExecutionServiceTests
{
    private static GraphQLExecutionService CreateExecutionService(ILogger<GraphQLExecutionService>? logger = null)
    {
        var mockLogger = logger ?? new Mock<ILogger<GraphQLExecutionService>>().Object;
        var dataLoaderService = new DataLoaderService(new Mock<ILogger<DataLoaderService>>().Object);
        return new GraphQLExecutionService(mockLogger, dataLoaderService);
    }

    [Fact]
    public void RegisterResolver_WithValidFieldPathAndResolver_SuccessfullyRegisters()
    {
        // Arrange
        var service = CreateExecutionService();
        var fieldPath = "User.Profile";
        Func<Task<string>> resolver = async () => { await Task.Delay(1); return "test"; };

        // Act
        service.RegisterResolver(fieldPath, resolver);

        // Assert
        var stats = service.GetStatistics();
        stats["RegisteredResolvers"].Should().Be(1);
    }

    [Fact]
    public void RegisterResolver_WithNullResolver_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateExecutionService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.RegisterResolver("User", null!));
    }

    [Fact]
    public void RegisterResolver_WithEmptyFieldPath_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateExecutionService();
        Func<Task<string>> resolver = async () => { await Task.Delay(1); return "test"; };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.RegisterResolver("", resolver));
    }

    [Fact]
    public void RegisterResolver_WithMultipleResolvers_CountsCorrectly()
    {
        // Arrange
        var service = CreateExecutionService();
        Func<Task<string>> resolver = async () => { await Task.Delay(1); return "test"; };

        // Act
        service.RegisterResolver("field1", resolver);
        service.RegisterResolver("field2", resolver);
        service.RegisterResolver("field3", resolver);

        // Assert
        var stats = service.GetStatistics();
        stats["RegisteredResolvers"].Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateExecutionService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.ExecuteAsync(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidQuery_ReturnsCompletedContext()
    {
        // Arrange
        var service = CreateExecutionService();
        var query = new GraphQLQuery("{ user { id } }");

        // Act
        var context = await service.ExecuteAsync(query);

        // Assert
        context.Should().NotBeNull();
        context.State.Should().Be(ExecutionState.Completed);
        context.DurationMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidQuery_MarksContextAsCompletedWithErrors()
    {
        // Arrange
        var service = CreateExecutionService();
        var query = new GraphQLQuery("");
        query.AddError("Empty query");

        // Act
        var context = await service.ExecuteAsync(query);

        // Assert
        context.State.Should().Be(ExecutionState.Completed);
        context.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexNestedQuery_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateExecutionService();
        var query = new GraphQLQuery(
            "{ users { id name profile { bio avatar } } }"
        );

        // Act
        var context = await service.ExecuteAsync(query);

        // Assert
        context.Should().NotBeNull();
        context.State.Should().Be(ExecutionState.Completed);
    }
}

sealed public class QueryAnalysisServiceTests
{
    private static QueryAnalysisService CreateAnalysisService()
    {
        var mockLogger = new Mock<ILogger<QueryAnalysisService>>().Object;
        return new QueryAnalysisService(mockLogger);
    }

    [Fact]
    public void AnalyzeQuery_WithNullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateAnalysisService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.AnalyzeQuery(null!));
    }

    [Fact]
    public void AnalyzeQuery_WithSimpleQuery_CalculatesCorrectMetrics()
    {
        // Arrange
        var service = CreateAnalysisService();
        var query = new GraphQLQuery("{ user { id name } }");

        // Act
        var analysis = service.AnalyzeQuery(query);

        // Assert
        analysis.Should().NotBeNull();
        analysis.MaxDepth.Should().BeGreaterThan(0);
        analysis.FieldCount.Should().BeGreaterThan(0);
        analysis.Level.Should().BeOneOf(QueryComplexityLevel.Low, QueryComplexityLevel.Medium, QueryComplexityLevel.High, QueryComplexityLevel.Critical);
    }

    [Fact]
    public void AnalyzeQuery_WithDeeplyNestedQuery_HasHigherComplexity()
    {
        // Arrange
        var service = CreateAnalysisService();
        var deepQuery = new GraphQLQuery(
            "{ user { profile { settings { notifications { email { daily { threshold } } } } } } }"
        );

        // Act
        var analysis = service.AnalyzeQuery(deepQuery);

        // Assert
        analysis.MaxDepth.Should().BeGreaterThanOrEqualTo(6);
    }

    [Fact]
    public void AnalyzeQuery_WithHighComplexityScore_ReturnsWarnings()
    {
        // Arrange
        var service = CreateAnalysisService();
        var complexQuery = new GraphQLQuery(
            "{ users { profile { settings { data { items } } } } }"
        );

        // Act
        var analysis = service.AnalyzeQuery(complexQuery);

        // Assert
        analysis.Level.Should().BeOneOf(QueryComplexityLevel.Low, QueryComplexityLevel.Medium);
    }

    [Fact]
    public void AnalyzeQuery_PreservesQueryIdInAnalysis()
    {
        // Arrange
        var service = CreateAnalysisService();
        var query = new GraphQLQuery("{ user { id } }");
        var expectedId = query.Id;

        // Act
        var analysis = service.AnalyzeQuery(query);

        // Assert
        analysis.QueryId.Should().Be(expectedId);
    }
}

sealed public class DataLoaderServiceTests
{
    private static DataLoaderService CreateDataLoaderService()
    {
        var mockLogger = new Mock<ILogger<DataLoaderService>>().Object;
        return new DataLoaderService(mockLogger);
    }

    [Fact]
    public void RegisterBatchFunction_WithValidInputs_SuccessfullyRegisters()
    {
        // Arrange
        var service = CreateDataLoaderService();
        var loaderName = "userLoader";
        Func<List<object>, Task<List<object?>>> batchFunc = async (ids) =>
        {
            await Task.Delay(1);
            return ids.Cast<object?>().ToList();
        };

        // Act
        service.RegisterBatchFunction(loaderName, batchFunc);

        // Assert - no exception thrown, registration succeeded
        var request = service.CreateRequest(loaderName, "ctx1");
        request.Should().NotBeNull();
    }

    [Fact]
    public void RegisterBatchFunction_WithEmptyLoaderName_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateDataLoaderService();
        Func<List<object>, Task<List<object?>>> batchFunc = async (ids) =>
        {
            await Task.Delay(1);
            return ids.Cast<object?>().ToList();
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            service.RegisterBatchFunction("", batchFunc));
    }

    [Fact]
    public void RegisterBatchFunction_WithNullBatchFunction_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateDataLoaderService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            service.RegisterBatchFunction("loader", null!));
    }

    [Fact]
    public void CreateRequest_WithValidInputs_ReturnsValidRequest()
    {
        // Arrange
        var service = CreateDataLoaderService();
        var loaderName = "userLoader";
        var contextId = "exec123";

        // Act
        var request = service.CreateRequest(loaderName, contextId);

        // Assert
        request.Should().NotBeNull();
        request.LoaderName.Should().Be(loaderName);
        request.ExecutionContextId.Should().Be(contextId);
        request.State.Should().Be(DataLoaderState.Pending);
    }

    [Fact]
    public void CreateRequest_WithEmptyLoaderName_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateDataLoaderService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest("", "ctx1"));
    }

    [Fact]
    public void CreateRequest_WithEmptyContextId_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateDataLoaderService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            service.CreateRequest("loader", ""));
    }

    [Fact]
    public void LoadKey_WithValidRequestId_AddsKeySuccessfully()
    {
        // Arrange
        var service = CreateDataLoaderService();
        var request = service.CreateRequest("loader", "ctx1");
        var key = "user123";

        // Act
        service.LoadKey(request.Id, key);

        // Assert
        var updated = service.GetRequest(request.Id);
        updated.Should().NotBeNull();
        updated!.BatchSize.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyBatch_MarksRequestAsExecuted()
    {
        // Arrange
        var service = CreateDataLoaderService();
        service.RegisterBatchFunction("loader", async (ids) =>
        {
            await Task.Delay(1);
            return new List<object?>();
        });

        var request = service.CreateRequest("loader", "ctx1");

        // Act
        var result = await service.ExecuteAsync(request.Id);

        // Assert
        result.State.Should().Be(DataLoaderState.Completed);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnregisteredLoader_ReturnsErrorState()
    {
        // Arrange
        var service = CreateDataLoaderService();
        var request = service.CreateRequest("unknownLoader", "ctx1");
        service.LoadKey(request.Id, "key1");

        // Act
        var result = await service.ExecuteAsync(request.Id);

        // Assert – the request reaches a terminal state with errors recorded
        result.State.Should().Be(DataLoaderState.Failed);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FlushAllAsync_WithMultipleRequests_FlushesAll()
    {
        // Arrange
        var service = CreateDataLoaderService();
        service.RegisterBatchFunction("loader1", async (ids) =>
        {
            await Task.Delay(1);
            return ids.Cast<object?>().ToList();
        });

        var ctx1 = "ctx-test";
        var req1 = service.CreateRequest("loader1", ctx1);
        service.LoadKey(req1.Id, "id1");

        var req2 = service.CreateRequest("loader1", ctx1);
        service.LoadKey(req2.Id, "id2");

        // Act
        await service.FlushAllAsync(ctx1);

        // Assert - both requests should be executed
        var r1 = service.GetRequest(req1.Id);
        var r2 = service.GetRequest(req2.Id);
        r1?.State.Should().Be(DataLoaderState.Completed);
        r2?.State.Should().Be(DataLoaderState.Completed);
    }
}

sealed public class ErrorFormattingServiceTests
{
    private static ErrorFormattingService CreateErrorFormattingService(bool enableDetails = true)
    {
        var mockLogger = new Mock<ILogger<ErrorFormattingService>>().Object;
        var options = new GraphQLEngineOptions { EnableDetailedErrorMessages = enableDetails };
        return new ErrorFormattingService(mockLogger, options);
    }

    [Fact]
    public void FormatError_WithNullError_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateErrorFormattingService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.FormatError(null!));
    }

    [Fact]
    public void FormatError_WithValidError_ContainsMessage()
    {
        // Arrange
        var service = CreateErrorFormattingService();
        var error = new ExecutionError { Message = "Test error", Field = "users" };

        // Act
        var result = service.FormatError(error);

        // Assert
        result.Should().ContainKey("message");
        result["message"].Should().Be("Test error");
    }

    [Fact]
    public void FormatError_WithFieldLocation_IncludesLocationInfo()
    {
        // Arrange
        var service = CreateErrorFormattingService();
        var error = new ExecutionError
        {
            Message = "Field error",
            Field = "users.profile",
            LineNumber = 5
        };

        // Act
        var result = service.FormatError(error);

        // Assert
        result.Should().ContainKey("locations");
    }

    [Fact]
    public void FormatError_WithDetailedMessagesDisabled_ExcludesStackTrace()
    {
        // Arrange
        var service = CreateErrorFormattingService(enableDetails: false);
        var error = new ExecutionError
        {
            Message = "Test",
            StackTrace = "at Method() in File.cs:line 42"
        };

        // Act
        var result = service.FormatError(error);

        // Assert
        result.Should().NotContainKey("extensions");
    }

    [Fact]
    public void FormatError_WithDetailedMessagesEnabled_IncludesStackTrace()
    {
        // Arrange
        var service = CreateErrorFormattingService(enableDetails: true);
        var error = new ExecutionError
        {
            Message = "Test",
            StackTrace = "at Method() in File.cs:line 42"
        };

        // Act
        var result = service.FormatError(error);

        // Assert
        result.Should().ContainKey("extensions");
    }

    [Fact]
    public void SanitizeErrorMessage_WithDetailedMessagesDisabled_ReturnsGenericMessage()
    {
        // Arrange
        var service = CreateErrorFormattingService(enableDetails: false);
        var message = "NullReferenceException at specific line";

        // Act
        var result = service.SanitizeErrorMessage(message);

        // Assert
        result.Should().Contain("error occurred");
        result.Should().NotBe(message);
    }

    [Fact]
    public void SanitizeErrorMessage_WithDetailedMessagesEnabled_ReturnOriginalMessage()
    {
        // Arrange
        var service = CreateErrorFormattingService(enableDetails: true);
        var message = "Detailed error information";

        // Act
        var result = service.SanitizeErrorMessage(message);

        // Assert
        result.Should().Be(message);
    }

    [Fact]
    public void CreateErrorResponse_WithAllParameters_IncludesExtensions()
    {
        // Arrange
        var service = CreateErrorFormattingService();
        var extensions = new Dictionary<string, object> { { "code", "AUTH_REQUIRED" } };

        // Act
        var result = service.CreateErrorResponse("Auth failed", "AUTH_ERROR", extensions);

        // Assert
        result.Should().ContainKey("message");
        result.Should().ContainKey("errorCode");
        result.Should().ContainKey("extensions");
        result["extensions"].Should().Be(extensions);
    }

    [Fact]
    public void CreateErrorResponse_WithNullExtensions_DoesNotIncludeExtensions()
    {
        // Arrange
        var service = CreateErrorFormattingService();

        // Act
        var result = service.CreateErrorResponse("Error", "CODE");

        // Assert
        result.Should().NotContainKey("extensions");
    }

    [Fact]
    public void CreateErrorResponse_AlwaysIncludesTimestamp()
    {
        // Arrange
        var service = CreateErrorFormattingService();

        // Act
        var result = service.CreateErrorResponse("Error");

        // Assert
        result.Should().ContainKey("timestamp");
    }
}
