using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace RedRockets.GraphQL;

public static class GraphQLHttpRequestExtensions
{
    public static bool IsQuery(this GraphQLHttpRequest request)
    {
        return !string.IsNullOrEmpty(request.Query);
    }

    public static Dictionary<string, object?> GetVariablesAsDictionary(this GraphQLHttpRequest request)
    {
        return request.Variables ?? new Dictionary<string, object?>();
    }

    public static string GetOperationNameOrDefault(this GraphQLHttpRequest request, string defaultValue = "")
    {
        return request.OperationName ?? defaultValue;
    }
}
