# Security Architecture

Security is baked deeply into the Enterprise AI Platform to protect sensitive corporate data and enforce strict compliance.

## 1. Data Loss Prevention (DLP)
Before any prompt leaves the corporate network to hit a public LLM provider (like OpenAI or Anthropic), it passes through the Gateway's **DLP Scanner**.
- **PII Scrubbing**: Automatically detects and redacts emails, phone numbers, and addresses.
- **Secrets Management**: Detects leaked AWS keys, JWT tokens, and passwords, replacing them with `[REDACTED_AWS_KEY]` placeholders to prevent accidental leakage.

## 2. Tenant Isolation
Multi-tenancy is enforced uniformly across the ecosystem:
- The `IRequestContextAccessor` extracts the `TenantId` from the JWT Token.
- **Database Boundary**: Entity Framework Core Global Query Filters automatically scope all SQL queries to the active `TenantId`.
- **Cache Boundary**: Redis keys are prefixed automatically with the `TenantId` so tenants cannot cross-pollinate cache entries.

## 3. Red Teaming & Jailbreak Detection
The `Policy-API` enforces strict adversarial protection:
- Defends against known **Prompt Injection** techniques.
- Prevents System Prompt leakage.
- Continuously tested via the internal Red Teaming evaluation pipelines.