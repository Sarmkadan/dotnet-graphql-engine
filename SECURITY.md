# Security Policy

## Reporting Security Vulnerabilities

If you discover a security vulnerability in dotnet-graphql-engine, please **DO NOT** create a public GitHub issue. Instead:

1. **Email:** Send details to rutova2@gmail.com
2. **Include:**
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if available)

3. **Timeline:** We will:
   - Acknowledge receipt within 48 hours
   - Work on a fix immediately
   - Release patched version as soon as possible
   - Credit you (if desired) in security bulletin

## Security Best Practices

### For Application Developers

When using dotnet-graphql-engine in production:

#### 1. Query Complexity Limits

Always enforce complexity limits to prevent DoS attacks:

```csharp
services.AddGraphQLEngine(options =>
{
    options.MaxQueryComplexity = 5000;  // Adjust based on your infrastructure
    options.MaxQueryDepth = 10;
    options.MaxQueryFields = 200;
    options.QueryTimeoutMs = 30000;
});
```

#### 2. Authentication & Authorization

```csharp
// Always verify user identity
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "your-auth-authority";
        options.Audience = "your-api";
    });

// Check permissions in resolvers
public async Task<User> GetUserAsync(string id, ExecutionContext context)
{
    var userId = context.GetHeader("X-User-Id");
    var isAdmin = context.GetData("isAdmin") is bool b && b;

    if (userId != id && !isAdmin)
        throw new GraphQLException("Not authorized");

    return await _userService.GetUserAsync(id);
}
```

#### 3. Input Validation

Validate all user input:

```csharp
public async Task<User> CreateUserAsync(string name, string email, ExecutionContext context)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(name) || name.Length > 255)
        throw new GraphQLException("Invalid name");

    if (!IsValidEmail(email))
        throw new GraphQLException("Invalid email");

    return await _userService.CreateAsync(name, email);
}
```

#### 4. Error Messages

Never expose sensitive information in error messages:

```csharp
// Bad: Leaks internal details
throw new GraphQLException($"User 'alice@example.com' not found in database");

// Good: Generic message
throw new GraphQLException("User not found");
```

#### 5. Secrets Management

Never hardcode secrets:

```csharp
// Bad
string apiKey = "sk_live_123456789";
string dbPassword = "Password123!";

// Good: Use secrets manager
var apiKey = builder.Configuration["ApiKey"];  // From environment or Azure Key Vault
var dbPassword = builder.Configuration["ConnectionStrings:Default"];
```

#### 6. HTTPS/TLS

Always use HTTPS in production:

```csharp
// Configure HTTPS
services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

app.UseHttpsRedirection();
```

#### 7. CORS Configuration

Configure CORS restrictively:

```csharp
services.AddCors(options =>
{
    options.AddPolicy("SpecificOrigins", policy =>
    {
        policy.WithOrigins("https://trusted-domain.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors("SpecificOrigins");
```

#### 8. Rate Limiting

Implement rate limiting:

```csharp
services.AddRateLimiting(options =>
{
    options.GlobalLimitPolicy = "default";
    options.AddPolicy("default", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

#### 9. Logging

Log security events but don't log sensitive data:

```csharp
// Bad: Logs sensitive data
_logger.LogInformation("User authenticated: {Email} with password {Password}", email, password);

// Good: Logs only necessary info
_logger.LogInformation("User authenticated: {Email}", email);
_logger.LogWarning("Failed authentication attempt from {IpAddress}", ipAddress);
```

#### 10. Dependency Management

Keep dependencies up to date:

```bash
# Check for vulnerabilities
dotnet list package --vulnerable

# Update vulnerable packages
dotnet package update package-name

# In CI/CD pipeline, fail on critical vulnerabilities
dotnet list package --vulnerable --include-transitive
```

### Known Security Considerations

#### Query Complexity Scoring

The complexity analysis uses a scoring system. Be aware:

- Simple fields: 1 point each
- Array fields: Multiplied by estimated size
- Nested objects: Accumulated complexity

Tune limits based on your infrastructure:
```csharp
// Permissive (development)
options.MaxQueryComplexity = 10000;

// Balanced (production)
options.MaxQueryComplexity = 5000;

// Strict (high-security)
options.MaxQueryComplexity = 2000;
```

#### Error Information Disclosure

In production, sanitize error messages:

```csharp
services.AddGraphQLEngine(options =>
{
    options.IncludeDetailedErrorMessages = false;  // Don't expose internals
    options.LogInternalErrors = true;               // Log for debugging
});
```

#### Authentication Bypass

Always enforce authentication:

```csharp
// Don't expose sensitive queries without authentication
app.UseAuthentication();
app.UseAuthorization();

// Require auth for all GraphQL endpoints
app.MapGraphQL().RequireAuthorization();
```

#### Schema Enumeration

Control who can query the schema:

```csharp
services.AddGraphQLEngine(options =>
{
    // Only allow authenticated users to introspect
    options.EnableSchemaIntrospection = true;  // Set to false to disable
});
```

## Security Checklist for Production

- [ ] HTTPS/TLS configured
- [ ] Authentication enabled and verified
- [ ] Authorization checks in all resolvers
- [ ] Query complexity limits enforced
- [ ] Rate limiting enabled
- [ ] CORS configured restrictively
- [ ] Secrets not hardcoded
- [ ] Dependencies updated and checked for vulnerabilities
- [ ] Error messages sanitized
- [ ] Security logging enabled
- [ ] Database connections secured
- [ ] Input validation implemented
- [ ] API keys rotated regularly
- [ ] Security headers configured (HSTS, CSP, etc.)
- [ ] Database backups tested
- [ ] Monitoring and alerting configured
- [ ] Security audit completed
- [ ] Penetration testing performed (recommended)

## Security Headers

Add security headers to responses:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    await next();
});
```

## Dependency Vulnerability Scanning

Use automated scanning in CI/CD:

```yaml
# .github/workflows/security.yml
name: Security Scan
on: [push, pull_request]
jobs:
  security:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
    - run: dotnet list package --vulnerable
    - run: dotnet tool install -g dotnet-sonarscanner
```

## Responsible Disclosure Timeline

We follow responsible disclosure:

1. **Day 0:** Vulnerability reported
2. **Day 1:** Acknowledge receipt and begin investigation
3. **Day 7:** Patch developed and tested
4. **Day 14:** Security release published
5. **Day 21:** Public disclosure

## Additional Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [GraphQL Security](https://cheatsheetseries.owasp.org/cheatsheets/GraphQL_Cheat_Sheet.html)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/security/)

## Version Support

| Version | Status | End of Support |
|---------|--------|-----------------|
| 1.2.x | Supported | 2027-05-04 |
| 1.1.x | Security fixes only | 2026-12-31 |
| 1.0.x | End of life | 2026-06-30 |
| 0.9.x | End of life | 2026-03-01 |

## Contact

Security concerns: rutova2@gmail.com

---

**Last updated:** May 4, 2026

For the latest security information, visit the [GitHub Security Advisories](https://github.com/vladyslavzaiets/dotnet-graphql-engine/security/advisories)
