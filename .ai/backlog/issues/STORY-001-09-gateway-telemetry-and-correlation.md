---
labels:
  - EPIC-001
  - AI API Gateway
  - story
---

# STORY-001-09: Gateway telemetry and correlation

**Epic:** EPIC-001 — AI API Gateway

## Description

As an operations engineer, I want the gateway to emit traces, metrics, and correlation IDs so that requests are observable end-to-end.

## Acceptance Criteria

- Correlation IDs are generated and propagated.
- Metrics include request counts, latency, and status codes.
- Traces include gateway, policy, routing, and provider steps.
