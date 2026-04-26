# Claude Code Instructions for DreamOps

## Project Overview

DreamOps is an Azure Service Bus operational management tool built with .NET Blazor Server. It focuses on **message management** (not infrastructure provisioning — that is handled via Bicep IaC). Users can monitor queues/topics, send/delete messages, manage dead-letter queues, and resubmit messages. All user actions on messages are audit-logged.

- **Framework**: .NET 10.0, C# latest, Blazor Server (Interactive Server)
- **Authentication**: Microsoft Entra ID (OpenID Connect + On-Behalf-Of token flow)
- **Infrastructure**: Azure (App Service, Service Bus, Key Vault, Application Insights)
- **IaC**: Bicep (in `/bicep/`)
- **CI/CD**: Azure Pipelines (in `/pipelines/`)

## Architecture

Clean Architecture with 4 layers. Dependencies flow inward only.

```
WebApp (Presentation - Blazor Server + Background Service)
    |
Application (Interfaces / Contracts only)
    |
Core/Domain (Models, Records, Value Types - no dependencies)
    |
Infrastructure (Implementations - Azure Service Bus, Audit, Workers)
```

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **WebApp** | `Droomploeg.DreamOps.WebApp` | Blazor components, pages, DI setup, security config, `WorkerHostedService` |
| **Application** | `Droomploeg.DreamOps.Application` | Interfaces only: service contracts, adapter contracts, factory contracts |
| **Core/Domain** | `Droomploeg.DreamOps.Core` | Pure domain models (`Queue`, `Topic`, `Subscription`, `WorkerItem`), value types, enums |
| **Infrastructure** | `Droomploeg.DreamOps.Infrastructure` | Azure Service Bus adapters/services, audit logging, worker service, context management |

### Additional Projects

- **AppHost** (`Droomploeg.DreamOps.AppHost`): .NET Aspire orchestration (development)
- **Tests**: `tests/Droomploeg.DreamOps.Aspire.Tests/` (Aspire), `tests/Droomploeg.DreamOps.IntegrationsTests/` (bUnit + xUnit)

## Key Architectural Patterns

### Adapter Pattern
Azure Service Bus operations are abstracted behind adapter interfaces (`IActiveQueueAdapter`, `IDeadLetterQueueAdapter`, `IActiveTopicAdapter`, `IDeadLetterTopicAdapter`, `IRuntimeInfoAdapter`). Implementations live in Infrastructure.

### Factory Pattern (AdapterFactory)
`AdapterFactory<T>` creates adapter instances in two modes:
- **OnBehalfOf**: User context — uses the logged-in user's delegated token
- **ServiceAccount**: Elevated permissions — uses managed identity

### Worker / Background Service Pattern
Long-running user actions are dispatched to a background service:
1. `WorkerDispatcher` queues a `WorkerItem` via `IWorkerService`
2. `WorkerHostedService` (ASP.NET Core `BackgroundService`) polls every 1 second
3. `WorkerItem` has a state machine: `Scheduled` -> `Started` -> `Completed|Failed|Cancelled`
4. `NotificationService` monitors for updates and surfaces popup notifications in the UI

Worker state is in-memory with lock-based thread safety.

### Context Pattern
`ApplicationContext` is a scoped service holding per-request state: `CorrelationId`, `UserName`, `CurrentConnection`. `WebContextSetter` reads from `ProtectedSessionStorage` and `AuthenticationStateProvider`.

### Audit Logging
All user actions on messages are audit-logged via `IAuditLogger`. Dual output: Application Insights (structured EventTelemetry) + ILogger fallback. Each audit entry includes: UserName, Action, Resource, CorrelationId, Details, Timestamp.

## Build Commands

```bash
# Restore and build
dotnet build

# Run the web application
dotnet run --project src/Droomploeg.DreamOps.WebApp

# Run tests
dotnet test

# Run the AppHost (Aspire orchestration)
dotnet run --project src/Droomploeg.DreamOps.AppHost
```

## Project Structure

