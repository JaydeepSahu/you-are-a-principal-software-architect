# ADR-0005 Docker

## Status

Accepted

## Context

The platform needs a consistent, portable runtime environment for development, CI, and deployment.

## Decision

Use Docker to containerize services and support consistent build, test, and deployment workflows.

## Consequences

- Services become reproducible across environments
- Local development and CI deployments can share container images
- The platform aligns with cloud-native deployment patterns
