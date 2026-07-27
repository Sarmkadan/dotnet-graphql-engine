#!/usr/bin/env bash
# =============================================================================
# Build script for the GraphQL Engine solution
# =============================================================================
# This script restores NuGet packages, builds the solution, and runs all tests.
# It is invoked by the task-factory's build command (aider_buildcmd.py).
# =============================================================================

set -euo pipefail

# Restore NuGet packages for all projects
dotnet restore

# Build the solution in Release configuration
dotnet build --configuration Release

# Run all unit tests (they will be built automatically if needed)
dotnet test --no-build --configuration Release
