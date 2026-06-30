---
labels:
  - EPIC-006
  - Provider Adapter Host & Adapters
  - story
---

# STORY-006-01: Adapter host runtime

**Epic:** EPIC-006 — Provider Adapter Host & Adapters

## Description

As an architect, I want a provider adapter host that loads and executes adapters so that provider integration is isolated from the gateway.

## Acceptance Criteria

- Adapter host can register provider adapters.
- Adapter host receives canonical requests and routes them to adapters.
- Adapter host exposes health and telemetry.
