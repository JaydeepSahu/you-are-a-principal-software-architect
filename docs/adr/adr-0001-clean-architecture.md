# ADR-0001 Clean Architecture

## Status

Accepted

## Context

We need a structure that keeps domain logic independent from transport, persistence, and provider integrations.

## Decision

Use Clean Architecture with inward dependencies and bounded-context service slices.

## Consequences

- Core rules stay testable
- Infrastructure can change without rewriting business logic
- Each service owns its own composition root

