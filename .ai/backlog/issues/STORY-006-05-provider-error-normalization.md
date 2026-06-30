---
labels:
  - EPIC-006
  - Provider Adapter Host & Adapters
  - story
---

# STORY-006-05: Provider error normalization

**Epic:** EPIC-006 — Provider Adapter Host & Adapters

## Description

As an operations engineer, I want provider-specific errors normalized into platform errors so that downstream services can handle them consistently.

## Acceptance Criteria

- Provider errors map to platform error categories.
- Normalized responses include original provider details for debugging.
- Error handling avoids leaking secrets.
