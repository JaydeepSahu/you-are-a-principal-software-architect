---
labels:
  - EPIC-011
  - Rate Limiting & Distributed Counters
  - story
---

# STORY-011-05: Rate-limit telemetry

**Epic:** EPIC-011 — Rate Limiting & Distributed Counters

## Description

As an SRE, I want rate-limit metrics so that throttling events can be monitored and analyzed.

## Acceptance Criteria

- Rate-limit counts are exported.
- Rejections are labeled by tenant/application.
- Throttle rates are queryable.
