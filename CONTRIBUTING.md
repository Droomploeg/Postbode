# Contributing to DreamOps

Thank you for your interest in contributing to DreamOps! This document explains how to contribute effectively.

By submitting a pull request, you agree to the terms of the [Contributor License Agreement (CLA)](CLA.md).

## Code of Conduct

This project adheres to the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Report unacceptable behavior to mark@droomploeg.nl.

## Ways to Contribute

- **Report bugs** — open an issue using the bug report template
- **Request features** — open an issue using the feature request template
- **Improve documentation** — fixes and clarifications are always welcome
- **Submit code** — bug fixes, new features, or refactors via pull request
- **Report security issues** — see [SECURITY.md](SECURITY.md), please do **not** open a public issue

## Development Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- Docker (for Aspire-based integration tests)
- Azure subscription with a Service Bus namespace (for end-to-end testing)

### Build and Test

```bash
dotnet restore
dotnet build
dotnet test
```

For local development with Aspire orchestration:

```bash
dotnet run --project src/Droomploeg.DreamOps.AppHost
```

## Pull Request Process

1. **Fork** the repository and create a branch from `main`.
2. **Branch naming**: `feature/<short-name>` for new features, `bugfix/<short-name>` for fixes.
3. **Make your changes** following the architecture and code style described below.
4. **Add or update tests** — see the testing guidelines below.
5. **Verify** that `dotnet build` and `dotnet test` succeed.
6. **Commit** with a clear, imperative-mood message (see commit conventions below).
7. **Open a pull request** against `main` and complete the PR template checklist.

A maintainer will review your PR. Reviews focus on: architecture, code quality, security, and tests.

## Architecture Guidelines

DreamOps follows **Clean Architecture** with strict layer separation. Dependencies flow inward only.

| Layer | Project | Responsibility |
|-------|---------|----------------|
| WebApp | `Droomploeg.DreamOps.WebApp` | Blazor components, DI, security config |
| Application | `Droomploeg.DreamOps.Application` | Interfaces / contracts only |
| Core/Domain | `Droomploeg.DreamOps.Core` | Pure domain models |
| Infrastructure | `Droomploeg.DreamOps.Infrastructure` | Azure Service Bus adapters, audit, workers |

Key rules:

- **No implementations in Application** — interfaces only
- **No business logic in WebApp** — Blazor components delegate to services
- **No direct Azure Service Bus SDK calls from services** — go through adapters
- **No infrastructure provisioning** — queue/topic creation belongs in Bicep
- **All message operations must be audit-logged** via `IAuditLogger`
- **Long-running operations** use the worker/dispatcher pattern, not inline
- **User actions** use the On-Behalf-Of token flow; service actions use Managed Identity

## Code Style

- Follow [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Settings live in `.editorconfig` — make sure your editor respects it
- Async methods are suffixed with `Async`
- Private fields use `_camelCase`
- Prefer Blazor Server capabilities over JavaScript

### Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Mappers | `[Source]Mapper` (static) | `QueueMapper` |
| Constants | `[Domain]Constants` | `ServiceBusConstants` |
| Blazor Pages | `[Name]Page` with code-behind `.razor.cs` | `OverviewPage` |
| Blazor Controls | `[Name]Control` | `DialogControl` |
| Config Options | `[Service]Connection` / `[Service]Options` | `AzureServiceBusConnection` |

Namespaces follow `Droomploeg.DreamOps.[Layer].[Feature].[Category]`.

## Commit Conventions

Use imperative mood with one of these prefixes:

| Prefix | Usage |
|--------|-------|
| `Fix` | Bug fixes |
| `Add` | New features |
| `Refactor` | Code restructuring without behavior change |
| `Update` | Dependency or version updates |
| `Change` | Behavioral changes |
| `Remove` | Removing code/features |

Examples:

- `Fix login message when logged out`
- `Add audit for background tasks`
- `Refactor service account and user account`

Rules:

- Imperative mood: "Fix", not "Fixed" or "Fixes"
- First line under 72 characters
- No conventional-commits prefix format (no `feat:`, `fix:`, etc.)

## Testing Guidelines

DreamOps uses **functional/integration tests**, not unit tests. We test what a user can do via a page against a real Service Bus instance.

- Tests live in `tests/Droomploeg.DreamOps.Aspire.Tests`
- Tests spin up real dependencies (Service Bus emulator, Application Insights) via .NET Aspire
- Use standard xUnit `Assert` methods — do **not** introduce FluentAssertions
- Do not mock the Service Bus — always use a real instance through Aspire
- Do not write unit tests for individual classes (mappers, helpers, extensions)

## Questions?

Open a discussion on GitHub or contact the maintainers at mark@droomploeg.nl.
