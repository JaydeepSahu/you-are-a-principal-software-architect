# Architecture Context

## Principles

- Control plane first: centralize policy, routing, observability, and cost governance.
- Provider-neutral core: provider-specific details live behind adapter boundaries.
- Tenant isolation by design: identity, data, caches, secrets, events, logs, and metrics are tenant-scoped.
- Clean Architecture and DDD: domain and use cases do not depend on infrastructure.
- Secure by default and cloud-native operations (Kubernetes, health checks, autoscaling, OpenTelemetry).

## System overview

The platform exposes an AI API Gateway as the synchronous data plane, backed by control-plane services: Identity & Tenant, Policy & Governance, Routing & Optimization, Provider/Model Registry, Metering, Audit, and Observability. Provider adapters run in the Provider Adapter Host and communicate with external AI providers.

Key dataplane flows:
- Authenticate and resolve tenant
- Rate-limit and validate
- Evaluate policy (Policy Service)
- Select route (Routing Service)
- Forward to provider adapter
- Stream response back to client
- Emit events for metering and audit

## Service boundaries (summary)

- Gateway: ingress, enforcement, streaming proxy, short-lived state (rate-limit keys, idempotency).
- Identity & Tenant: tenants, users, applications, credential metadata.
- Policy: policy sets, assignments, decisions.
- Routing: route selection, provider/model choices, route health.
- Provider Registry: provider and model catalog, pricing metadata.
- Metering & Audit: usage facts, cost, immutable audit events.

## Diagrams

Refer to the full architecture document for system context, container, component, and deployment diagrams (mermaid diagrams are included in the source architecture document).

