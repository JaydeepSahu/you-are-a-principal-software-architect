# Operational Runbooks

This runbook collection captures production operations guidance for the Enterprise AI Platform.

## Runbook Scope

- service deployment checks
- health probe behavior and remediation
- service restarts and rollback guidance
- capacity planning and scaling
- database and cache failover procedures
- incident response and escalation paths

## Health Check Operations

- Verify `/health/live` and `/health/ready` endpoints for every service.
- Confirm the health check reflects dependencies such as Redis, PostgreSQL, and policy services.
- Use container or orchestration probes to mark services healthy only when all critical dependencies are available.

## Service Restore

- Capture service logs and trace IDs before restart.
- Restart the failing service from the platform host or container orchestrator.
- Validate health endpoint and API readiness after restart.

## Troubleshooting

- Authentication failures: check JWT issuer, audiences, and token validation settings.
- API docs issues: confirm OpenAPI generation and Scalar UI availability.
- Configuration drift: compare deployed environment variables with documented values.

## Change Management

- Update this runbook when new service dependencies, host-level behaviors, or health probes are introduced.
- Communicate operational changes in release notes and the project roadmap.
