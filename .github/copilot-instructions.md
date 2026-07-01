# GitHub Copilot Instructions

Use these instructions for all AI-assisted coding in this repository.

## Project Context

This repository contains the Enterprise AI Platform, a .NET 9 enterprise AI control plane for developer tooling. It centralizes AI gateway access, identity, tenant isolation, policy governance, routing, provider adapters, metering, audit, observability, and a VS Code client.

The platform is not an IDE assistant replacement. It treats GitHub Copilot, Azure OpenAI, Anthropic, Gemini, self-hosted models, and future providers as governed integrations behind platform boundaries.

## Architecture Rules

- Preserve Clean Architecture per bounded context.
- Keep dependency direction inward:
  - `Api` or `Host` composes and exposes transport boundaries.
  - `Infrastructure` implements persistence, Redis, provider clients, secrets, messaging, and external integrations.
  - `Application` owns use cases, commands, queries, validators, transaction orchestration, and application contracts.
  - `Domain` owns aggregates, entities, value objects, domain services, domain events, and invariants.
  - `Contracts` owns versioned integration contracts for the bounded context.
- Do not add infrastructure dependencies to Domain or Application unless an existing abstraction explicitly permits it.
- Do not share service-owned database tables across bounded contexts. Cross-service access must use APIs, events, or contracts.
- Keep provider-specific behavior inside provider adapter or infrastructure boundaries. Core gateway, policy, routing, and domain logic must remain provider-neutral.
- Use existing shared building blocks before creating new primitives:
  - `EnterpriseAiPlatform.SharedKernel`
  - `EnterpriseAiPlatform.Application.Abstractions`
  - `EnterpriseAiPlatform.Infrastructure.Abstractions`
  - `EnterpriseAiPlatform.Contracts`
  - `EnterpriseAiPlatform.ServiceDefaults`

## Security and Governance Defaults

- Tenant isolation is mandatory in APIs, domain operations, persistence, Redis keys, events, telemetry, and provider credential selection.
- Default to deny-by-default authorization and policy behavior.
- Never store prompt or response content by default. Content retention requires explicit tenant policy, encryption, retention rules, access control, and audit.
- Never commit secrets, signing keys, peppers, provider credentials, connection strings, or real tokens.
- Secrets must come from deployment secret managers or local user configuration, not source-controlled appsettings files.
- Logs, traces, metrics, and audit records must not include secrets, API keys, bearer tokens, refresh tokens, provider credentials, or prompt/source content unless explicitly allowed by policy.
- Preserve correlation IDs and trace context across API, gateway, adapter, and background processing paths.
- Cost optimization must never override security, tenant policy, or data residency constraints.

## Coding Standards

- Target .NET 9 and C# 13.
- Nullable reference types and warnings-as-errors are enabled. Write code that compiles cleanly.
- Use implicit usings and central package management. Add package versions only in `Directory.Packages.props`.
- Prefer constructor injection and options validation for configuration.
- Use MediatR for application commands and queries where the surrounding service uses it.
- Use FluentValidation for input validation in Application.
- Use EF Core only in Infrastructure projects.
- Use Redis only through application-facing abstractions where practical.
- Keep public APIs versioned under `/api/v1/...` unless the existing endpoint style says otherwise.
- Use explicit request/response contracts. Avoid leaking domain entities through API boundaries.
- Keep comments short and only where they clarify non-obvious decisions.
- Follow the naming and layout patterns already present in nearby files.

## Testing Expectations

- Add or update tests for behavior changes.
- Put domain and application behavior in focused unit tests.
- Add architecture or boundary tests when dependency direction or solution structure changes.
- Include tenant isolation, security-sensitive, and failure-path tests when touching identity, policy, gateway, persistence, auth, tokens, rate limits, or provider routing.
- For VS Code client changes, update or add tests under `clients/vscode/enterprise-ai-platform/tests`.

## Verification Commands

Use the smallest verification set that matches the change, then broaden when touching shared behavior.

```powershell
dotnet restore .\EnterpriseAiPlatform.sln
dotnet build .\EnterpriseAiPlatform.sln --no-restore
dotnet test .\EnterpriseAiPlatform.sln --no-build
```

For VS Code extension changes:

```powershell
cd .\clients\vscode\enterprise-ai-platform
npm.cmd run check
npm.cmd test
```

## Vibe Coding Guardrails

- Read the nearby code before editing.
- Make the smallest coherent change that satisfies the request.
- Do not invent new architectural patterns when an existing project pattern works.
- Do not generate placeholder production logic that silently succeeds.
- Do not bypass validation, authorization, rate limiting, policy, audit, or telemetry to make a flow work.
- Do not call AI providers directly from clients or domain/application layers. Client prompts go to the AI Gateway.
- Do not introduce broad refactors during feature work unless the user explicitly asks.
- Keep generated code production-shaped: explicit errors, safe defaults, cancellation tokens for async work, and no hidden global state.
- When unsure about a product/library/API behavior that may have changed, verify against official documentation before coding.

