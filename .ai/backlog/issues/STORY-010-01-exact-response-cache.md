---
labels:
  - EPIC-010
  - Semantic & Exact Cache
  - story
---

# STORY-010-01: Exact response cache

**Epic:** EPIC-010 — Semantic & Exact Cache

## Description

As a performance engineer, I want the gateway to use an exact response cache so that repeat requests can yield faster responses.

## Acceptance Criteria

- Exact cache entries can be stored and retrieved.
- Cache keys are tenant-scoped.
- Cache hits reduce provider calls.
