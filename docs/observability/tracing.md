# Tracing

This page defines tracing practices for the Enterprise AI Platform.

## Purpose

- Visualize distributed requests across services.
- Measure latency and identify bottlenecks.

## Standards

- Use OpenTelemetry tracing for cross-service requests.
- Capture spans for API entry, routing, provider calls, and policy decisions.
- Include service name, operation name, and status.
- Propagate context through HTTP, gRPC, and background processing.

## Recommendations

- Record errors and exceptions on failed spans.
- Use consistent span naming across services.
- Link logs and traces using correlation IDs.
