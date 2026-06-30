---
labels:
  - EPIC-004
  - Routing & Optimization Service
  - story
---

# STORY-004-01: Route decision engine

**Epic:** EPIC-004 — Routing & Optimization Service

## Description

As a gateway, I want the routing service to choose the optimal provider/model path so that requests meet policy, cost, and latency goals.

## Acceptance Criteria

- Routing decisions return provider, model, and adapter targets.
- Decisions include reason codes and health abstractions.
- Route selection respects tenant policy constraints.
