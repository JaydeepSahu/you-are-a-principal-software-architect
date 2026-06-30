---
labels:
  - EPIC-011
  - Rate Limiting & Distributed Counters
  - story
---

# STORY-011-01: Tenant scoped rate limits

**Epic:** EPIC-011 — Rate Limiting & Distributed Counters

## Description

As a platform admin, I want tenant-level rate limits so that each tenant has isolation from noisy neighbors.

## Acceptance Criteria

- Tenant-specific rate limit policies are supported.
- Excess tenant requests return 429.
- Rate limit state is stored in Redis.
