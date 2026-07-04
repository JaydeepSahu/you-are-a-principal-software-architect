# Architecture Overview

Enterprise AI Platform follows Clean Architecture and bounded-context service slices. Each service owns its domain model, application use cases, infrastructure adapters, and API/host composition root.

## Layers

- Domain: aggregates, entities, value objects, and domain rules
- Application: use cases, commands, queries, orchestration, and contracts
- Infrastructure: persistence, external integrations, caching, queues, telemetry
- API/Host: transport, authentication, OpenAPI, health checks, and routing

## Platform Principles

- Services are independently deployable
- External AI providers are isolated behind adapter abstractions
- Shared building blocks contain only cross-cutting primitives
- APIs expose versioned contracts and documented endpoints