```
dreamops/
├── src/
│   ├── Droomploeg.DreamOps.WebApp/          # Blazor Server app + BackgroundService
│   │   ├── Components/                       # Blazor components (Layout, Controls, Pages)
│   │   ├── Configurations/                   # DI extensions (Security, ServiceBus, Workers)
│   │   ├── Security/                         # OnBehalfOfTokenCredential, route mapping
│   │   ├── HostedServices/                   # WorkerHostedService
│   │   └── Program.cs                        # App entry point, DI composition root
│   ├── Droomploeg.DreamOps.Application/      # Interfaces only
│   │   ├── ServiceBus/Adapters/              # Adapter contracts
│   │   ├── ServiceBus/Services/              # Service contracts
│   │   ├── ServiceBus/Factories/             # Factory contracts
│   │   └── Workers/                          # Worker + Notification contracts
│   ├── Droomploeg.DreamOps.Core/             # Domain models
│   │   ├── ServiceBus/Models/                # Queue, Topic, Subscription, EntityRuntimeInfo
│   │   ├── ServiceBus/Types/                 # Value types, enums
│   │   └── Workers/                          # WorkerItem, WorkerEvent, WorkerAction
│   ├── Droomploeg.DreamOps.Infrastructure/   # Implementations
│   │   ├── AzureServiceBus/Adapters/         # Service Bus adapter implementations
│   │   ├── AzureServiceBus/Services/         # Service implementations
│   │   ├── AzureServiceBus/Factories/        # AdapterFactory
│   │   ├── AzureServiceBus/Mappers/          # Domain model mappers
│   │   ├── AzureServiceBus/Extensions/       # Client factory, message helpers
│   │   ├── Contexts/                         # ApplicationContext, WebContextSetter
│   │   ├── Workers/                          # WorkerService, NotificationService, Dispatcher
│   │   └── Audit/                            # AuditLogger
│   └── Droomploeg.DreamOps.AppHost/          # Aspire orchestration
├── tests/
│   ├── Droomploeg.DreamOps.Aspire.Tests/     # Aspire integration tests
│   └── Droomploeg.DreamOps.IntegrationsTests/ # bUnit component tests
├── bicep/                                     # Azure infrastructure (IaC)
├── pipelines/                                 # Azure Pipelines CI/CD
├── build.props                                # Shared build properties (net10.0)
├── droomploeg.props                           # License metadata (AGPL-3.0)
├── global.json                                # .NET SDK version
└── Droomploeg.DreamOps.slnx                   # Solution file
```

## Design Principles

1. **Clean Architecture** — strict layer separation, dependencies flow inward
2. **Interfaces in Application layer** — all contracts live here, implementations in Infrastructure
3. **No infrastructure provisioning** — DreamOps manages messages only; queue/topic creation is done via Bicep
4. **On-Behalf-Of flow** — user actions execute with the user's delegated Azure token
5. **Background processing** — long-running operations go through the worker pattern, not inline
6. **Audit everything** — all message operations are logged with correlation IDs
7. **Minimal JavaScript** — Blazor Server with server-side interactivity; avoid JS where possible

## Authentication Flow

1. User logs in via Entra ID (OpenID Connect)
2. App acquires delegated token via `ITokenAcquisition` (MSAL)
3. `OnBehalfOfTokenCredential` wraps the token for Azure Service Bus SDK
4. Service account operations use `DefaultAzureCredential` with managed identity

## Naming Conventions

