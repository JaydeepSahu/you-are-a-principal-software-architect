# Roadmap

## Current Foundation

- Documentation infrastructure
- API documentation infrastructure
- ADR baseline
- Bruno health collection
- Agent Framework (Planner, Executor, Memory, Tools, Parallel/Sequential, Retries, Approvals, Streaming, SDK)
- Observability Stack (OpenTelemetry Tracing, Metrics, Serilog Logging, Prometheus, Grafana, Health Probes)
- Production Deployment Stack (Docker, Docker Compose, Kubernetes, Helm v3, AKS, GitHub Actions CI/CD, HPA)
- Advanced Enterprise AI Infrastructure (Hybrid RAG, Multi-Agent Swarms, Model & Agent Marketplace, Evaluation & Fine-Tuning Pipeline, GPU Cluster Manager, AI Governance Portal BFF, VS Code Client)
- Interactive Web Playground & Admin UI (`PortalBff`)
- Inline Data Loss Prevention (PII & Secret Scanner in `EnterpriseAiPlatform.Policy`)
- Resilience & Circuit Breaker Engine in `EnterpriseAiPlatform.AiGateway`
- Developer CLI Tooling (`ai-cli` in `src/Tools/EnterpriseAiPlatform.Cli/`)
- Financial Predictability & Cost Allocation Engine (`EnterpriseAiPlatform.CostOptimization`)
- Centralized Dynamic Configuration & Options Validation (`src/BuildingBlocks/EnterpriseAiPlatform.Application.Abstractions/Configuration/`)
- Semantic Vector Response Cache (`src/BuildingBlocks/EnterpriseAiPlatform.Caching/`)
- LLM Red Teaming & Jailbreak Security Tester (`src/BuildingBlocks/EnterpriseAiPlatform.Security/`)
- Disaster Recovery & Multi-Region Failover Runbook (`docs/operations/disaster-recovery-runbook.md`)














## Next Milestones

- Expand OpenAPI grouping conventions across all feature modules
- Add per-service examples and request/response schemas
- Expand Bruno collections for each service slice

