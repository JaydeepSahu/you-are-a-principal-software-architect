# Docker Compose Orchestration

This page defines the modular Docker Compose architecture for the Enterprise AI Platform.

## Modular Strategy

Due to the massive scale of the Enterprise AI Platform (19+ microservices), a single monolithic `docker-compose.yml` file is difficult to maintain and overwhelms the Docker Desktop engine during simultaneous builds.

Instead, we use a **modular compose strategy** with `include` statements and **profiles**.

### Compose Files
The orchestration is split into logical domains:
- `docker-compose.yml`: The main entrypoint that includes all other files.
- `docker-compose.infrastructure.yml`: Core dependencies (PostgreSQL, Redis, RabbitMQ).
- `docker-compose.api.yml`: All frontend and backend APIs (Gateway, Identity, Governance, Agents).
- `docker-compose.workers.yml`: Asynchronous background workers and event processors.
- `docker-compose.monitoring.yml`: Observability stack (Prometheus, Grafana, OpenTelemetry Collector).
- `docker-compose.test.yml`: Transient containers for integration testing.

## Profiles

We leverage Docker Compose Profiles to allow developers to spin up only the domains they need.

| Profile Name | Description | Command |
|--------------|-------------|---------|
| `infrastructure` | Starts databases and message queues. | `docker compose --profile infrastructure up -d` |
| `api` | Starts all APIs (requires `infrastructure`). | `docker compose --profile infrastructure --profile api up -d` |
| `workers` | Starts background workers. | `docker compose --profile infrastructure --profile workers up -d` |
| `monitoring` | Starts the observability stack. | `docker compose --profile monitoring up -d` |

## Best Practices & Recommendations

- **Use Parallel Limits**: When building the entire stack for the first time, limit parallel builds to prevent internal Docker DNS resolver crashes (`dial tcp: lookup auth.docker.io: no such host`).
  - *Windows*: `$env:COMPOSE_PARALLEL_LIMIT=3`
  - *Linux/macOS*: `export COMPOSE_PARALLEL_LIMIT=3`
- **Use Environment Variables**: Configuration is passed via `.env` files. Ensure `POSTGRES_PASSWORD` and `PLATFORM_JWT_SIGNING_KEY` are set.
- **Targeted Rebuilds**: If modifying a single service, rebuild only that service to save time: `docker compose --profile api up -d --build aigateway-api`.
