---
labels:
  - EPIC-004
  - Routing & Optimization Service
  - story
---

# STORY-004-05: Route health scoring

**Epic:** EPIC-004 — Routing & Optimization Service

## Description

As an operations engineer, I want the routing service to track provider/adapter health and score routes so that routing degrades safely.

## Acceptance Criteria

- Health metrics are ingested into route scoring.
- Poor health reduces route selection priority.
- Health score changes are auditable.
