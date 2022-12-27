# Contributing to dotnet-graphql-engine

Thank you for considering contributing to dotnet-graphql-engine!

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- A code editor with C# support (Visual Studio, VS Code with C# Dev Kit, or JetBrains Rider)

## Building Locally

```bash
# Clone the repository
git clone https://github.com/sarmkadan/dotnet-graphql-engine.git
cd dotnet-graphql-engine

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# (Optional) Publish
dotnet publish -c Release -o ./publish
```

## Running Tests

```bash
# Run all tests
dotnet test --verbosity normal

# Run tests with TRX output (for CI-style reporting)
dotnet test --verbosity normal --logger "trx" --results-directory TestResults

# Run a specific test project
dotnet test tests/dotnet-graphql-engine.Tests/dotnet-graphql-engine.Tests.csproj
```

## Running with Docker

```bash
# Build the image
docker build -t dotnet-graphql-engine .

# Or use the compose file
docker compose up
```

## Workflow

1. **Fork** the repository.
2. **Create a branch** from `main` using a descriptive name:
   - `feature/your-feature-name` for new features
   - `fix/issue-description` for bug fixes
   - `docs/update-description` for documentation changes
3. Make your changes, keeping commits focused and atomic.
4. **Run tests** and ensure they all pass before opening a PR.
5. **Open a Pull Request** targeting `main` with a clear title and description.

## Pull Request Guidelines

- Keep PRs focused — one concern per PR.
- Reference any related issues in the PR description.
- Ensure all CI checks pass before requesting review.
- Add or update tests when changing behaviour.
- Update documentation if the public API changes.

## Code Style

- Follow the formatting rules defined in [`.editorconfig`](.editorconfig).
- Use `var` only when the type is apparent from the right-hand side.
- Provide XML documentation comments for all public types and members.
- Prefer expression bodies for simple one-liners; use block bodies for multi-statement methods.
- Run `dotnet format` before committing to ensure consistent formatting:
  ```bash
  dotnet format
  ```

## Reporting Issues

Use [GitHub Issues](https://github.com/sarmkadan/dotnet-graphql-engine/issues) to report bugs or request features. When reporting a bug, include:

- .NET version (`dotnet --version`)
- Steps to reproduce
- Expected vs actual behaviour
- Any relevant stack traces or error messages

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).

