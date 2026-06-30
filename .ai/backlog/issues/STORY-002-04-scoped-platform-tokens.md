---
labels:
  - EPIC-002
  - Identity & Tenant Service
  - story
---

# STORY-002-04: Scoped platform tokens

**Epic:** EPIC-002 — Identity & Tenant Service

## Description

As a developer, I want platform tokens that carry tenant, application, and role claims so that API authorization is enforced.

## Acceptance Criteria

- Tokens include tenant, application, subject, and scope claims.
- Token issuance is auditable.
- Invalid or expired tokens are rejected.
