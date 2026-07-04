# Correlation IDs

This page defines correlation ID usage for the Enterprise AI Platform.

## Purpose

- Track requests across services and logs.
- Enable diagnostics of distributed requests.

## Standards

- Generate a correlation ID at API ingress.
- Propagate the correlation ID through HTTP, messaging, and background work.
- Include the correlation ID in logs, traces, and metric dimensions.
- Respect any existing request ID header if provided by upstream load balancers or API gateways.

## Recommendations

- Use a stable header name such as `X-Correlation-ID`.
- Emit the correlation ID in logs and trace context.
- Avoid embedding sensitive data in the correlation ID.
