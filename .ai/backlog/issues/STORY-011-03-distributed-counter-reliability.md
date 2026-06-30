---
labels:
  - EPIC-011
  - Rate Limiting & Distributed Counters
  - story
---

# STORY-011-03: Distributed counter reliability

**Epic:** EPIC-011 — Rate Limiting & Distributed Counters

## Description

As a platform engineer, I want rate limiting counters to be reliable in Redis so that counts remain accurate under concurrent load.

## Acceptance Criteria

- Counter updates are atomic.
- Race conditions are prevented.
- Limits are enforced consistently.
