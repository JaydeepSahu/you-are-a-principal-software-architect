# Troubleshooting

Welcome to the Enterprise AI Platform Troubleshooting documentation. When developing a 19-container distributed service mesh locally, things can go wrong.

## Advanced Troubleshooting Runbook

If you encounter issues during builds, container startup, dependency injection mapping, or Minimal API routing, please refer to the dedicated runbook:

👉 **[Advanced Troubleshooting Guide](troubleshooting-guide.md)**

## General Debugging Tenets
1. **Always Check Health Endpoints**: Every service exposes a `/health/live` and `/health/ready` endpoint. Use `docker compose ps` to verify the container's health status.
2. **Read the Logs**: If a container exits with code 139 or crashes on boot, use `docker compose logs [service-name]` to inspect the exact stack trace.
3. **Verify Dependencies**: Often, failures are cascading. If the AI Gateway fails to boot, verify that RabbitMQ, PostgreSQL, and Redis are in a `Healthy` state first.