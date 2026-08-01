# Enterprise AI Platform

Enterprise AI Platform is a .NET 9 Clean Architecture workspace for building governed AI products and platform services.

## Quick Start

To spin up the entire 19-container platform locally, use the new modular Docker Compose profiles:

```bash
# Recommended: Set parallel limit to prevent Docker DNS crashes during massive concurrent builds
export COMPOSE_PARALLEL_LIMIT=3 # Linux/macOS
$env:COMPOSE_PARALLEL_LIMIT=3   # Windows PowerShell

# Build and start the infrastructure and all APIs
docker compose --profile infrastructure --profile api --profile workers up -d --build
```

## Documentation

- [Documentation Portal](docs/README.md)
- [Architecture Overview](docs/architecture/architecture-overview.md)
- [Folder Structure](docs/architecture/folder-structure.md)
- [API Guidelines](docs/api/api-guidelines.md)
- [Coding Standards](docs/architecture/coding-standards.md)
- [Deployment Guide](docs/deployment/deployment-guide.md)
- [Environment Variables Guide](docs/deployment/environment-variables.md)
- [Logging & Monitoring Guide](docs/deployment/logging-monitoring.md)
- [Security Guide](docs/security/security-guide.md)
- [Operational Runbooks](docs/operations/operational-runbooks.md)
- [Observability Guide](docs/observability/observability-guide.md)
- [Performance Guide](docs/performance/performance-guide.md)
- [Testing Strategy](docs/testing/testing-strategy.md)
- [CI/CD Quality Gates](docs/cicd/quality-gates.md)
- [Developer Onboarding](docs/dev/onboarding.md)
- [Local PC Setup & User Guide](docs/dev/local-setup-and-user-guide.md)
- [Comprehensive How-To Guide](docs/dev/how-to-guide.md)
- [Automated Documentation Generation](docs/automation/automated-docs-generation.md)



## ADRs

- [ADR-0001 Clean Architecture](docs/adr/adr-0001-clean-architecture.md)
- [ADR-0002 .NET 9](docs/adr/adr-0002-net-9.md)
- [ADR-0003 PostgreSQL](docs/adr/adr-0003-postgresql.md)
- [ADR-0004 Redis](docs/adr/adr-0004-redis.md)
- [ADR-0005 Docker](docs/adr/adr-0005-docker.md)

## API Documentation

- Native ASP.NET Core OpenAPI is enabled for runnable API projects
- Scalar provides the interactive docs UI
- JWT Bearer security is included in the OpenAPI document
- API versioning is configured for `v1` and future versions
- Documentation infrastructure is defined in `docs/api/api-guidelines.md`, `docs/adr`, `CHANGELOG.md`, `ROADMAP.md`, `CODE_OF_CONDUCT.md`, `CONTRIBUTING.md`, and `SECURITY.md`

## Documentation & API Standards

- OpenAPI and Scalar are the standard documentation stack for all runnable services.
- JWT Bearer authentication is documented in the OpenAPI schema.
- Health probe endpoints are required for every service: `/health/live` and `/health/ready`.
- ADRs capture major decisions for Clean Architecture, .NET 9, PostgreSQL, Redis, and Scalar documentation.
- `CODE_OF_CONDUCT.md` and `CONTRIBUTING.md` support team collaboration and review standards.
- `CHANGELOG.md` and `ROADMAP.md` track progress, milestones, and future API roadmap items.

## API Tests

- Bruno collection: `api-tests/health/bruno.json`

