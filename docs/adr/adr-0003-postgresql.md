# ADR-0003 PostgreSQL

## Status

Accepted

## Context

The platform requires a relational store with strong ecosystem support and reliable transactional behavior.

## Decision

Use PostgreSQL for persistence-backed service slices.

## Consequences

- Natural fit for EF Core
- Good support for JSON, indexing, and extensions
- Operational model is familiar for cloud deployments