Follow the [Microsoft C# naming conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) unless stated otherwise below.

### Project-Specific Naming

| Type | Pattern | Example |
|------|---------|---------|
| Mappers | `[Source]Mapper` (static class, static `Map()` methods) | `QueueMapper`, `TopicMapper` |
| Constants | `[Domain]Constants` | `ApplicationConstants`, `ServiceBusConstants` |
| Blazor Pages | `[Name]Page` with code-behind `.razor.cs` | `BackgroundJobsPage`, `OverviewPage` |
| Blazor Controls | `[Name]Control` | `DialogControl`, `GridControl<TItem>` |
| Config Options | `[Service]Connection` / `[Service]Options` | `AzureServiceBusConnection` |

Pages use route constants: `@attribute [Route(PageConstants.HomePage)]`

### Namespaces

Pattern: `Droomploeg.DreamOps.[Layer].[Feature].[Category]`

Examples:
- `Droomploeg.DreamOps.Application.ServiceBus.Services`
- `Droomploeg.DreamOps.Infrastructure.AzureServiceBus.Mappers`
- `Droomploeg.DreamOps.Domain.ServiceBus.Models`
- `Droomploeg.DreamOps.WebApp.Components.Controls.Forms`

### Code Style

- See `.editorconfig` for full formatting rules

## Commit Conventions

Commit messages follow a simple imperative pattern:

| Prefix | Usage | Example |
|--------|-------|---------|
| `Fix` | Bug fixes | `Fix login message when logged out` |
| `Add` | New features | `Add audit for background tasks` |
| `Refactor` | Code restructuring | `Refactor service account and user account` |
| `Update` | Dependency/version updates | `Update nuget packages` |
| `Change` | Behavioral changes | `Change access identifier from internal to private` |
| `Remove` | Removing code/features | `Remove obsolete middleware` |

Rules:
- Use imperative mood ("Fix", not "Fixed" or "Fixes")
- Keep the first line concise (under 72 characters)
- No conventional commits prefix format (no `feat:`, `fix:`, etc.)
- Branch naming: `feature/[name]`, `bugfix/[name]`

## PR Review Checklist

Before merging a pull request, verify:

### Architecture
- [ ] Layer boundaries respected (no implementations in Application, no business logic in WebApp)
- [ ] New interfaces defined in Application layer, implementations in Infrastructure
- [ ] No direct Azure Service Bus SDK calls from services (must go through adapters)
- [ ] No infrastructure provisioning logic (queue/topic creation belongs in Bicep)

### Code Quality
- [ ] Naming conventions followed (see Naming Conventions section)
- [ ] Async methods suffixed with `Async`
- [ ] Private fields use `_camelCase` prefix
- [ ] No unnecessary JavaScript added (prefer Blazor Server capabilities)
- [ ] Code style matches `.editorconfig` rules

### Functionality
- [ ] All message operations are audit-logged via `IAuditLogger`
- [ ] Long-running operations use the worker/dispatcher pattern (not inline)
- [ ] New adapters use `AdapterFactory` with correct mode (`OnBehalfOf` vs `ServiceAccount`)
- [ ] `ApplicationContext` is properly set (CorrelationId, UserName, CurrentConnection)

### Security
- [ ] No secrets or credentials committed
- [ ] Authentication/authorization properly applied
- [ ] Token flow correct (On-Behalf-Of for user actions, Managed Identity for service actions)

### Testing
- [ ] Tests pass (`dotnet test`)
- [ ] New functionality has appropriate test coverage
- [ ] No broken existing tests

### General
- [ ] PR description explains the "why", not just the "what"
- [ ] Commit messages follow conventions
- [ ] No unrelated changes mixed in

## Testing Guidelines

### Philosophy

**No unit tests.** Tests are functional/integration tests that verify real user scenarios end-to-end. We test *functionality*, not classes. Each test should represent an action a user can perform via a page against a real Service Bus instance.

### Test Infrastructure

Tests use **.NET Aspire** to spin up real dependencies:
- **Azure Service Bus** emulator/container — real queues, topics, subscriptions
- **Application Insights** — verify audit log entries are produced
- Other infrastructure dependencies as needed

### What to Test

Tests should cover **user-facing functionality** as performed through Blazor pages:

| Scenario | What to verify |
|----------|----------------|
| Queue overview | Queues are listed with correct message counts |
| Peek messages | Active messages and dead-letter messages are visible |
| Send message | Message arrives on the queue |
| Delete message | Message is removed from the queue |
| Dead-letter a message | Message moves to dead-letter queue |
| Resubmit dead-letter | Message moves back to active queue |
| Background job execution | Long-running action completes via worker |
| Audit logging | User action produces a visible audit log entry |

### What NOT to Test

- Individual classes or methods in isolation (no unit tests)
- Mocked Service Bus interactions — always use a real instance via Aspire
- Internal implementation details (mappers, helpers, extensions)

### Test Structure

```
tests/
├── Droomploeg.DreamOps.Aspire.Tests/     # Aspire-based functional tests
│   ├── Infrastructure/                    # Test setup, Aspire resource builders
│   └── Features/                          # Tests organized by feature/page
│       ├── QueueTests.cs                  # Queue page actions
│       ├── TopicTests.cs                  # Topic page actions
│       ├── DeadLetterTests.cs             # Dead-letter management
│       ├── BackgroundJobTests.cs          # Worker/background job scenarios
│       └── AuditTests.cs                  # Audit log visibility
```

### Running Tests

```bash
# Requires Docker (for Aspire containers)
dotnet test
```

## When Making Changes

1. **Read existing code first** — understand patterns before modifying
2. **Respect layer boundaries** — don't put implementations in Application, don't put domain logic in Infrastructure
3. **New adapters/services** — follow the existing interface-in-Application, implementation-in-Infrastructure pattern
4. **New long-running actions** — use the worker/dispatcher pattern, don't block the UI thread
5. **All message operations must be audit-logged**
6. **Run tests** before and after changes

## Don'ts

- Don't add queue/topic/subscription creation — that's Bicep's job
- Don't bypass the adapter pattern to call Azure Service Bus SDK directly from services
- Don't put business logic in the WebApp layer
- Don't skip audit logging for message operations
- Don't add heavy JavaScript — use Blazor Server capabilities
