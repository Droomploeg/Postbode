# DreamOps — Azure Service Bus Operations Tool

[![CI](https://github.com/Droomploeg/DreamOps/actions/workflows/ci.yml/badge.svg)](https://github.com/Droomploeg/DreamOps/actions/workflows/ci.yml)
[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

DreamOps is an operational management tool for Azure Service Bus, built with .NET Blazor Server. It provides advanced authorization through Azure Entra ID and focuses exclusively on **message management** — infrastructure provisioning (queues, topics, subscriptions) is intentionally left to Infrastructure as Code (Bicep).

Unlike tools such as Service Bus Explorer, DreamOps offers fine-grained authorization at different levels, ensuring effective permission management. It operates entirely within Azure.

Ideally, no messages should end up in the dead-letter queue of a service bus. However, in reality, this can happen due to factors outside your team's control. DreamOps surfaces the dead-letter reason and description for each message, and can deep-link to Azure Application Insights — by correlation ID or message ID — so you can trace what happened end-to-end. From there you can delete the message, resubmit it, or send a new one.

## Features

- Overview of all queues, topics, and subscriptions
- Status monitoring of queues, topics, and subscriptions
- Peek and inspect messages on queues and topics
- Send new messages to queues and topics
- Delete messages from queues
- Dead-letter queue management (inspect, resubmit, delete) with reason and description
- Deep-link from messages to Application Insights for end-to-end tracing (optional, requires configuration)
- Background processing for long-running operations
- Audit logging of all user message actions
- Authorization through Azure Entra ID
- Fully operates on Azure

## Architecture

DreamOps follows Clean Architecture with four layers:

```
┌──────────────────────────────────────────┐
│  WebApp (Blazor Server + BackgroundService)│
├──────────────────────────────────────────┤
│  Application (Interfaces / Contracts)     │
├──────────────────────────────────────────┤
│  Core / Domain (Models, Value Types)      │
├──────────────────────────────────────────┤
│  Infrastructure (Azure Service Bus, Audit)│
└──────────────────────────────────────────┘
```

- **WebApp**: Blazor Server application with interactive server-side rendering and a background service for long-running tasks
- **Application**: Pure interface layer — service contracts, adapter contracts, factory contracts
- **Core/Domain**: Domain models (Queue, Topic, Subscription, WorkerItem), value types, and enums
- **Infrastructure**: Azure Service Bus adapter/service implementations, audit logging, worker service

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Framework | .NET 10.0 / C# latest |
| UI | Blazor Server (Interactive Server) |
| Authentication | Microsoft Entra ID (OpenID Connect) |
| Token Flow | On-Behalf-Of (user delegation) + Managed Identity (service account) |
| Service Bus | Azure.Messaging.ServiceBus SDK |
| Telemetry | Application Insights |
| IaC | Bicep |
| CI/CD | GitHub Actions |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/) (or .NET 8+ with `rollForward: latestMajor`)
- Azure Subscription
- Azure Service Bus namespace
- Azure Entra ID (app registration)
- Azure Service Principal
- Azure Application Insights (optional)

### Build and Run

```bash
# Build the solution
dotnet build

# Run the web application
dotnet run --project src/Droomploeg.DreamOps.WebApp

# Run with Aspire orchestration (development)
dotnet run --project src/Droomploeg.DreamOps.AppHost

# Run tests
dotnet test
```

### Configuration

The application requires the following settings in `appsettings.json`:

```json
{
  "AzureEntra": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "<domain>.onmicrosoft.com",
    "TenantId": "<TenantId>",
    "ClientId": "<ClientId>",
    "ClientSecret": "<ClientSecret>",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-oidc"
  },
  "AzureServiceBusConnections": [
    {
      "Name": "<ConnectionName>",
      "FullyQualifiedNamespace": "<namespace>.servicebus.windows.net"
    }
  ],
  "ManagedIdentityClientId": "<ManagedIdentityClientId>",
  "ApplicationInsights": {
    "ConnectionString": "<ConnectionString>"
  }
}
```

## Installation on Azure

For this manual, a demo application name is used. You can change it as needed.
The demo application name is "DreamOpsDemo01".

### Setting Up Azure Entra ID

#### Create New App Registration

1. Open [Azure Entra ID](https://entra.microsoft.com)
2. Go to **Applications** > **App registrations**
3. Select **New registration**
4. Fill in the form (this is where the demo name is set: "DreamOpsDemo01")
5. After creation, go to the app registration and set the **Authentication**:
   - Select **Add URI** and fill in the redirect URIs:
     - Localhost: `https://localhost:7273/signin-oidc`
     - Azure: `https://dreamopsdemo01.azurewebsites.net/signin-oidc`
   - Select checkbox **ID tokens (used for implicit and hybrid flows)**
   - Press **Save**
6. Go to **Certificates & Secrets**:
   - Select tab **Client secrets** and create a secret (this will be the `ClientSecret` in appsettings)
7. Go to **API permissions** and grant:
   - `Microsoft.Graph` > `User.Read` (Delegated)
   - `Microsoft.ServiceBus` > `user_impersonation` (Delegated)
8. Go to **App roles** and create a role:
   - Display name: `General_Access`
   - Allowed member types: **Users/Groups**
   - Value: `General_Access`
   - Description: `General access for DreamOps`
   - Check **Do you want to enable this app role?**
   - Press **Apply**

#### Enable Access for Users/Groups

1. Open [Azure Entra ID](https://entra.microsoft.com)
2. Go to **Applications** > **Enterprise applications**
3. Select your application
4. Select **Users and Groups** to manage access
5. Select **Add user/group**, choose a User/Group and the Role
6. Press **Assign**

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for the contribution process and [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for community standards. By submitting a pull request, you agree to the [Contributor License Agreement](CLA.md).

For security-related issues, please follow the process described in [SECURITY.md](SECURITY.md) — do **not** open a public issue.

## License

This project is licensed under [AGPL-3.0](LICENSE). See [CLA.md](CLA.md) for the Contributor License Agreement.

## Disclaimer

DreamOps is provided **"AS IS"**, without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, and non-infringement. In no event shall the authors or copyright holders be liable for any claim, damages, or other liability, whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the software or its use. See sections 15 and 16 of the [AGPL-3.0 license](LICENSE) for the full disclaimer.
