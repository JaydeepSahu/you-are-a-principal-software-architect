# Enterprise AI Platform

This repository contains the .NET solution for the Enterprise AI Platform: an AI control plane for software development organizations. The platform sits between developer tools and AI providers to centralize governance, routing, cost controls, security, observability, knowledge retrieval, and prompt optimization.

The current codebase includes production-shaped service slices for identity, prompt intelligence, knowledge ingestion/search, and the platform gateway and supporting services.

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
    LocalModel/
    Metering/
    Audit/
    Observability/
    Knowledge/
    PromptIntelligence/
    VectorSearch/
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
  EnterpriseAiPlatform.ModelRegistry.UnitTests/
  EnterpriseAiPlatform.Knowledge.UnitTests/
  EnterpriseAiPlatform.PromptIntelligence.UnitTests/
  EnterpriseAiPlatform.PromptIntelligence.Benchmarks/
  EnterpriseAiPlatform.VectorSearch.UnitTests/
  EnterpriseAiPlatform.VectorSearch.Benchmarks/
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
- Local Model Service
- Metering and Cost Service
- Audit and Compliance Service
- Observability Control Service
- Knowledge Service
- Prompt Intelligence Service
- Vector Search Service
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

## Knowledge Service

The Knowledge Service ingests tenant-scoped content and exposes search and retrieval APIs for RAG-style workflows.

Implemented capabilities:

- Document ingestion
- GitHub ingestion
- Confluence ingestion
- SharePoint ingestion
- Jira ingestion
- Chunking
- Embedding generation
- Metadata extraction
- Incremental indexing
- Versioning
- Search APIs

Current endpoints:

- `POST /api/v1/knowledge/ingestions`
- `POST /api/v1/knowledge/search`
- `GET /api/v1/knowledge/documents/{id}`
- `GET /api/v1/knowledge/documents/{id}/versions`

## Prompt Intelligence Service

The Prompt Intelligence Service optimizes prompts before they reach downstream model providers.

Implemented capabilities:

- Prompt rewriting
- Prompt compression
- Conversation summarization
- Context trimming
- Duplicate removal
- Token estimation
- Prompt templates
- Language detection

Current endpoints:

- `POST /api/v1/prompt-intelligence/optimize`
- `POST /api/v1/prompt-intelligence/profiles`
- `GET /api/v1/prompt-intelligence/profiles`
- `GET /api/v1/prompt-intelligence/sessions`

The service also includes benchmark coverage under `tests/EnterpriseAiPlatform.PromptIntelligence.Benchmarks`.

## Model Registry Service

The Model Registry Service tracks provider models and their operational metadata for routing, policy, and cost-aware selection.

Supported providers:

- OpenAI
- GitHub Copilot
- Azure OpenAI
- Anthropic
- Gemini
- DeepSeek
- Qwen
- Llama

Stored data:

- Capabilities
- Pricing
- Latency
- Context size
- Availability
- Health
- Configuration

Current endpoints:

- `POST /api/v1/model-registry/models`
- `GET /api/v1/model-registry/models`
- `GET /api/v1/model-registry/models/{id}`
- `PUT /api/v1/model-registry/models/{id}`
- `DELETE /api/v1/model-registry/models/{id}`
- `GET /api/v1/model-registry/providers`

The service includes tenant-scoped CRUD behavior and unit coverage under `tests/EnterpriseAiPlatform.ModelRegistry.UnitTests`.

Local run:

```powershell
dotnet run --project .\src\Services\ModelRegistry\EnterpriseAiPlatform.ModelRegistry.Api\EnterpriseAiPlatform.ModelRegistry.Api.csproj
```

## Routing Engine

The Routing Engine classifies requests, estimates complexity, token usage, and cost, and selects the best model across rule-based, ML, policy, budget, department, and repository routing modes.

Supported routing modes:

- Rule-based routing
- ML routing
- Policy routing
- Budget routing
- Department routing
- Repository routing

Current endpoints:

- `GET /api/v1/routing/modes`
- `GET /api/v1/routing/configuration`
- `PUT /api/v1/routing/configuration`
- `DELETE /api/v1/routing/configuration`
- `POST /api/v1/routing/evaluate`

Local run:

```powershell
dotnet run --project .\src\Services\Routing\EnterpriseAiPlatform.Routing.Api\EnterpriseAiPlatform.Routing.Api.csproj
```

Routing benchmarks:

```powershell
dotnet run --project .\tests\EnterpriseAiPlatform.Routing.Benchmarks\EnterpriseAiPlatform.Routing.Benchmarks.csproj -- --filter *
```

## Vector Search Service

The Vector Search Service provides tenant-scoped vector retrieval for RAG and knowledge workflows.

Implemented capabilities:

- pgvector provider support
- Qdrant provider support
- In-memory local provider
- Hybrid search
- Semantic search
- Metadata filters
- Top-K search
- Re-ranking
- Query-result caching
- Benchmark coverage
- Browser UI for indexing, searching, and viewing progress

Current endpoints:

- `POST /api/v1/vector-search/documents`
- `POST /api/v1/vector-search/search`
- `GET /api/v1/vector-search/progress`

Local dashboard:

```powershell
dotnet run --project .\src\Services\VectorSearch\EnterpriseAiPlatform.VectorSearch.Api\EnterpriseAiPlatform.VectorSearch.Api.csproj
```

Open `http://localhost:5188` and use the dashboard to index content, run semantic or hybrid search, apply metadata filters, toggle re-ranking, and observe indexed/cached counts.

Provider setup:

```json
{
  "VectorSearch": {
    "Provider": "InMemory"
  }
}
```

