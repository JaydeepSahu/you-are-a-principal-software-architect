---
labels:
  - EPIC-008
  - Audit & Compliance Service
  - story
---

# STORY-008-01: Immutable audit event capture

**Epic:** EPIC-008 — Audit & Compliance Service

## Description

As a compliance officer, I want audit events to be captured immutably so that platform actions can be reviewed later.

## Acceptance Criteria

- Audit events are written append-only.
- Events include tenant, request, and action metadata.
- Audit store is protected against modification.
