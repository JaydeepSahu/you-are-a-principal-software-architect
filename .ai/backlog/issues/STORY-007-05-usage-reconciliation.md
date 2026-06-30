---
labels:
  - EPIC-007
  - Metering & Cost Service
  - story
---

# STORY-007-05: Usage reconciliation

**Epic:** EPIC-007 — Metering & Cost Service

## Description

As an operations engineer, I want the metering service to reconcile provider usage records with event-derived usage so that billing data is accurate.

## Acceptance Criteria

- Reconciliation jobs compare provider and platform usage.
- Mismatches are reported.
- Reconciliation supports retries.
