---
labels:
  - EPIC-006
  - Provider Adapter Host & Adapters
  - story
---

# STORY-006-04: Streaming translation support

**Epic:** EPIC-006 — Provider Adapter Host & Adapters

## Description

As a gateway engineer, I want adapters to relay streaming provider responses without buffering so that token delivery remains low-latency.

## Acceptance Criteria

- Adapters stream tokens as they arrive.
- Backpressure is handled correctly.
- Streaming cancellation is supported.