```json
{
  "VectorSearch": {
    "Provider": "PgVector",
    "PgVectorConnectionString": "Host=localhost;Port=5432;Database=enterprise_ai;Username=postgres;Password=postgres",
    "PgVectorTable": "vector_search_documents"
  }
}
```

```json
{
  "VectorSearch": {
    "Provider": "Qdrant",
    "QdrantEndpoint": "http://localhost:6333",
    "QdrantCollection": "enterprise_ai_platform",
    "QdrantApiKey": ""
  }
}
```

API example:

```powershell
Invoke-RestMethod -Method Post http://localhost:5188/api/v1/vector-search/documents `
  -ContentType 'application/json' `
  -Body '{"documents":[{"externalId":"runbook-1","content":"Hybrid vector search with pgvector, Qdrant, metadata filters, reranking, cache, and top-k retrieval.","metadata":{"team":"platform","source":"runbook"}}]}'

Invoke-RestMethod -Method Post http://localhost:5188/api/v1/vector-search/search `
  -ContentType 'application/json' `
  -Body '{"query":"hybrid vector metadata filters","mode":"Hybrid","topK":5,"metadataFilters":{"team":"platform"},"rerank":true,"useCache":true}'
```

## Local Model Service

The Local Model Service routes chat and streaming requests to local or OpenAI-compatible model backends.

Supported backends:

- vLLM
- Ollama
- OpenAI-compatible APIs

Implemented capabilities:

- Streaming
- Model health
- Load balancing
- GPU selection
- Model discovery
- Configuration

Current endpoints:

- `GET /api/v1/local-model/providers`
- `GET /api/v1/local-model/providers/{providerKey}`
- `POST /api/v1/local-model/providers`
- `PUT /api/v1/local-model/providers/{providerKey}`
- `DELETE /api/v1/local-model/providers/{providerKey}`
- `POST /api/v1/local-model/chat`
- `POST /api/v1/local-model/stream`
- `GET /api/v1/local-model/health`
- `GET /api/v1/local-model/models`

Configuration example:

```json
{
  "LocalModel": {
    "Providers": [
      {
        "ProviderKey": "ollama-main",
        "Name": "Ollama Main",
        "BackendKind": "Ollama",
        "ModelName": "llama3.1",
        "BaseUri": "http://localhost:11434",
        "ChatPath": "/v1/chat/completions",
        "StreamPath": "/v1/chat/completions",
        "HealthPath": "/health",
        "DiscoveryPath": "/v1/models",
        "DefaultStrategy": "GpuAware",
        "MaxConcurrency": 4
      }
    ]
  }
}
```

Local run:

```powershell
dotnet run --project .\src\Services\LocalModel\EnterpriseAiPlatform.LocalModel.Api\EnterpriseAiPlatform.LocalModel.Api.csproj
```

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

Prompt Intelligence benchmarks:

```powershell
dotnet run --project .\tests\EnterpriseAiPlatform.PromptIntelligence.Benchmarks\EnterpriseAiPlatform.PromptIntelligence.Benchmarks.csproj -- --filter *
```

Vector Search benchmarks:

```powershell
dotnet run --project .\tests\EnterpriseAiPlatform.VectorSearch.Benchmarks\EnterpriseAiPlatform.VectorSearch.Benchmarks.csproj -- --filter *
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

## Current Milestones

- Identity Service: implemented
- Prompt Intelligence Service: implemented
- Knowledge Service: implemented
- Vector Search Service: implemented
- Remaining service slices continue to evolve in future milestones

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

## Prompt Intelligence Service

- **Purpose:** Optimize, validate, and prepare prompts for downstream AI providers to improve quality, reduce cost, and enforce limits.
- **Responsibilities:**
  - Token counting, prompt trimming, template expansion, and prompt augmentation.
  - Optimization rules engine and prompt-level heuristics (see `src/Services/PromptIntelligence`).
  - Safety and policy pre-checks (policy enforcement remains the Policy service responsibility).
  - Exposes focused endpoints such as `POST /api/v1/prompt-intelligence/optimize`, `POST /api/v1/prompt-intelligence/token-count`, and template management endpoints.
  - Provides metrics, cost estimation, and optional caching for repeated prompt shapes.
  - Does not persist prompt content by default; retention requires explicit tenant policy and encryption.

## Knowledge Service

- **Purpose:** Ingest, index, and serve tenant knowledge for retrieval-augmented generation (RAG) and semantic search.
- **Responsibilities:**
  - Connectors and ingestion pipelines for documents, databases, and external content sources.
  - Document chunking, metadata tagging, embedding generation, vector index management, and re-ranking.
  - Exposes ingestion and query endpoints such as `POST /api/v1/knowledge/ingest` and `POST /api/v1/knowledge/query`.
  - Tenant isolation, encryption at rest, and retention rules are required for all stored knowledge artifacts.
  - Integrates with vector stores and embedding providers via `ProviderAdapters`.

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

## Quick Start

- **Clone:** git clone https://github.com/your-org/EnterpriseAiPlatform.git
- **Restore & build:**

```powershell
dotnet restore .\EnterpriseAiPlatform.sln
dotnet build .\EnterpriseAiPlatform.sln --no-restore
```

- **Run unit tests:**

```powershell
dotnet test .\EnterpriseAiPlatform.sln --no-build
```

- **VS Code extension checks:**

```powershell
cd .\clients\vscode\enterprise-ai-platform
npm.cmd run check
npm.cmd test
```

## Contributing

- Follow the architecture and coding rules in `copilot-instructions.md` and `CLAUDE.md`.
- Run the focused verification commands before opening a pull request.
- Keep changes small and preserve dependency direction and bounded-context boundaries.

## License

This repository is licensed under the terms in [LICENSE.txt](LICENSE.txt).
