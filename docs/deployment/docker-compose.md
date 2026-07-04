# Docker Compose

This page defines Docker Compose practices for the Enterprise AI Platform.

## Purpose

- Orchestrate local multi-service compositions.
- Enable developers to run a service mesh of locally containerized components.

## Standards

- Use `docker-compose.yml` for local development scenarios.
- Include only the services needed for local testing.
- Keep Compose files separate from production orchestration artifacts.

## Recommendations

- Use environment variable files for configuration.
- Start with minimal services and add dependencies as needed.
- Validate service health via exposed health endpoints.
