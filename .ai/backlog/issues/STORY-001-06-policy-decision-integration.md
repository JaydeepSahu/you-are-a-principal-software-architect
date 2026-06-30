---
labels:
  - EPIC-001
  - AI API Gateway
  - story
---

# STORY-001-06: Policy decision integration

**Epic:** EPIC-001 — AI API Gateway

## Description

As a governance engine, I want the gateway to call the Policy Service before forwarding requests so that policy is enforced at the edge.

## Acceptance Criteria

- Gateway sends request attributes to Policy Service.
- Policy allow/deny decisions are respected.
- Denied requests return appropriate problem responses.
