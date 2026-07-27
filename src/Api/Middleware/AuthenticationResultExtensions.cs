#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GraphQLEngine.Exceptions;

namespace GraphQLEngine.Api.Middleware;

/// <summary>
/// Extension methods for <see cref="AuthenticationResult"/>.
/// </summary>
public static class AuthenticationResultExtensions
{
    /// <summary>
    /// Executes one of the supplied functions depending on whether the authentication succeeded.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="result">The authentication result.</param>
    /// <param name="ok">Function to invoke when authentication succeeded. Receives an <see cref="AuthenticationContext"/>.</param>
    /// <param name="failed">Function to invoke when authentication failed. Receives the error message.</param>
    /// <returns>The value returned by the selected function.</returns>
    public static T Match<T>(this AuthenticationResult result,
                             Func<AuthenticationContext, T> ok,
                             Func<string, T> failed)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (ok == null) throw new ArgumentNullException(nameof(ok));
        if (failed == null) throw new ArgumentNullException(nameof(failed));

        if (result.Success)
        {
            var context = new AuthenticationContext
            {
                IsAuthenticated = true,
                Principal = result.Principal,
                UserId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Roles = result.Principal?.FindAll(ClaimTypes.Role)
                                         .Select(c => c.Value)
                                         .ToList() ?? new List<string>(),
                Metadata = new Dictionary<string, string>()
            };
            return ok(context);
        }

        return failed(result.Error ?? "Authentication failed");
    }

    /// <summary>
    /// Throws a <see cref="GraphQLException"/> if the authentication result indicates failure.
    /// </summary>
    /// <param name="result">The authentication result.</param>
    public static void ThrowIfFailed(this AuthenticationResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        if (!result.Success)
        {
            throw new GraphQLException(result.Error ?? "Authentication failed");
        }
    }

    /// <summary>
    /// Indicates whether the request was authenticated.
    /// </summary>
    /// <param name="result">The authentication result.</param>
    /// <returns>True if authentication succeeded; otherwise false.</returns>
    public static bool IsAuthenticated(this AuthenticationResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        return result.Success;
    }
}
