# ADR-0004 Redis

## Status

Accepted

## Context

Caching, rate limiting, and distributed coordination need a low-latency shared store.

## Decision

Use Redis for cache and distributed state where needed.

## Consequences

- Faster repeated reads
- Shared resilience and quota state
- Extra operational dependency to manage

