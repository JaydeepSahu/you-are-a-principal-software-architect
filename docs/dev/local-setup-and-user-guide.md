# Complete Local PC Setup & End-to-End User Guide

This guide provides step-by-step instructions for installing, running, and effectively using the complete **Enterprise AI Platform** on your local developer workstation.

---

## Table of Contents

1. [Prerequisites & System Requirements](#1-prerequisites--system-requirements)
2. [Step 1: Clone Repository & Spin Up Infrastructure](#step-1-clone-repository--spin-up-infrastructure)
3. [Step 2: Interactive Web Playground & Admin Portal](#step-2-interactive-web-playground--admin-portal)
4. [Step 3: Terminal-First Developer CLI (`ai-cli`)](#step-3-terminal-first-developer-cli-ai-cli)
5. [Step 4: Autonomous Agent Workflows (C# SDK)](#step-4-autonomous-agent-workflows-c-sdk)
6. [Step 5: Hybrid RAG Knowledge Search](#step-5-hybrid-rag-knowledge-search)
7. [Step 6: Inline DLP PII & Secret Redaction](#step-6-inline-dlp-pii--secret-redaction)
8. [Step 7: Resilience & Circuit Breaker Failover](#step-7-resilience--circuit-breaker-failover)
9. [Step 8: FinOps Cost Allocation & Chargeback](#step-8-finops-cost-allocation--chargeback)
10. [Step 9: Security Red Teaming & Jailbreak Testing](#step-9-security-red-teaming--jailbreak-testing)
11. [Step 10: Observability, Prometheus & Grafana](#step-10-observability-prometheus--grafana)
12. [Step 11: VS Code Client Extension](#step-11-vs-code-client-extension)

---

## 1. Prerequisites & System Requirements

Ensure the following tools are installed on your local workstation:

- **Operating System**: Windows 11 / Windows Server / macOS / Linux
- **.NET 9 SDK**: [Download .NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop**: [Download Docker Desktop](https://www.docker.com/products/docker-desktop/) (Ensure Docker Engine is running)
- **Node.js v20+** (Optional, for VS Code extension): [Download Node.js](https://nodejs.org/)

Verify installation in terminal:
```bash
dotnet --version   # Should output 9.0.xxx
docker --version   # Should output Docker version 24.x or higher
```

---

## Step 1: Clone Repository & Spin Up Infrastructure

1. Open terminal and navigate to the project directory:
   ```bash
   cd path/to/you-are-a-principal-software-architect
   ```

2. Build and launch all backend microservices and databases via Docker Compose:
   ```bash
   docker compose up --build -d
   ```

3. Verify all container services are running cleanly:
   ```bash
   docker compose ps
   ```

**Service Endpoints Table**:
| Service | Local Endpoint | Description |
|---|---|---|
| **Web Portal BFF & UI** | `http://localhost:5007` | Interactive Prompt Playground & Admin Control Plane |
| **AI Gateway API** | `http://localhost:5000` | Multi-model router, DLP scanner, Circuit Breaker |
| **Agent Framework API** | `http://localhost:5003` | Autonomous Agent execution engine |
| **Grafana Dashboard** | `http://localhost:3000` | OpenTelemetry Metrics & Golden Signals (User: `admin`, Pass: `admin`) |
| **Prometheus Metrics** | `http://localhost:9090` | OpenTelemetry metric scraper |

---

## Step 2: Interactive Web Playground & Admin Portal

Open your web browser and navigate to `http://localhost:5007`.

### 1. Concurrent Prompt Comparison
- Click the **Prompt Playground** tab.
- Check target models: `Azure OpenAI GPT-4o`, `Claude 3.5 Sonnet`, `Gemini 1.5 Pro`, `Self-Hosted DeepSeek (GPU)`.
- Enter prompt in the canvas (e.g. *"Design a Clean Architecture solution for order processing"*).
- Click **Run Comparison**. Inspect side-by-side quality, latency (ms), token usage, and cost ($).

### 2. Live Control Plane Analytics
- Click **Analytics & Metrics** tab to inspect total request volume, tokens consumed, p95 latency, and cost saved via local GPU model routing.

### 3. Model & Agent Marketplace
- Click **Model & Agent Catalog** to browse internal scorecards and benchmarks.

### 4. GPU Cluster Monitor
- Click **GPU Cluster Status** to view live VRAM load and active node compute allocations.

### 5. Departmental Token Budgets
- Click **Token Budgets** to inspect department monthly spend limits and soft/hard quota statuses.

---

## Step 3: Terminal-First Developer CLI (`ai-cli`)

Interact with the AI Control Plane directly from your terminal:

```powershell
# 1. Execute quick prompt against AI Gateway with failover protection
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- prompt "Explain Clean Architecture invariants"

# 2. Execute autonomous agent workflow task
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- agent run "Refactor payment processing module"

# 3. Search RAG knowledge base
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- rag search "coding standards"

# 4. Inspect Gateway health, circuit breaker states, and monthly token quota
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- status

# 5. List available AI models and GPU node cluster catalog
dotnet run --project .\src\Tools\EnterpriseAiPlatform.Cli\EnterpriseAiPlatform.Cli.csproj -- models
```

---

## Step 4: Autonomous Agent Workflows (C# SDK)

Create custom autonomous AI agents in your C# projects using `EnterpriseAiPlatform.Agents.Sdk`:

```csharp
using EnterpriseAiPlatform.Agents.Domain.Enums;
using EnterpriseAiPlatform.Agents.Sdk.Builder;
using EnterpriseAiPlatform.SharedKernel;

// Build Agent with custom tools and approval gates
var agent = new AgentBuilder("code-reviewer-agent")
    .AddTool("static_analysis", "Runs static analysis tool", async (argsJson, ct) =>
    {
        return "Static analysis completed: 0 errors, 0 warnings.";
    })
    .AddTool("security_audit", "Scans for vulnerable dependencies", async (argsJson, ct) =>
    {
        return "Security audit passed clean.";
    }, requiresApproval: true) // Human-in-the-loop gated step
    .Build();

var tenantId = TenantId.From("tenant-engineering");
var result = await agent.RunAsync(tenantId, "Perform full pull request review");

if (result.IsSuccess)
{
    Console.WriteLine($"Agent Status: {result.Value.Status}");
    foreach (var step in result.Value.Steps)
    {
        Console.WriteLine($" - Step: {step.StepName} -> {step.Output}");
    }
}
```

---

## Step 5: Hybrid RAG Knowledge Search

Execute hybrid dense vector + BM25 keyword search with Reciprocal Rank Fusion (RRF):

```csharp
using EnterpriseAiPlatform.Knowledge.Infrastructure.Rag;
using EnterpriseAiPlatform.SharedKernel;

var ragEngine = new HybridRagEngine();
var tenantId = TenantId.From("tenant-docs");

// Ingest document into knowledge store
await ragEngine.IngestDocumentAsync(
    tenantId,
    "Architecture Guidelines",
    "docs/architecture.md",
    "Clean architecture enforces inner domain boundary isolation with zero external infrastructure dependencies."
);

// Perform hybrid search with RRF reranking (k=60.0)
var searchResult = await ragEngine.HybridSearchAsync(tenantId, "domain boundary dependencies");

foreach (var hit in searchResult.Value)
{
    Console.WriteLine($"[RRF Score: {hit.CombinedScore:F3}] {hit.TextContent}");
}
```

---

## Step 6: Inline DLP PII & Secret Redaction

Test inline secret masking and PII redaction:

```powershell
# Send prompt containing AWS Key and Email address to DLP scanner endpoint
$body = @{ payload = "Connect using key AKIAIOSFODNN7EXAMPLE and email john.doe@company.com" } | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/policy/dlp/scan" -Method Post -Body $body -ContentType "application/json"
```

**Output**:
```json
{
  "originalText": "Connect using key AKIAIOSFODNN7EXAMPLE and email john.doe@company.com",
  "sanitizedText": "Connect using key [REDACTED_AWS_KEY] and email [REDACTED_EMAIL]",
  "shouldBlock": false,
  "matches": [ ... ]
}
```

---

## Step 7: Resilience & Circuit Breaker Failover

Inspect live provider circuit status and test automatic failover:

```powershell
# Inspect real-time circuit breaker states
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/gateway/circuitbreakers"
```

**Execute Resilient Request with Automatic Fallback**:
```powershell
$body = @{
    preferredProvider = "AzureOpenAi"
    modelId = "gpt-4o"
    promptPayload = "Generate refactoring plan"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/gateway/route" -Method Post -Body $body -ContentType "application/json"
```

---

## Step 8: FinOps Cost Allocation & Chargeback

Inspect monthly departmental cost chargebacks and run-rate spend forecasts:

```powershell
# Get departmental chargeback report
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/cost/chargeback" -Headers @{ "X-Tenant-Id" = "tenant-enterprise-eng" }

# Get spend forecast
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/cost/forecast" -Headers @{ "X-Tenant-Id" = "tenant-enterprise-eng" }
```

---

## Step 9: Security Red Teaming & Jailbreak Testing

Run automated adversarial security scans (Prompt Injections, System Prompt Leaks, Jailbreaks) against target models:

```powershell
$body = @{ targetModelId = "azure-gpt-4o" } | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v1/security/redteam/evaluate" -Method Post -Body $body -ContentType "application/json"
```

**Output**:
```json
{
  "targetModelId": "azure-gpt-4o",
  "totalVectorsTested": 4,
  "bypassesDetected": 0,
  "safetyScorecardPercentage": 100.0,
  "findings": [ ... ]
}
```

---

## Step 10: Observability, Prometheus & Grafana

1. Open Grafana at `http://localhost:3000` (User: `admin`, Pass: `admin`).
2. Open **Enterprise AI Platform - Executive & Operational Dashboard**.
3. View Golden Signals:
   - Request Volume (RPS)
   - Latency Percentiles ($p50$, $p95$, $p99$)
   - Token Metering & Cost Savings
   - Active Circuit Breakers & Policy Violations

---

## Step 11: VS Code Client Extension

1. Open VS Code and navigate to `clients/vscode/enterprise-ai-platform/`.
2. Run extension tests:
   ```bash
   npm run check
   npm test
   ```
3. Press `F5` in VS Code to launch Extension Development Host and interact with the AI Control Plane directly inside VS Code!
