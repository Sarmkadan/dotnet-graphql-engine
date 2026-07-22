#!/usr/bin/env python3
"""
A simple helper script to build and run the .NET test suite.

This script is intended to be invoked as:
    python3 /home/redrocket/task-factory/aider_buildcmd.py

It will execute `dotnet test` in the repository root, forwarding the
exit code of the test runner so that CI systems can detect failures.
"""

import subprocess
import sys
import pathlib

def main() -> None:
    # Resolve the directory containing this script – assumed to be the repo root.
    repo_root = pathlib.Path(__file__).resolve().parent

    # Run the dotnet test command in the repository root.
    # The subprocess inherits stdout/stderr so test output is displayed directly.
    try:
        completed = subprocess.run(
            ["dotnet", "test"],
            cwd=repo_root,
            check=False,
        )
    except FileNotFoundError:
        # dotnet CLI is not installed or not on PATH.
        sys.stderr.write("Error: 'dotnet' command not found. Ensure the .NET SDK is installed.\n")
        sys.exit(1)

    # Propagate the test runner's exit code.
    sys.exit(completed.returncode)


if __name__ == "__main__":
    main()
