# Getting Started with the Enterprise AI Platform

Welcome to the Enterprise AI Platform! This platform provides a centralized control plane for building governed, resilient, and observable AI products.

## Developer Onboarding Checklist

To get up and running quickly on your local machine, follow this standard progression:

1. **Prerequisites Verification**
   Ensure you have .NET 9, Docker Desktop, and Node.js v20+ installed. See the [Local Setup Guide](../dev/local-setup-and-user-guide.md) for exact version constraints.
2. **Clone and Configure Environment**
   Duplicate `.env.example` to `.env` and provide a secure `POSTGRES_PASSWORD` and `PLATFORM_JWT_SIGNING_KEY`.
3. **Spin Up the Local Stack**
   Use our modular Docker Compose profiles with parallel limits to safely boot the 19-container stack:
   ```bash
   export COMPOSE_PARALLEL_LIMIT=3
   docker compose --profile infrastructure --profile api --profile workers up -d --build
   ```
4. **Explore the Portals**
   - **Interactive Web UI**: Navigate to `http://localhost:5007` to access the Prompt Playground, compare models, and review Token Budgets.
   - **Observability Stack**: Navigate to `http://localhost:3000` (Grafana) to view live OpenTelemetry metrics.
5. **VS Code Extension**
   Open `clients/vscode/enterprise-ai-platform`, run `npm install`, and hit `F5` to interact with the platform natively within your IDE.

## Core Concepts

Before writing code, familiarize yourself with the platform's constraints:
- **Clean Architecture**: All 19 microservices adhere strictly to domain boundary isolation. Dependencies point *inward*.
- **Tenant Isolation**: Every API request and background worker operation requires a `TenantId`. Cross-tenant data sharing is blocked at the infrastructure boundary.
- **Circuit Breakers**: We do not hit AI providers (Azure, OpenAI, Anthropic) directly from application layers. All prompts traverse the **AI Gateway API**, which enforces routing, fallbacks, and DLP.

## Next Steps
Read the [How-To Guide](../dev/how-to-guide.md) for detailed CLI commands and C# SDK examples.