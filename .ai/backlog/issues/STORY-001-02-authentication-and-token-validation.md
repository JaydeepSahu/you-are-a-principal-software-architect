---
labels:
  - EPIC-001
  - AI API Gateway
  - story
---

# STORY-001-02: Authentication and token validation

**Epic:** EPIC-001 — AI API Gateway

## Description

As a secure platform, I want the gateway to validate platform tokens so that only authorized clients may invoke AI services.

## Acceptance Criteria

- Gateway rejects unauthenticated requests with 401.
- Valid platform tokens are validated against identity service claims.
- Token validation failures result in clear error responses.
