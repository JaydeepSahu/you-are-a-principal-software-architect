# Platform Architecture

The Enterprise AI Platform is engineered using strict **Clean Architecture** principles across a distributed ecosystem of 19 microservices.

## Architectural Tenets

### 1. Clean Architecture (Onion Pattern)
Every service is rigidly separated into four distinct projects to guarantee decoupling:
- **Domain**: Contains aggregates, entities, value objects, and invariants. Zero external dependencies.
- **Application**: Contains Use Cases (Commands/Queries), MediatR handlers, and FluentValidation. Depends only on the Domain.
- **Infrastructure**: The implementation layer. Contains EF Core Repositories, Redis Caches, and Provider Adapters. Depends on Application abstractions.
- **Api / Host**: The presentation layer. Exposes Minimal APIs or background worker boundaries.

### 2. The AI Gateway Control Plane
Instead of allowing scattered microservices or frontend clients to call LLM providers directly, the platform mandates all AI traffic route through the **AI Gateway API**. 
The Gateway enforces:
- **Resilience**: Polly-based circuit breakers, retries, and automatic model failover.
- **Security**: Data Loss Prevention (DLP) to scrub PII and secrets before leaving the network.
- **Governance**: Token metering, cost allocation, and quota enforcement.

### 3. Asynchronous Messaging
Inter-service communication heavily leverages **RabbitMQ** for event-driven workflows. When a model is updated or a token budget is breached, Domain Events are dispatched over the message bus to ensure eventual consistency without tight HTTP coupling.

### 4. Dependency Injection Validation
The platform validates Dependency Injection at startup to prevent **Captive Dependencies**. For example, `IRequestContextAccessor` is globally scoped to the HTTP request lifecycle; therefore, long-lived infrastructure components (like Cache Stores) must also be scoped or transient to prevent memory leaks or context contamination across tenants.