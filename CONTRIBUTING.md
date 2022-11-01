# Contributing to dotnet-graphql-engine

Thank you for your interest in contributing to dotnet-graphql-engine! This document provides guidelines and instructions for contributing.

## Code of Conduct

Be respectful, inclusive, and professional. We're building a welcoming community for all developers.

## How to Contribute

### Reporting Bugs

1. Check if bug is already reported in [Issues](https://github.com/vladyslavzaiets/dotnet-graphql-engine/issues)
2. Create a new issue with:
   - Clear title describing the bug
   - Detailed description of the problem
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment (OS, .NET version, etc.)
   - Code example if applicable

### Requesting Features

1. Check [GitHub Discussions](https://github.com/vladyslavzaiets/dotnet-graphql-engine/discussions) for similar requests
2. Create a discussion or issue with:
   - Clear title describing the feature
   - Motivation and use case
   - Proposed solution if you have one
   - Alternative approaches considered

### Submitting Code

1. **Fork the repository**
   ```bash
   git clone https://github.com/YOUR-USERNAME/dotnet-graphql-engine.git
   cd dotnet-graphql-engine
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Follow the code style guidelines (see below)
   - Write clear, descriptive commit messages
   - Add tests for new functionality
   - Update documentation if needed

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Format code**
   ```bash
   dotnet format
   make format
   ```

6. **Push and create Pull Request**
   ```bash
   git push origin feature/your-feature-name
   ```

## Development Setup

### Prerequisites
- .NET 10 SDK
- Git
- Text editor or IDE (Visual Studio, VS Code, Rider recommended)

### Build

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Build release version
dotnet build -c Release

# Run all tests
dotnet test

# Run specific test
dotnet test --filter "TestClassName"
```

### Common Tasks

```bash
# Format code
make format

# Run linting
make lint

# Generate documentation
make docs

# Run benchmarks
make benchmark

# Docker build
make docker-build

# Docker run
make docker-run
```

## Code Style Guide

### General Rules

- **Language Version:** Use latest C# features (C# 14 for .NET 10)
- **Null References:** Use `#nullable enable` in all files
- **Implicit Usings:** Leverage implicit usings for cleaner code
- **Max Line Length:** Keep lines under 120 characters
- **Max Method Length:** Keep methods under 30 lines
- **Indentation:** 4 spaces (never tabs)

### Naming Conventions

```csharp
// Classes and public members: PascalCase
public class UserService
{
    public string UserName { get; set; }
    public async Task<User> GetUserAsync(string id) { }
}

// Private members and local variables: camelCase
private string _connectionString;
private int _maxRetries;
var userName = "john";

// Constants: UPPER_CASE
private const int DEFAULT_TIMEOUT = 30000;

// Interfaces: IPrefixPascalCase
public interface IUserRepository { }
public interface IGraphQLService { }
```

### File Structure

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Sarmkadan.GraphQLEngine.Services;

/// <summary>
/// Brief description of the class.
/// </summary>
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Method documentation.
    /// </summary>
    /// <param name="id">Parameter description</param>
    /// <returns>Return value description</returns>
    public async Task<string> GetItemAsync(string id)
    {
        // Implementation
        return "result";
    }
}
```

### Comments

- **Default:** No comments unless absolutely necessary
- **Why:** Code should be self-documenting
- **When:** Comments explain WHY, not WHAT
- **Examples:**
  ```csharp
  // Bad: Explains what the code does
  i++;  // Increment i

  // Good: Explains why or documents non-obvious behavior
  // Retry count must start at 1, not 0, due to API requirement (see issue #123)
  int retryCount = 1;

  // Bad: Over-commented
  public class User
  {
      // The user's ID
      public string Id { get; set; }
      // The user's name
      public string Name { get; set; }
  }

  // Good: Self-documenting
  public class User
  {
      public string Id { get; set; }
      public string Name { get; set; }
  }
  ```

### Error Handling

- Use specific exception types
- Include context in exception messages
- Avoid swallowing exceptions silently

```csharp
// Bad
try { /* code */ } catch { }

// Good
try
{
    // code
}
catch (DbException ex)
{
    _logger.LogError(ex, "Failed to load user {UserId}", userId);
    throw new GraphQLException("User lookup failed", ex);
}
```

### Async/Await

- Always use `async`/`await` for I/O operations
- Avoid blocking calls (`Result`, `Wait`)
- Use `ConfigureAwait(false)` in library code

```csharp
// Bad
var user = userService.GetUserAsync(id).Result;

// Good
var user = await userService.GetUserAsync(id);
```

### LINQ

- Use method syntax consistently
- Chain LINQ operations for clarity

```csharp
// Bad: Mixed styles
var result = users.Where(u => u.Active)
    .Select(u => u.Name)
    .OrderByDescending(n => n);

// Good: Consistent style
var result = users
    .Where(u => u.Active)
    .Select(u => u.Name)
    .OrderByDescending(n => n)
    .ToList();
```

## Testing Guidelines

### Test Structure

```csharp
[TestFixture]
public class UserServiceTests
{
    private UserService _service;
    private Mock<IUserRepository> _mockRepository;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IUserRepository>();
        _service = new UserService(_mockRepository.Object);
    }

    [Test]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = "user-123";
        var expectedUser = new User { Id = userId, Name = "John" };
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _service.GetUserAsync(userId);

        // Assert
        Assert.That(result, Is.EqualTo(expectedUser));
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetUser_WithInvalidId_ThrowsException()
    {
        // Arrange
        var userId = "invalid";
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        Assert.ThrowsAsync<GraphQLException>(
            async () => await _service.GetUserAsync(userId));
    }
}
```

### Test Naming

- Use `Method_Scenario_ExpectedResult` pattern
- Be descriptive and specific
- Make tests independent and repeatable

## Commit Messages

Use clear, descriptive commit messages:

```
feat: add DataLoader batch operations

- Implement batch function registration
- Add automatic batching within execution scope
- Prevent N+1 query problems
- Includes unit tests and documentation

Fixes #123
```

### Types
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation
- `style:` - Code style (formatting, etc.)
- `refactor:` - Code refactoring
- `test:` - Test additions/fixes
- `perf:` - Performance improvements
- `chore:` - Build, dependencies, etc.

## Pull Request Process

1. **Title:** Use same format as commit messages
2. **Description:** Explain what and why
3. **Testing:** Include test instructions
4. **Performance:** Note any performance implications
5. **Documentation:** Update docs if needed

### PR Template

```markdown
## Description
Brief description of changes

## Motivation
Why are these changes needed?

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation

## Testing
How to test the changes:
1. ...
2. ...

## Performance
Any performance implications?

## Checklist
- [ ] Code follows style guidelines
- [ ] Tests pass locally
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Changes reviewed by others
```

## Review Process

All pull requests are reviewed by maintainers. We look for:
- Code quality and style adherence
- Test coverage
- Documentation completeness
- Performance implications
- Security considerations

## Versioning

We follow [Semantic Versioning](https://semver.org/):
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes (backward compatible)

## License

By contributing, you agree to license your contributions under the MIT License.

## Questions?

- **Email:** rutova2@gmail.com
- **Issues:** Open a GitHub issue for questions
- **Discussions:** Use GitHub Discussions for general topics

## Thank You!

Your contributions make dotnet-graphql-engine better for everyone!
