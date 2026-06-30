---
labels:
  - EPIC-001
  - AI API Gateway
  - story
---

# STORY-001-03: Tenant and principal resolution

**Epic:** EPIC-001 — AI API Gateway

## Description

As an enterprise admin, I want the gateway to resolve tenant, application, and subject context from each request so that downstream enforcement is tenant-aware.

## Acceptance Criteria

- Tenant ID is extracted from the authenticated principal.
- Application and subject identifiers are available in request context.
- Tenant-scoped metadata is attached to telemetry and enforcement.
