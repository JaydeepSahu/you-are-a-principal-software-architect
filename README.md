# Enterprise AI Platform

This repository contains the .NET solution skeleton for the Enterprise AI Platform: an AI control plane for software development organizations. The platform is designed to sit between developer tools and AI providers to centralize governance, routing, cost controls, security, observability, and provider integration.

This milestone intentionally implements structure only. It does not implement business workflows, provider calls, policy evaluation, routing algorithms, persistence, or API feature endpoints.

## Solution

```text
EnterpriseAiPlatform.sln
src/
  BuildingBlocks/
    EnterpriseAiPlatform.SharedKernel/
    EnterpriseAiPlatform.Application.Abstractions/
    EnterpriseAiPlatform.Infrastructure.Abstractions/
    EnterpriseAiPlatform.Contracts/
    EnterpriseAiPlatform.ServiceDefaults/
  Gateways/
    EnterpriseAiPlatform.AiGateway.Api/
    EnterpriseAiPlatform.AiGateway.Application/
    EnterpriseAiPlatform.AiGateway.Infrastructure/
    EnterpriseAiPlatform.AiGateway.Contracts/
  Services/
    Identity/
    Policy/
    ModelRegistry/
    Routing/
    ProviderAdapters/
    Metering/
    Audit/
    Observability/
    PortalBff/
  WorkerServices/
    EnterpriseAiPlatform.BackgroundWorkers.Host/
    EnterpriseAiPlatform.BackgroundWorkers.Application/
    EnterpriseAiPlatform.BackgroundWorkers.Infrastructure/
    EnterpriseAiPlatform.BackgroundWorkers.Contracts/
clients/
  vscode/
    enterprise-ai-platform/
tests/
  EnterpriseAiPlatform.Architecture.Tests/
  EnterpriseAiPlatform.SharedKernel.UnitTests/
outputs/
  enterprise-ai-platform-architecture.md
```

## Clean Architecture Rules

Each bounded context follows the approved architecture:

- `Domain` contains aggregates, entities, value objects, domain services, and domain events.
- `Application` contains use cases, commands, queries, validators, and application contracts.
- `Infrastructure` contains persistence, provider clients, message bus, Redis, secrets, and external integrations.
- `Api` or `Host` is the composition root and transport boundary.
- `Contracts` contains versioned integration contracts owned by the bounded context.

Dependency direction is inward:

```text
Api/Host -> Infrastructure -> Application -> Domain -> SharedKernel
Api/Host -> Contracts
Application -> Application.Abstractions
Infrastructure -> Infrastructure.Abstractions
```

## Service Boundaries

The solution includes projects for these service boundaries:

- AI API Gateway
- Identity and Tenant Service
- Policy and Governance Service
- Provider and Model Registry Service
- Routing and Optimization Service
- Provider Adapter Host
- Metering and Cost Service
- Audit and Compliance Service
- Observability Control Service
- Portal BFF
- Background Worker Host

## Shared Building Blocks

- `SharedKernel`: domain-neutral primitives such as `Entity<TId>`, `AggregateRoot<TId>`, `IDomainEvent`, `Result`, `Error`, `TenantId`, and `ValueObject`.
- `Application.Abstractions`: MediatR-based command/query contracts, request context, and unit-of-work abstraction.
- `Infrastructure.Abstractions`: infrastructure-facing abstractions such as system clock, secret references, and distributed leases.
- `Contracts`: integration-event envelope primitives.
- `ServiceDefaults`: host-level health check registration and health endpoints.

## Developer Clients

- `clients/vscode/enterprise-ai-platform`: VS Code extension that authenticates through the Identity Service, stores tokens in VS Code SecretStorage, sends prompts to the AI Gateway, renders streaming chat responses, and supports editor inline code generation.

## Dependencies

Package versions are managed centrally in `Directory.Packages.props`.

Key dependency families are included for the approved architecture:

- ASP.NET Core and .NET 9
- MediatR
- FluentValidation
- Entity Framework Core
- Npgsql PostgreSQL provider
- StackExchange.Redis
- YARP
- OpenTelemetry
- Serilog
- xUnit

## Build Configuration

Build settings are centralized in `Directory.Build.props`:

- `net9.0`
- nullable reference types
- implicit usings
- deterministic builds
- latest recommended analyzers
- warnings as errors
- centralized dependency versions

## Build

```powershell
dotnet restore .\EnterpriseAiPlatform.sln
dotnet build .\EnterpriseAiPlatform.sln --no-restore
dotnet test .\EnterpriseAiPlatform.sln --no-build
```

VS Code extension checks:

```powershell
cd .\clients\vscode\enterprise-ai-platform
npm.cmd run check
npm.cmd test
```

## Host Endpoints

Every runnable API/host project exposes infrastructure health endpoints only:

- `/health/live`
- `/health/ready`

Feature endpoints will be added only in future requested milestones.

## Identity Service

The Identity Service implements the first production feature slice:

- Microsoft Entra ID JWT validation.
- Platform-issued JWT validation.
- API-key authentication for IDE extensions.
- Role-based authorization policies.
- Tenant-aware identity context.
- Refresh-token issuance and rotation.
- Secure response headers and correlation IDs.
- Audit logging for identity API activity and security-sensitive token/key operations.
- PostgreSQL persistence through EF Core under the `identity` schema.

Identity endpoints:

- `POST /api/v1/identity/api-keys` requires `TenantAdmin` or `PlatformAdmin`.
- `POST /api/v1/identity/tokens/api-key` exchanges `X-API-Key` for access and refresh tokens.
- `POST /api/v1/identity/tokens/refresh` rotates a refresh token and issues a new access token.
- `GET /api/v1/identity/me` returns the authenticated principal and requires the `Developer` policy.

Required production configuration:

- `ConnectionStrings:IdentityDatabase`
- `Identity:EntraId:Audience`
- `Identity:PlatformJwt:Issuer`
- `Identity:PlatformJwt:Audience`
- `Identity:PlatformJwt:SigningKey`
- `Identity:ApiKeys:Pepper`
- `Identity:RefreshTokens:Pepper`

Secrets such as signing keys and peppers must come from the deployment secret manager, not source-controlled settings files.

## VS Code Extension

The Enterprise AI Platform VS Code extension implements the developer-facing client milestone:

- Login using an Identity Service-issued IDE API key.
- Authentication through a VS Code `AuthenticationProvider`.
- Secure token persistence through VS Code SecretStorage.
- Chat view in the Enterprise AI activity bar.
- Streaming gateway responses from SSE, NDJSON, JSON, and text streams.
- Inline code generation from selected editor context.
- Configurable Identity Service URL, AI Gateway URL, gateway endpoints, prompt limits, and request timeout.
- VS Code update-check command; marketplace-installed extensions continue to use VS Code automatic extension updates.

Required extension configuration:

- `enterpriseAiPlatform.identityBaseUrl`
- `enterpriseAiPlatform.gatewayBaseUrl`
- `enterpriseAiPlatform.chatEndpoint`
- `enterpriseAiPlatform.inlineCodeEndpoint`

The extension sends prompts only to the AI Gateway. It does not call AI providers directly.
