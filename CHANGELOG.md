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
- Advanced Enterprise AI Infrastructure: Delivered Hybrid Dense/Sparse RAG Engine (`HybridRagEngine`) with Reciprocal Rank Fusion (RRF), Multi-Agent Swarm Orchestrator (`AgentSwarmCoordinator`), Internal Model & Agent Marketplace (`ModelMarketplace`), LLM-as-a-Judge Evaluation Engine (`EvaluationEngine`), Fine-Tuning Pipeline (`FineTuningPipeline`), GPU Cluster Management (`GpuClusterManager`), AI Governance Portal BFF (`GovernancePortalEndpoints`), and VS Code Extension integration.
- Added Interactive Web Playground & Admin UI (`PortalBff`) featuring side-by-side prompt execution across models (Azure OpenAI, Claude 3.5, Gemini, DeepSeek), token budget monitoring, GPU metrics, and a dark-mode Single Page Application (SPA).
- Added Inline Data Loss Prevention (DLP) PII & Secret Scanner (`InlineDlpScanner`) in `EnterpriseAiPlatform.Policy` with high-throughput compiled regex rules (AWS Keys, Azure Keys, GitHub PATs, RSA Private Keys, Connection Strings, SSNs, Credit Cards, Emails), redaction masking, policy enforcement actions (`Mask`, `Block`, `Audit`), and REST endpoints (`/api/v1/policy/dlp/scan`).
- Added Resilience & Circuit Breaker Engine (`ProviderCircuitBreakerManager`) in `EnterpriseAiPlatform.AiGateway` featuring thread-safe circuit state machine tracking (`Closed`, `Open`, `HalfOpen`), automatic fallback provider failover routing (e.g. Azure OpenAI $\rightarrow$ Anthropic $\rightarrow$ Gemini $\rightarrow$ Self-Hosted vLLM), sliding window failure rates, and live status inspection endpoints (`/api/v1/gateway/circuitbreakers`).
- Added Developer CLI Tooling (`ai-cli`) in `src/Tools/EnterpriseAiPlatform.Cli/` supporting terminal-first prompt execution (`ai prompt`), autonomous agent workflow launching (`ai agent run`), hybrid RAG search (`ai rag search`), gateway status & quota checks (`ai status`), and model catalog inspection (`ai models`).
- Added Financial Predictability & Cost Allocation Engine (`CostAllocationEngine`) in `EnterpriseAiPlatform.CostOptimization` providing departmental cost chargeback reports (`/api/v1/cost/chargeback`), run-rate spend projections (`/api/v1/cost/forecast`), and token budget quota alerts (`/api/v1/cost/budgets`).
- Added comprehensive unit test suites covering Agent Framework, Observability, Advanced Infrastructure, Portal BFF, Policy DLP, AI Gateway Resilience, Developer CLI, and FinOps.










