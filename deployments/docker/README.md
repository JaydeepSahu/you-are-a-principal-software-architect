# Enterprise AI Platform — Docker Compose Setup

Production-grade, modular Docker Compose configuration for the Enterprise AI Platform.

## Quick Start

From the **repository root**:

```bash
# Copy environment template and edit secrets
cp .env.example .env

# Start everything
docker compose --profile infrastructure --profile api --profile monitoring --profile workers up -d
```

## Profiles

Services are grouped into profiles for selective startup:

| Profile          | Services                                         |
| ---------------- | ------------------------------------------------ |
| `infrastructure` | PostgreSQL, Redis, OpenTelemetry Collector, RabbitMQ |
| `api`            | All 18 API/host microservices                    |
| `monitoring`     | Prometheus, Grafana                              |
| `workers`        | Background Workers Host                          |

```bash
# Infrastructure only (databases, message broker, telemetry)
docker compose --profile infrastructure up -d

# Infrastructure + API services
docker compose --profile infrastructure --profile api up -d

# Everything
docker compose --profile infrastructure --profile api --profile monitoring --profile workers up -d
```

## Modular Compose Files

Instead of profiles, you can compose files explicitly:

```bash
cd deployments/docker

# Infrastructure + services
docker compose \
  -f docker-compose.yml \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.services.yml up -d

# Add monitoring
docker compose \
  -f docker-compose.yml \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.services.yml \
  -f docker-compose.monitoring.yml up -d

# Add development port overrides
docker compose \
  -f docker-compose.yml \
  -f docker-compose.infrastructure.yml \
  -f docker-compose.services.yml \
  -f docker-compose.override.yml up -d
```

## File Structure

```
deployments/docker/
├── docker-compose.yml                 # Base: YAML anchors, networks, volumes
├── docker-compose.infrastructure.yml  # PostgreSQL, Redis, OTel, RabbitMQ
├── docker-compose.services.yml        # All API microservices (18 services)
├── docker-compose.monitoring.yml      # Prometheus, Grafana
├── docker-compose.workers.yml         # Background Workers
├── docker-compose.override.yml        # Dev: re-publishes all internal ports
├── .env.example                       # Environment variable template
└── README.md                          # This file
```

## Published Ports (Production)

Only externally-facing services publish ports:

| Service           | Port  | Purpose              |
| ----------------- | ----- | -------------------- |
| AI Gateway        | 5111  | Public API entry     |
| Grafana           | 3000  | Monitoring dashboard |
| Prometheus        | 9090  | Metrics              |
| RabbitMQ Mgmt     | 15672 | Broker management UI |

All other services communicate internally via the `enterprise-ai-network` Docker network.

## Development Overrides

The `docker-compose.override.yml` file re-publishes all internal service ports for local debugging:

| Service                | Dev Port |
| ---------------------- | -------- |
| PostgreSQL             | 5432     |
| Redis                  | 6379     |
| OTel Collector (gRPC)  | 4317     |
| OTel Collector (HTTP)  | 4318     |
| Identity API           | 5277     |
| Policy API             | 5014     |
| Agents API             | 5090     |
| Audit API              | 5148     |
| Cost Optimization API  | 5040     |
| Evaluation API         | 50856    |
| Knowledge API          | 5177     |
| Local Model API        | 59923    |
| Metering API           | 5132     |
| Model Registry API     | 5235     |
| Observability API      | 5158     |
| Portal BFF API         | 5276     |
| Prompt Intelligence    | 52469    |
| Provider Adapters Host | 5167     |
| Routing API            | 5261     |
| Semantic Cache API     | 50857    |
| Vector Search API      | 5188     |

## Scaling

Because `container_name` has been removed from all services, horizontal scaling works:

```bash
docker compose --profile infrastructure --profile api up -d --scale routing-api=4
```

## CI/CD — Pre-built Images

Each service includes a commented-out `image:` directive for CI/CD pipelines:

```yaml
# CI/CD: Replace build block with pre-built image
# image: ${CONTAINER_REGISTRY}/enterprise-ai/gateway:${IMAGE_TAG:-latest}
build:
  context: ../..
  dockerfile: Dockerfile
  args: ...
```

Production workflow:

```
Build → Push to Registry → docker compose pull → docker compose up
```

Set `CONTAINER_REGISTRY` and `IMAGE_TAG` in your CI/CD environment, then swap `build:` for the `image:` line.

## Environment Variables

Copy `.env.example` to `.env` and set your secrets:

```bash
cp deployments/docker/.env.example .env
# Edit .env — never commit this file
```

| Variable                | Required | Default                                          |
| ----------------------- | -------- | ------------------------------------------------ |
| `POSTGRES_PASSWORD`     | ✅       | —                                                |
| `PLATFORM_JWT_SIGNING_KEY` | ✅    | —                                                |
| `POSTGRES_DB`           |          | `enterprise_ai_db`                               |
| `POSTGRES_USER`         |          | `ai_platform_user`                               |
| `ASPNETCORE_ENVIRONMENT`|          | `Development`                                    |
| `PLATFORM_JWT_ISSUER`   |          | `https://identity.enterprise-ai-platform.test`   |
| `PLATFORM_JWT_AUDIENCE` |          | `enterprise-ai-platform`                         |
| `RABBITMQ_USER`         |          | `guest`                                          |
| `RABBITMQ_PASSWORD`     |          | `guest`                                          |
| `GRAFANA_ADMIN_PASSWORD`|          | `admin`                                          |
| `CONTAINER_REGISTRY`    |          | —                                                |
| `IMAGE_TAG`             |          | `latest`                                         |
