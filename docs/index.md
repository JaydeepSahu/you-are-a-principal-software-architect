# Enterprise AI Platform

A production-grade AI control plane for enterprise developer tooling, governance, and secure provider orchestration.

::: tip Project Vision
The Enterprise AI Platform enables organizations to centrally manage AI interactions, enforce policy, preserve tenant isolation, and operationalize provider routing across developer tools.
:::

## Architecture Overview

The platform uses a Clean Architecture service model with bounded-context services for identity, policy, routing, provider adapters, knowledge, prompt intelligence, and observability.

- **Service-oriented** architecture with independently deployable bounded contexts
- **API/Host** layer for transport, authentication, OpenAPI, and health checks
- **Infrastructure** layer for persistence, provider integrations, and caching
- **Application** layer for use cases, validation, and orchestration
- **Domain** layer for business rules, aggregates, and tenant-aware invariants

## Technology Stack

- **Platform:** .NET 9, ASP.NET Core
- **Data:** PostgreSQL, Redis
- **Observability:** OpenTelemetry, structured logging
- **API docs:** OpenAPI + Scalar + MkDocs Material
- **Testing:** xUnit, integration/contract tests
- **Client:** VS Code extension and developer tooling integrations

## Repository Structure

- `src/` — service source code, adapters, and host projects
- `clients/` — VS Code extension and client integrations
- `tests/` — unit, integration, and architecture tests
- `docs/` — documentation portal and governance artifacts
- `outputs/` — generated architecture reports and diagrams

## Documentation Navigation

- **Getting Started:** onboarding and setup
- **Architecture:** system design, folder structure, coding standards
- **API:** API guidelines and documentation standards
- **AI:** prompt intelligence, knowledge, and model routing
- **Deployment:** environment and release guidance
- **Security:** authentication, authorization, and secure design
- **Observability:** telemetry, monitoring, and diagnostics
- **Testing:** quality strategy and validation
- **Operations:** runbooks and operational procedures
- **Troubleshooting:** common issues and fixes
- **ADRs:** architecture decision records
- **Diagrams:** visual architecture and request flow diagrams
- **Assets:** supporting diagrams and documentation files

## Quick Start

```powershell
cd C:\Users\kumar\Documents\Codex\2026-06-30\you-are-a-principal-software-architect
.venv\Scripts\python.exe -m mkdocs serve
```

Then open `http://127.0.0.1:8000`.

## Useful Links

- [Repository README](https://github.com/JaydeepSahu/EnterpriseAiPlatform/blob/main/README.md)
- [Documentation Portal](documentation-portal.md)
- [Architecture overview](architecture/architecture-overview.md)
- [API guidelines](api/api-guidelines.md)
- [Security guide](security/security-guide.md)
- [Developer onboarding](dev/onboarding.md)
