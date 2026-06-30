---
labels:
  - EPIC-015
  - Background Workers & Event Processing
  - story
---

# STORY-015-03: Metering consumer jobs

**Epic:** EPIC-015 — Background Workers & Event Processing

## Description

As a metering engineer, I want background jobs to process usage events so that billing data is built asynchronously.

## Acceptance Criteria

- Metering consumers process usage events.
- Failures are retried.
- Processing lag metrics are emitted.
