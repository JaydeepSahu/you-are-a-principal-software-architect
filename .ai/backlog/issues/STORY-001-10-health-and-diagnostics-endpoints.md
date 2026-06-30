---
labels:
  - EPIC-001
  - AI API Gateway
  - story
---

# STORY-001-10: Health and diagnostics endpoints

**Epic:** EPIC-001 — AI API Gateway

## Description

As a platform operator, I want the gateway to expose health and readiness endpoints so that Kubernetes and monitoring systems can verify service health.

## Acceptance Criteria

- Liveness and readiness probes are available.
- Health checks reflect dependencies like Redis and Policy Service.
- Gateway returns expected status codes for healthy/unhealthy states.
