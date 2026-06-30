---
labels:
  - EPIC-007
  - Metering & Cost Service
  - story
---

# STORY-007-01: Usage event ingestion

**Epic:** EPIC-007 — Metering & Cost Service

## Description

As an analytics consumer, I want the metering service to ingest usage events from the event bus so that platform usage is recorded.

## Acceptance Criteria

- Metering consumers read usage events reliably.
- Events are processed idempotently.
- Ingestion failures are retried.
