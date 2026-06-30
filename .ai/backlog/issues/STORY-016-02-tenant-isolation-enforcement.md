---
labels:
  - EPIC-016
  - Security & Threat Protection
  - story
---

# STORY-016-02: Tenant isolation enforcement

**Epic:** EPIC-016 — Security & Threat Protection

## Description

As an architect, I want tenant isolation enforced across data stores and caches so that no tenant can access another tenant’s data.

## Acceptance Criteria

- Tenant IDs are propagated through every service.
- Cache keys are tenant-scoped.
- RLS or equivalent is implemented for shared tables.
