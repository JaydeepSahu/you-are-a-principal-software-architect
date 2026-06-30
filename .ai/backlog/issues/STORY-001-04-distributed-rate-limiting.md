---
labels:
  - EPIC-001
  - AI API Gateway
  - story
---

# STORY-001-04: Distributed rate limiting

**Epic:** EPIC-001 — AI API Gateway

## Description

As a platform operator, I want the gateway to enforce tenant/application/user rate limits so that request volumes are protected and abuse is prevented.

## Acceptance Criteria

- Excess requests are rejected with 429 and retry headers.
- Rate-limit counters are stored in Redis.
- Metrics indicate rate-limit events.
