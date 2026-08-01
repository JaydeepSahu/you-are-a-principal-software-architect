# Deployment & Environments

This repository leverages modern containerization to ensure parity between local development and production environments.

## 1. Containerization (Alpine Linux)
All .NET 9 APIs and Workers are containerized using `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` as a base image.
- **Minimized Attack Surface**: Alpine Linux offers a significantly smaller footprint, reducing vulnerabilities.
- **Globalization Fix**: Standard Alpine images lack ICU libraries causing .NET 9 to crash on boot. Our unified `Dockerfile` explicitly installs `icu-libs` and `tzdata` to enable full globalization support.

## 2. Docker Compose Orchestration
The local service mesh is orchestrated via Docker Compose. To prevent internal DNS resolution crashes (e.g. `dial tcp: lookup auth.docker.io: no such host`) when building 19 containers, we utilize:
- **Parallel Limits**: `$env:COMPOSE_PARALLEL_LIMIT=3`.
- **Modular Profiles**: The monolith `docker-compose.yml` uses profiles (`infrastructure`, `api`, `workers`, `monitoring`) so developers only load the resources they need.

## 3. Production Readiness
- **Secrets Management**: Secrets (e.g., `POSTGRES_PASSWORD`) must be injected via environment variables or secret vaults. They are explicitly `.gitignore`d.
- **Telemetry**: All images inject OpenTelemetry instrumentation for downstream scraping into Prometheus/Grafana.