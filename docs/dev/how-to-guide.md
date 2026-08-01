# Enterprise AI Platform - How-To Developer & Operator Guide

Welcome to the **Enterprise AI Platform** How-To Guide. This guide covers step-by-step instructions for developers, platform engineers, security teams, and finance administrators to operate and consume the AI Control Plane.

---

## Table of Contents

1. [How to Run the Platform Locally (Docker Compose)](#1-how-to-run-the-platform-locally-docker-compose)
2. [How to Use the Interactive Web Playground & Admin UI](#2-how-to-use-the-interactive-web-playground--admin-ui)
3. [How to Use the Developer CLI (`ai-cli`)](#3-how-to-use-the-developer-cli-ai-cli)
4. [How to Build & Run Autonomous Agent Workflows (SDK)](#4-how-to-build--run-autonomous-agent-workflows-sdk)
5. [How to Query the Hybrid RAG Knowledge Engine](#5-how-to-query-the-hybrid-rag-knowledge-engine)
6. [How to Configure Inline Data Loss Prevention (PII & Secret Scanner)](#6-how-to-configure-inline-data-loss-prevention-pii--secret-scanner)
7. [How to Monitor Circuit Breakers & Failover Routing](#7-how-to-monitor-circuit-breakers--failover-routing)
8. [How to Inspect FinOps Departmental Chargeback Reports](#8-how-to-inspect-finops-departmental-chargeback-reports)
9. [How to Deploy to Production Kubernetes (Helm & AKS)](#9-how-to-deploy-to-production-kubernetes-helm--aks)

---

## 1. How to Run the Platform Locally (Docker Compose)

Spin up PostgreSQL 16, Redis 7, Prometheus, Grafana, OpenTelemetry Collector, AI Gateway, and Agent API:

# 1. Clone repository & navigate to root
cd path/to/you-are-a-principal-software-architect

# 2. BRun the entire platform (Infrastructure + APIs + Workers) locally using the new modular compose profiles:

```bash
# Recommended: Set parallel limit to prevent Docker DNS crashes during massive concurrent builds
export COMPOSE_PARALLEL_LIMIT=3 # Linux/macOS
$env:COMPOSE_PARALLEL_LIMIT=3   # Windows PowerShell

docker compose --profile infrastructure --profile api --profile workers up --build -d
```

Verify services are running:

```bash
docker compose --profile infrastructure --profile api --profile workers ps
```

**Service Endpoints**:
- **AI Gateway API**: `http://localhost:5000`
- **Agent API**: `http://localhost:5003`
- **Web Portal UI & Playground**: `http://localhost:5007`
- **Prometheus Metrics**: `http://localhost:9090`
- **Grafana Dashboards**: `http://localhost:3000` (User: `admin`, Pass: `admin`)

---

## 2. How to Use the Interactive Web Playground & Admin UI

1. Open your browser to `http://localhost:5007/`.
2. Navigate to the **Prompt Playground** tab.
3. Select target models to compare side-by-side (e.g. `Azure OpenAI GPT-4o`, `Claude 3.5 Sonnet`, `Gemini 1.5 Pro`, `Self-Hosted DeepSeek-Coder`).
4. Type your prompt in the **Prompt Canvas** and click **Run Comparison**.
5. Inspect side-by-side cards showing:
   - Generated Response Text
   - Execution Latency (ms)
   - Token Count (Prompt + Completion)
   - Estimated Cost ($)
   - Quality Score

---

## 3. How to Use the Developer CLI (`ai-cli`)

The CLI tool allows terminal-first prompt execution, agent workflow execution, RAG search, and status checks:

```powershell
# Execute quick prompt against AI Gateway with failover protection
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- prompt "Explain Clean Architecture invariants"

# Run autonomous agent task
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- agent run "Refactor payment module"

# Search RAG knowledge base
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- rag search "coding standards"

# Inspect Gateway health, circuit breakers, and remaining token budget
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- status

# List available AI models and GPU cluster nodes
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- models
```

---

## 4. How to Build & Run Autonomous Agent Workflows (SDK)

Use `EnterpriseAiPlatform.Agents.Sdk` in your C# service:

```csharp
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Sdk.Builder;
using EnterpriseAiPlatform.SharedKernel;

var client = new AgentBuilder("code-reviewer-agent")
    .AddTool("static_analysis", "Runs static analysis tool", async (argsJson, ct) =>
    {
        return "Static analysis completed: 0 errors, 0 warnings.";
    })
    .AddTool("security_audit", "Scans for vulnerable dependencies", async (argsJson, ct) =>
    {
        return "Security audit passed clean.";
    }, requiresApproval: true) // Human-in-the-loop approval gated step
    .Build();

var tenantId = TenantId.From("tenant-engineering");
var result = await client.RunAsync(tenantId, "Perform full pull request review");

if (result.IsSuccess)
{
    Console.WriteLine($"Agent status: {result.Value.Status}");
}
```

---

## 5. How to Query the Hybrid RAG Knowledge Engine

Execute hybrid vector/keyword search with Reciprocal Rank Fusion (RRF):

```csharp
using EnterpriseAiPlatform.Knowledge.Infrastructure.Rag;
using EnterpriseAiPlatform.SharedKernel;

var ragEngine = new HybridRagEngine();
var tenantId = TenantId.From("tenant-docs");

// Ingest Document
await ragEngine.IngestDocumentAsync(
    tenantId,
    "Architecture Guidelines",
    "docs/architecture.md",
    "Clean architecture enforces inner domain boundaries with zero external infrastructure dependencies."
);

// Search with RRF reranking
var searchResult = await ragEngine.HybridSearchAsync(tenantId, "domain boundary dependencies");

foreach (var hit in searchResult.Value)
{
    Console.WriteLine($"[RRF Score: {hit.CombinedScore:F3}] {hit.TextContent}");
}
```

---

## 6. How to Configure Inline Data Loss Prevention (PII & Secret Scanner)

Scan and mask prompt payloads before they leave the enterprise network:

```csharp
using EnterpriseAiPlatform.Policy.Infrastructure.Dlp;
using EnterpriseAiPlatform.SharedKernel;

var scanner = new InlineDlpScanner();
var tenantId = TenantId.From("tenant-security");

string promptPayload = "Connect using key AKIAIOSFODNN7EXAMPLE and email dev@enterprise.com";
var result = await scanner.ScanAndRedactAsync(tenantId, promptPayload);

Console.WriteLine($"Sanitized Text: {result.Value.SanitizedText}");
// Output: "Connect using key [REDACTED_AWS_KEY] and email [REDACTED_EMAIL]"
```

---

## 7. How to Monitor Circuit Breakers & Failover Routing

Inspect provider circuit breaker state via REST API:

```http
GET /api/v1/gateway/circuitbreakers
Host: localhost:5000
```

**Response**:
```json
[
  {
    "providerName": "AzureOpenAi",
    "state": "Closed",
    "failureRatePercentage": 0.0,
    "primaryFallbackProvider": "Anthropic"
  },
  {
    "providerName": "Anthropic",
    "state": "Closed",
    "failureRatePercentage": 0.0,
    "primaryFallbackProvider": "GoogleGemini"
  }
]
```

---

## 8. How to Inspect FinOps Departmental Chargeback Reports

Retrieve departmental cost chargebacks via REST API:

```http
GET /api/v1/cost/chargeback
Host: localhost:5000
X-Tenant-Id: tenant-enterprise-eng
```

**Response**:
```json
[
  {
    "departmentId": "dept-eng",
    "departmentName": "Software Engineering",
    "costCenter": "CC-101",
    "consumedTokens": 450000000,
    "allocatedCostDollars": 22500.00,
    "percentageOfTotalBudget": 65.6
  }
]
```

---

## 9. How to Deploy to Production Kubernetes (Helm & AKS)

```bash
# 1. Authenticate to Azure & AKS Cluster
az aks get-credentials --resource-group rg-enterprise-ai-prod --name aks-enterprise-ai-prod

# 2. Create production namespace
kubectl create namespace enterprise-ai-platform

# 3. Upgrade or Install Helm Chart
helm upgrade --install enterprise-ai ./deployments/helm/enterprise-ai-platform \
  --namespace enterprise-ai-platform \
  --values ./deployments/helm/enterprise-ai-platform/values.yaml \
  --set image.repository=acrenterpriseaiprod.azurecr.io/enterprise-ai/aigateway-api \
  --set image.tag=v1.0.0
```
