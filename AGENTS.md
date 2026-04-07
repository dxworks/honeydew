# AGENTS Guide for Honeydew
This guide is for autonomous coding agents working in this repository.
It captures build/test commands and style conventions used by the codebase.

## Project Overview
- Stack: C# on .NET 9 (`net9.0`), xUnit, Moq.
- Root solution: `Honeydew.sln`.
- Main CLI project: `Honeydew/Honeydew.csproj`.
- Test projects:
  - `Honeydew.Tests/Honeydew.Tests.csproj`
  - `Honeydew.Extractors.Tests/Honeydew.Extractors.Tests.csproj`
  - `Honeydew.Extractors.CSharp.Tests/Honeydew.Extractors.CSharp.Tests.csproj`
  - `Honeydew.Extractors.VisualBasic.Tests/Honeydew.Extractors.VisualBasic.Tests.csproj`
- CI baseline (`.github/workflows/build-and-test.yml`): restore, build, test on .NET 9.

## Agent Working Rules
1. Prefer minimal, local edits over broad refactors.
2. Keep API and output formats stable unless the task says otherwise.
3. Match existing folder and naming patterns.
4. Preserve nullability behavior (`<Nullable>enable` across projects).
5. Add or update tests when changing extractor logic, model mapping, or exporters.

## Build Commands
Run from repository root.

Restore and build solution:
```bash
dotnet restore Honeydew.sln
dotnet build Honeydew.sln --no-restore
```

Build one project:
```bash
dotnet build Honeydew/Honeydew.csproj --no-restore
```

Release build:
```bash
dotnet build Honeydew.sln -c Release
```

Publish single-file self-contained binary:
```bash
# Windows
dotnet publish Honeydew/Honeydew.csproj -r win-x64 --self-contained true -p:PublishSingleFile=true -c Release -o bin

# Linux
dotnet publish Honeydew/Honeydew.csproj -r linux-x64 --self-contained true -p:PublishSingleFile=true -c Release -o bin
```

## Test Commands
Run all tests:
```bash
dotnet test Honeydew.sln --no-build --verbosity normal
```

Run one test project:
```bash
dotnet test Honeydew.Extractors.CSharp.Tests/Honeydew.Extractors.CSharp.Tests.csproj
```

Run a single test (important for fast iteration):
```bash
dotnet test Honeydew.Extractors.CSharp.Tests/Honeydew.Extractors.CSharp.Tests.csproj --filter "FullyQualifiedName~CSharpClassInfoTests.Extract_ShouldHaveMethods_WhenProvidedWithInterfaceWithImplementedMethods"
```

Run all tests from one class:
```bash
dotnet test Honeydew.Extractors.CSharp.Tests/Honeydew.Extractors.CSharp.Tests.csproj --filter "FullyQualifiedName~CSharpClassInfoTests"
```

Discover test names:
```bash
dotnet test Honeydew.Extractors.CSharp.Tests/Honeydew.Extractors.CSharp.Tests.csproj --list-tests
```

Coverage (collector is configured in test projects):
```bash
dotnet test Honeydew.sln --collect:"XPlat Code Coverage"
```

## Lint / Formatting
There is no dedicated lint workflow in CI.
Use these checks:
```bash
dotnet build Honeydew.sln
dotnet format Honeydew.sln --verify-no-changes
```
If format check fails, run `dotnet format Honeydew.sln`, review changes, then rerun tests.

## End-to-End Validation
CI workflow `end_to_end_test.yml` publishes the CLI, runs extraction against `dxworks/HoneydewTestProject`, and compares output with `e2e_results`.

Typical local flow:
```bash
./bin/Honeydew extract <test_project_folder>
sh e2e_scripts/compare.sh e2e_results ./results
```
`compare.sh` requires `filecomparer.jar` in repo root and Java installed.

## Docs Commands
Docs are MkDocs-based (`mkdocs.yml`):
```bash
pip install mkdocs-material mkdocs-material-extensions mkdocs-mermaid2-plugin
mkdocs serve
mkdocs build
```

## Code Style Guidelines
These are inferred from current source files.

### Imports and namespaces
- Use file-scoped namespaces (`namespace X.Y;`).
- Keep `using` directives at top of file.
- Keep import lists clean; remove unused usings.
- Prefer explicit imports even with implicit usings enabled.

### Formatting and structure
- Use 4-space indentation.
- Use Allman braces (open brace on next line).
- Prefer one public type per file.
- Keep methods focused; extract helpers for long blocks.
- Avoid excessive blank lines.

### Types and nullability
- Respect nullable reference types; model optional values with `?`.
- Use interfaces for boundaries and dependencies (`ILogger`, `IProgressLogger`, etc.).
- Use `var` when the type is obvious from RHS; otherwise use explicit types.
- Initialize collections defensively (`new List<T>()`, etc.).

### Naming conventions
- PascalCase for classes, interfaces, methods, properties, and test methods.
- Interface names start with `I`.
- Private readonly fields use `_camelCase`.
- Test methods follow `Method_ShouldBehavior_WhenCondition`.
- Keep domain names aligned with existing modules: `Extractors`, `Visitors`, `Processors`, `Models`.

### Error handling and logging
- Catch exceptions only where recovery/continuation is intended.
- Log useful context (path, operation, project/language).
- Fail fast for invalid extraction states (custom exceptions are used, e.g., `ExtractionException`).
- Do not swallow exceptions silently.

### Async and parallel behavior
- Propagate `CancellationToken` through async call chains.
- Preserve existing concurrency safeguards when touching parallel extraction logic.
- Prefer deterministic behavior when mutating shared collections.

### Testing practices
- Use xUnit `[Fact]` and `[Theory]`.
- Reuse `FileData`/`MultiFileData` attributes and `TestData` fixtures.
- Assert behavior and output shape rather than implementation details.
- For extractor/output changes, update corresponding fixture files and tests together.

## Cursor and Copilot Rules
Checked locations:
- `.cursor/rules/`
- `.cursorrules`
- `.github/copilot-instructions.md`
No Cursor or Copilot instruction files were found in this repository.

## Suggested Agent Flow
1. Restore and build solution.
2. Make minimal code changes.
3. Run impacted tests (single test or test project first).
4. Run broader test/build checks before finishing.
5. If outputs changed intentionally, validate e2e artifacts.
