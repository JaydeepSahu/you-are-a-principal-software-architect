# Logging

This page defines logging practices for the Enterprise AI Platform.

## Purpose

- Provide structured, searchable logs for platform operations and incidents.
- Make logs useful for debugging and compliance.

## Standards

- Use structured JSON logs with consistent fields.
- Include severity, timestamp, service name, tenant ID, and correlation ID.
- Avoid logging secrets or sensitive payload content.
- Capture error context and exception details.

## OpenTelemetry Integration

- Use OpenTelemetry-compatible log exporters when available.
- Keep log schema consistent across services.
- Use a central logging backend for indexing and searching logs.
