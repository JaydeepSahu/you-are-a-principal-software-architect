# Day-2 Operations & Observability

Managing a massive Enterprise AI Platform requires comprehensive observability and strict operational governance.

## 1. FinOps & Cost Optimization
LLM inferences are incredibly expensive. The `CostOptimization-API` governs the financial impact of the platform:
- **Metering**: Every request routed through the AI Gateway is metered (prompt tokens, completion tokens, latency, and chosen model).
- **Budgets & Quotas**: Soft and Hard quotas are enforced per `TenantId`. If a department breaches their budget, requests are blocked or downgraded.
- **Chargeback**: Reports are generated automatically to track consumption by internal engineering teams.

## 2. Metrics & OpenTelemetry
- All 19 microservices are instrumented with OpenTelemetry (`EnterpriseAiPlatform.ServiceDefaults`).
- Metrics and traces are scraped by the **OpenTelemetry Collector** and routed to **Prometheus**.
- **Grafana** (accessible at `http://localhost:3000`) visualizes the Golden Signals: RPS, Error Rates, and Latency Percentiles ($p95$, $p99$).

## 3. Operational Runbooks
If incidents occur (e.g. crashing Minimal APIs, DI captive dependencies, Docker DNS lookups dropping), engineers must consult the [Advanced Troubleshooting Guide](../troubleshooting/troubleshooting-guide.md).