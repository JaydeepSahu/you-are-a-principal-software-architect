---
labels:
  - EPIC-018
  - Testing, Observability & SLOs
  - story
---

# STORY-018-03: OpenTelemetry instrumentation

**Epic:** EPIC-018 — Testing, Observability & SLOs

## Description

As an SRE, I want all services instrumented with OpenTelemetry so that traces, metrics, and logs can be correlated.

## Acceptance Criteria

- Services emit traces with correlation IDs.
- Metrics include request, latency, and error counts.
- Logs include structured context.
