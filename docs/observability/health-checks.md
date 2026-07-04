# Health Checks

This page defines health check expectations for the Enterprise AI Platform.

## Purpose

- Verify service liveness and readiness.
- Support container orchestrators and deployment health probes.

## Standards

- Expose a `/health/live` or `/healthz` endpoint for liveness.
- Expose a `/health/ready` endpoint for readiness.
- Include dependency health for databases, caches, provider credential stores, and configuration.
- Keep liveness checks lightweight and suitable for restart behavior.

## Recommendations

- Return plain status for liveness and detailed dependency status for readiness.
- Use OpenTelemetry health check integration where supported.
- Document health endpoint contracts and expected HTTP status codes.
