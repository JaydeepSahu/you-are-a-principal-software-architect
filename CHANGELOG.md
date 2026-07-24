# Changelog

## Unreleased

- Added documentation foundation and API standards for the platform
- Added ADR templates and initial ADRs for Clean Architecture, .NET 9, PostgreSQL, Redis, and Scalar API documentation
- Added OpenAPI and Scalar documentation infrastructure
- Added JWT Bearer authentication support in OpenAPI documentation
- Implemented health check endpoint guidance for all services
- Added documentation portal, operational runbooks, observability, performance, testing, CI/CD, developer onboarding, and automation docs
- Added code of conduct and contributing guidelines
- Added roadmap and changelog tracking for future milestones
- Added Agent Framework bounded context (`EnterpriseAiPlatform.Agents`) with support for Planner, Executor, Memory, Tools, Parallel (`Task.WhenAll`) & Sequential execution, Retries with exponential backoff, Human-in-the-loop Approvals, Cancellation, SSE Event Streaming, and Developer Agent SDK.
- Added production Observability infrastructure (`EnterpriseAiPlatform.ServiceDefaults`) with OpenTelemetry Distributed Tracing, Metrics, Serilog Structured Logging, Correlation ID middleware, Prometheus scraping config, Grafana Dashboards, and Enterprise Health Probes (`/health/live`, `/health/ready`).
- Added full production Deployment stack including non-root multi-stage Dockerfiles (`Dockerfile.AiGateway`, `Dockerfile.AgentsApi`), Docker Compose environment (PostgreSQL 16, Redis 7, Prometheus, Grafana, OpenTelemetry Collector), Kubernetes manifests (Deployments, Services, ConfigMaps, Secret Templates, Ingress, HPA), Helm v3 Chart, AKS production deployment guide, and GitHub Actions CI/CD workflow with Trivy vulnerability scanning.
- Added unit test suites for Agent Framework and Observability.




