# Tech Stack

## Preferred technologies

- Runtime: .NET 9 and ASP.NET Core
- Web: YARP where useful for reverse-proxying and streaming
- Persistence: PostgreSQL (system of record), Redis (ephemeral state, rate limits, cache)
- Messaging: Kafka/Redpanda/Azure Event Hubs (event bus for durability and decoupling)
- Observability: OpenTelemetry, Serilog for structured logs
- ORM / Patterns: EF Core, MediatR, FluentValidation
- Deployment: Docker, Kubernetes, service mesh for mTLS and workload identity

## Rationale

The stack prioritizes enterprise maturity, streaming support, strong telemetry, and operational tooling. PostgreSQL supports partitioning and RLS for multi-tenancy; Redis supports low-latency counters and caches; OpenTelemetry and structured logs enable cross-service tracing and SLOs.

