---
labels:
  - EPIC-011
  - Rate Limiting & Distributed Counters
  - story
---

# STORY-011-02: Application and user quotas

**Epic:** EPIC-011 — Rate Limiting & Distributed Counters

## Description

As an operator, I want application and user quotas so that resources can be limited by finer-grained scope.

## Acceptance Criteria

- Application and user counters are maintained.
- Quotas can be configured per scope.
- Quota rejections include retry headers.
