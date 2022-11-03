# =============================================================================
# Makefile for dotnet-graphql-engine
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build test clean run publish docker-build docker-run docker-stop docker-compose-up docker-compose-down format lint docs

# Variables
PROJECT_NAME = dotnet-graphql-engine
CONFIGURATION = Release
DOTNET = dotnet
DOCKER = docker
DOCKER_REGISTRY = vladyslavzaiets
DOCKER_IMAGE = $(DOCKER_REGISTRY)/$(PROJECT_NAME)
DOCKER_TAG = latest

# Default target
help:
	@echo "$(PROJECT_NAME) - Build and Development Tasks"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@echo "  build              Build the project"
	@echo "  test               Run unit tests"
	@echo "  clean              Clean build artifacts"
	@echo "  run                Run the application"
	@echo "  publish            Publish release build"
	@echo "  format             Format code with dotnet-format"
	@echo "  lint               Run code analysis"
	@echo "  docs               Generate documentation"
	@echo "  docker-build       Build Docker image"
	@echo "  docker-run         Run Docker container"
	@echo "  docker-stop        Stop Docker container"
	@echo "  docker-compose-up  Start full stack with docker-compose"
	@echo "  docker-compose-down Stop full stack"
	@echo "  help               Show this help message"
	@echo ""

# Build
build:
	@echo "Building $(PROJECT_NAME)..."
	$(DOTNET) build -c $(CONFIGURATION)
	@echo "✓ Build complete"

# Test
test: build
	@echo "Running tests..."
	$(DOTNET) test -c $(CONFIGURATION) --no-build --verbosity normal
	@echo "✓ Tests complete"

# Clean
clean:
	@echo "Cleaning build artifacts..."
	$(DOTNET) clean -c $(CONFIGURATION)
	rm -rf bin/ obj/ publish/
	@echo "✓ Clean complete"

# Run
run: build
	@echo "Starting $(PROJECT_NAME)..."
	$(DOTNET) run -c $(CONFIGURATION)

# Publish
publish: clean
	@echo "Publishing $(PROJECT_NAME)..."
	$(DOTNET) publish -c $(CONFIGURATION) -o ./publish
	@echo "✓ Publish complete"
	@echo "  Output directory: ./publish"

# Format code
format:
	@echo "Formatting code..."
	$(DOTNET) format
	@echo "✓ Format complete"

# Lint
lint:
	@echo "Running code analysis..."
	$(DOTNET) build -c $(CONFIGURATION) -warnaserror
	@echo "✓ Lint complete"

# Generate documentation
docs:
	@echo "Generating documentation..."
	@echo "Documentation is available in the docs/ directory"
	@echo "  - README.md - Main documentation"
	@echo "  - docs/getting-started.md - Getting started guide"
	@echo "  - docs/architecture.md - Architecture guide"
	@echo "  - docs/api-reference.md - API reference"
	@echo "  - docs/deployment.md - Deployment guide"
	@echo "  - docs/faq.md - Frequently asked questions"

# Docker: Build image
docker-build: publish
	@echo "Building Docker image: $(DOCKER_IMAGE):$(DOCKER_TAG)"
	$(DOCKER) build -t $(DOCKER_IMAGE):$(DOCKER_TAG) .
	$(DOCKER) tag $(DOCKER_IMAGE):$(DOCKER_TAG) $(DOCKER_IMAGE):latest
	@echo "✓ Docker image built"
	@echo "  Image: $(DOCKER_IMAGE):$(DOCKER_TAG)"

# Docker: Run container
docker-run:
	@echo "Running Docker container..."
	$(DOCKER) run -d \
		--name $(PROJECT_NAME) \
		-p 5000:5000 \
		$(DOCKER_IMAGE):$(DOCKER_TAG)
	@echo "✓ Container running"
	@echo "  Name: $(PROJECT_NAME)"
	@echo "  URL: http://localhost:5000"

# Docker: Stop container
docker-stop:
	@echo "Stopping Docker container..."
	$(DOCKER) stop $(PROJECT_NAME) || true
	$(DOCKER) rm $(PROJECT_NAME) || true
	@echo "✓ Container stopped"

# Docker Compose: Start stack
docker-compose-up:
	@echo "Starting Docker Compose stack..."
	docker-compose up -d
	@echo "✓ Stack started"
	@echo "  GraphQL Engine: http://localhost:5000"
	@echo "  Redis: localhost:6379"
	@echo "  PostgreSQL: localhost:5432"
	@echo ""
	@echo "View logs: docker-compose logs -f"

# Docker Compose: Stop stack
docker-compose-down:
	@echo "Stopping Docker Compose stack..."
	docker-compose down
	@echo "✓ Stack stopped"

# Advanced targets
.PHONY: ci-build coverage security-scan benchmark

# CI Build (for CI/CD pipeline)
ci-build: clean build test lint
	@echo "✓ CI build complete"

# Code coverage
coverage:
	@echo "Generating code coverage report..."
	$(DOTNET) test -c $(CONFIGURATION) --no-build /p:CollectCoverage=true /p:CoverageFormat=opencover
	@echo "✓ Coverage report generated"

# Security scan
security-scan:
	@echo "Running security scan..."
	$(DOTNET) list package --vulnerable
	@echo "✓ Security scan complete"

# Benchmarks
benchmark:
	@echo "Running performance benchmarks..."
	$(DOTNET) test -c $(CONFIGURATION) --filter "Category=Benchmark"
	@echo "✓ Benchmarks complete"
