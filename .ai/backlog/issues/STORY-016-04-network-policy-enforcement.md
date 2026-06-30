---
labels:
  - EPIC-016
  - Security & Threat Protection
  - story
---

# STORY-016-04: Network policy enforcement

**Epic:** EPIC-016 — Security & Threat Protection

## Description

As a platform operator, I want network policies in Kubernetes so that service-to-service communication is restricted to approved paths.

## Acceptance Criteria

- Network policies restrict traffic by namespace and pod labels.
- Unauthorized flows are blocked.
- Policies are tested.
