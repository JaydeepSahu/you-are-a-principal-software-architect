---
labels:
  - EPIC-011
  - Rate Limiting & Distributed Counters
  - story
---

# STORY-011-04: Idempotency key support

**Epic:** EPIC-011 — Rate Limiting & Distributed Counters

## Description

As a caller, I want idempotency keys supported so that retrying requests does not cause duplicate execution.

## Acceptance Criteria

- Idempotency keys can be submitted with requests.
- Duplicate requests return the same result.
- Idempotency data expires appropriately.
