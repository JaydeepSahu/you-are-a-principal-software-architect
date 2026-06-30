---
labels:
  - EPIC-012
  - Secrets & Credential Management Integration
  - story
---

# STORY-012-05: Secret access logging

**Epic:** EPIC-012 — Secrets & Credential Management Integration

## Description

As an auditor, I want secret access to be logged so that retrieval events are traceable.

## Acceptance Criteria

- Secret retrieval events are logged without secret material.
- Access logs include service identity and timestamp.
- Logs are stored in an audit-backed store.
