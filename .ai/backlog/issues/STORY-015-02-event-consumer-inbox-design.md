---
labels:
  - EPIC-015
  - Background Workers & Event Processing
  - story
---

# STORY-015-02: Event consumer inbox design

**Epic:** EPIC-015 — Background Workers & Event Processing

## Description

As an operations engineer, I want consumer inbox patterns so that event processing can survive retries and duplicates.

## Acceptance Criteria

- Consumers track processed event IDs.
- Duplicate events are ignored.
- Consumer state is durable.
