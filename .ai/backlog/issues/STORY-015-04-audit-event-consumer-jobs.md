---
labels:
  - EPIC-015
  - Background Workers & Event Processing
  - story
---

# STORY-015-04: Audit event consumer jobs

**Epic:** EPIC-015 — Background Workers & Event Processing

## Description

As a compliance engineer, I want audit event consumers to persist audit records so that events are captured independently of the request path.

## Acceptance Criteria

- Audit events are consumed and stored.
- Consumer failures are monitored.
- Events remain immutable.
