---
labels:
  - EPIC-003
  - Policy & Governance Service
  - story
---

# STORY-003-03: Policy decision API

**Epic:** EPIC-003 — Policy & Governance Service

## Description

As a gateway, I want to call the Policy Service for decision evaluation so that runtime requests are allowed or denied based on current policy.

## Acceptance Criteria

- Policy decision API accepts request attributes and returns allow/deny.
- Decision responses include reason codes and enforcement metadata.
- Policy caches can be invalidated on publish.
