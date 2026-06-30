---
labels:
  - EPIC-015
  - Background Workers & Event Processing
  - story
---

# STORY-015-01: Transactional outbox pattern

**Epic:** EPIC-015 — Background Workers & Event Processing

## Description

As a developer, I want the platform to use a transactional outbox so that events are reliably published with state changes.

## Acceptance Criteria

- Outbox records are written transactionally.
- Publisher reads and publishes outbox events reliably.
- Duplicate event handling is idempotent.
